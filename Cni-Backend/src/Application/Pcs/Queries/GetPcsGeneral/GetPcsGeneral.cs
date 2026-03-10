using System.Text.Json;
using Backend.Application.Common.Interfaces;
using Backend.Application.Common.Models.Monitoring;
using Backend.Application.Common.Security;
using Backend.Application.Common.Services;
using Backend.Application.Common.Services.ParameterMapper;
using Backend.Domain.Constants;
using Microsoft.Extensions.Caching.Memory;

namespace Backend.Application.Pcs.Queries.GetPcsGeneral;

[Authorize(Roles = "Administrator,Tenant.Admin")]
[RequirePermission(Backend.Domain.Constants.Permissions.PcsView)]
public record GetPcsGeneralQuery : IRequest<PcsGeneralDto>
{
    public Guid UnitId { get; init; }
}

public class GetPcsGeneralQueryHandler : IRequestHandler<GetPcsGeneralQuery, PcsGeneralDto>
{
    private readonly IApplicationDbContext _context;
    private readonly IDeviceStatusService _statusService;
    private readonly IMemoryCache _cache;

    public GetPcsGeneralQueryHandler(IApplicationDbContext context, IDeviceStatusService statusService, IMemoryCache cache)
    {
        _context = context;
        _statusService = statusService;
        _cache = cache;
    }

    public async Task<PcsGeneralDto> Handle(GetPcsGeneralQuery request, CancellationToken cancellationToken)
    {
        var device = await _context.Devices
            .Include(d => d.DeviceType)
            .Include(d => d.LogicalDevice)
            .Include(d => d.SpecificationTemplate)
            .Include(d => d.Rtu!).ThenInclude(r => r.Facility)
            .Where(d => d.Id == request.UnitId &&
                ((d.DeviceType != null && d.DeviceType.Name == DeviceTypeNames.Pcs) ||
                 (d.LogicalDevice != null && d.LogicalDevice.Category == "PCS")))
            .FirstOrDefaultAsync(cancellationToken);

        Guard.Against.NotFound(request.UnitId, device);

        // Load latest telemetry per Kind
        var pcsEntries = await _context.DevicePcsData
            .Where(dd => dd.DeviceId == request.UnitId)
            .GroupBy(dd => dd.Kind)
            .Select(g => g.OrderByDescending(dd => dd.DeviceLocalTime).First())
            .ToListAsync(cancellationToken);

        var payloadsByKind = new Dictionary<string, JsonElement?>();
        foreach (var entry in pcsEntries)
        {
            if (entry.Kind != null)
                payloadsByKind[entry.Kind] = PayloadDeserializer.GetElement(entry.Payload);
        }

        var metadata = PayloadDeserializer.GetElement(device.Metadata);

        // Build ParameterResolver from template
        var parameterMap = ParameterMapParser.ParseCached(device.SpecificationTemplate?.ParameterMapTemplate, _cache);
        var resolver = parameterMap.Count > 0 && payloadsByKind.Count > 0
            ? new ParameterResolver(parameterMap, payloadsByKind, metadata)
            : null;

        // Status from active alarms
        var status = await _statusService.GetStatusAsync(device.Id, cancellationToken);
        var activeFaultCount = await _context.DeviceAlarms
            .CountAsync(a => a.DeviceId == device.Id && a.IsActive, cancellationToken);

        // Linked BESS: prefer parent if EnergyStorage, else fallback to facility
        var linkedBess = await ResolveLinkedBess(device, cancellationToken);

        // Available tabs from spec template
        var tabs = GetAvailableTabs(device.SpecificationTemplate?.SpecificationsTemplate);

        // Build general sections
        var sections = BuildSections(payloadsByKind, resolver);

        return new PcsGeneralDto
        {
            Id = device.Id,
            Name = device.Name,
            SerialNumber = device.SerialNumber,
            ModelName = device.Model,
            Manufacturer = device.Manufacturer,
            FacilityId = device.Rtu!.FacilityId,
            FacilityName = device.Rtu?.Facility?.Name,
            Status = StatusBadgeDto.FromStatus(status),
            LinkedBessUnit = linkedBess,
            AvailableTabs = tabs,
            ActiveFaultCount = activeFaultCount,
            LastSeenUtc = device.LastSeenUtc,
            Sections = sections
        };
    }

    // ── Linked BESS Resolution ──────────────────────────────────────────────

    private async Task<PcsGeneralLinkedBessDto?> ResolveLinkedBess(
        Domain.Entities.Device device, CancellationToken ct)
    {
        if (device.ParentDeviceId.HasValue)
        {
            var parent = await _context.Devices
                .Include(d => d.DeviceType)
                .FirstOrDefaultAsync(d => d.Id == device.ParentDeviceId.Value, ct);

            if (parent?.DeviceType?.Name == DeviceTypeNames.EnergyStorage ||
                parent?.LogicalDevice?.Category == "BESS")
                return new PcsGeneralLinkedBessDto { Id = parent.Id, Name = parent.Name };
        }

        return await _context.Devices
            .Include(d => d.DeviceType)
            .Include(d => d.LogicalDevice)
            .Where(d => d.Rtu!.FacilityId == device.Rtu!.FacilityId
                && ((d.DeviceType != null && d.DeviceType.Name == DeviceTypeNames.EnergyStorage) ||
                    (d.LogicalDevice != null && d.LogicalDevice.Category == "BESS")))
            .Select(d => new PcsGeneralLinkedBessDto { Id = d.Id, Name = d.Name })
            .FirstOrDefaultAsync(ct);
    }

    // ── Available Tabs ──────────────────────────────────────────────────────

    private static List<TabInfoDto> GetAvailableTabs(string? specifications)
    {
        var defaultTabs = new List<TabInfoDto>
        {
            new() { Key = "general", Label = "General", Order = 1 },
            new() { Key = "fault", Label = "Fault", Order = 2 },
            new() { Key = "curve", Label = "Curve", Order = 3 },
            new() { Key = "settings", Label = "Settings", Order = 4 }
        };

        if (string.IsNullOrWhiteSpace(specifications))
            return defaultTabs;

        var element = PayloadDeserializer.GetElement(specifications);
        if (!element.HasValue)
            return defaultTabs;

        if (element.Value.TryGetProperty("supportedTabs", out var tabsElement) &&
            tabsElement.ValueKind == JsonValueKind.Array)
        {
            var order = 0;
            var tabs = new List<TabInfoDto>();
            foreach (var tab in tabsElement.EnumerateArray())
            {
                order++;
                var key = tab.TryGetProperty("key", out var k) ? k.GetString() ?? "" : "";
                var label = tab.TryGetProperty("label", out var l) ? l.GetString() ?? key : key;
                var enabled = !tab.TryGetProperty("enabled", out var e) || e.GetBoolean();
                tabs.Add(new TabInfoDto { Key = key, Label = label, Enabled = enabled, Order = order });
            }
            return tabs.Count > 0 ? tabs : defaultTabs;
        }

        return defaultTabs;
    }

    // ── Section Builders ────────────────────────────────────────────────────

    private static List<DynamicSectionDto> BuildSections(
        Dictionary<string, JsonElement?> payloadsByKind,
        ParameterResolver? resolver)
    {
        var statusPayload = payloadsByKind.GetValueOrDefault("Pcs.Status");
        var powerPayload = payloadsByKind.GetValueOrDefault("Pcs.PowerDistribution");
        var tempsPayload = payloadsByKind.GetValueOrDefault("Pcs.Status");
        var dcPayload = payloadsByKind.GetValueOrDefault("Pcs.DcMeasurement");
        var acPayload = payloadsByKind.GetValueOrDefault("Pcs.AcMeasurement");
        var chargePayload = payloadsByKind.GetValueOrDefault("Pcs.ChargeAlgorithm");

        return new List<DynamicSectionDto>
        {
            BuildPcsStatusSection(statusPayload, resolver),
            BuildAcMeasurementsSection(acPayload, resolver),
            BuildDcMeasurementsSection(dcPayload, resolver),
            BuildPowerDistributionSection(powerPayload, resolver),
            BuildTemperaturesSection(tempsPayload, resolver),
            BuildChargeAlgorithmSection(chargePayload, resolver)
        };
    }

    private static DynamicSectionDto BuildPcsStatusSection(JsonElement? payload, ParameterResolver? resolver)
    {
        return new DynamicSectionDto
        {
            Key = "pcsStatus",
            Title = "PCS Status",
            Icon = "cpu",
            Columns = 3,
            Fields = new List<SectionFieldDto>
            {
                NumField("gridFrequency", "Grid Frequency",
                    resolver?.ResolveNumber(ParameterKeys.PcsGridFreq)
                        ?? ExtractDouble(payload, "pcsGridFreq"), "Hz"),
                StringField("phaseSequence", "Phase Sequence",
                    resolver?.ResolveString(ParameterKeys.PcsPhaseSequence)
                        ?? ExtractString(payload, "pcsPhaseSequence")),
                NumField("deratingCoefficient", "Derating Coefficient",
                    resolver?.ResolveNumber(ParameterKeys.PcsDeratingCoefficient)
                        ?? ExtractDouble(payload, "pcsDeratingCoefficient")),
                StringField("workingCondition", "Working Condition",
                    resolver?.ResolveString(ParameterKeys.PcsWorkingCondition)
                        ?? ExtractString(payload, "pcsWorkingCondition")),
                StringField("sourceMode", "Source Mode",
                    resolver?.ResolveString(ParameterKeys.PcsSourceMode)
                        ?? ExtractString(payload, "pcsSourceMode")),
                StringField("gridMode", "Grid Mode",
                    resolver?.ResolveString(ParameterKeys.PcsGridMode)
                        ?? MapGridMode(ExtractInt(payload, "pcsGridMode"))),
            }
        };
    }

    private static DynamicSectionDto BuildAcMeasurementsSection(JsonElement? payload, ParameterResolver? resolver)
    {
        return new DynamicSectionDto
        {
            Key = "acMeasurements",
            Title = "AC Measurements",
            Icon = "activity",
            Columns = 3,
            Fields = new List<SectionFieldDto>
            {
                // Line Voltages
                GroupedNumField("lineVoltAB", "V(AB)", "Line Voltages",
                    resolver?.ResolveNumber(ParameterKeys.PcsLineVoltageAB)
                        ?? ExtractDouble(payload, "pcsLineVoltageAB"), "V"),
                GroupedNumField("lineVoltBC", "V(BC)", "Line Voltages",
                    resolver?.ResolveNumber(ParameterKeys.PcsLineVoltageBC)
                        ?? ExtractDouble(payload, "pcsLineVoltageBC"), "V"),
                GroupedNumField("lineVoltCA", "V(CA)", "Line Voltages",
                    resolver?.ResolveNumber(ParameterKeys.PcsLineVoltageCA)
                        ?? ExtractDouble(payload, "pcsLineVoltageCA"), "V"),

                // Phase Voltages
                GroupedNumField("phaseVoltA", "V(A)", "Phase Voltages",
                    resolver?.ResolveNumber(ParameterKeys.PcsPhaseVoltageA)
                        ?? ExtractDouble(payload, "pcsPhaseVoltageA"), "V"),
                GroupedNumField("phaseVoltB", "V(B)", "Phase Voltages",
                    resolver?.ResolveNumber(ParameterKeys.PcsPhaseVoltageB)
                        ?? ExtractDouble(payload, "pcsPhaseVoltageB"), "V"),
                GroupedNumField("phaseVoltC", "V(C)", "Phase Voltages",
                    resolver?.ResolveNumber(ParameterKeys.PcsPhaseVoltageC)
                        ?? ExtractDouble(payload, "pcsPhaseVoltageC"), "V"),

                // Phase Currents
                GroupedNumField("phaseCurrentA", "I(A)", "Phase Currents",
                    resolver?.ResolveNumber(ParameterKeys.PcsPhaseCurrentA)
                        ?? ExtractDouble(payload, "pcsPhaseCurrentA"), "A"),
                GroupedNumField("phaseCurrentB", "I(B)", "Phase Currents",
                    resolver?.ResolveNumber(ParameterKeys.PcsPhaseCurrentB)
                        ?? ExtractDouble(payload, "pcsPhaseCurrentB"), "A"),
                GroupedNumField("phaseCurrentC", "I(C)", "Phase Currents",
                    resolver?.ResolveNumber(ParameterKeys.PcsPhaseCurrentC)
                        ?? ExtractDouble(payload, "pcsPhaseCurrentC"), "A"),

                // Inductor Currents
                GroupedNumField("inductorCurrentA", "IL(A)", "Inductor Currents",
                    resolver?.ResolveNumber(ParameterKeys.PcsInductorCurrentA)
                        ?? ExtractDouble(payload, "pcsInductorCurrentA"), "A"),
                GroupedNumField("inductorCurrentB", "IL(B)", "Inductor Currents",
                    resolver?.ResolveNumber(ParameterKeys.PcsInductorCurrentB)
                        ?? ExtractDouble(payload, "pcsInductorCurrentB"), "A"),
                GroupedNumField("inductorCurrentC", "IL(C)", "Inductor Currents",
                    resolver?.ResolveNumber(ParameterKeys.PcsInductorCurrentC)
                        ?? ExtractDouble(payload, "pcsInductorCurrentC"), "A"),
            }
        };
    }

    private static DynamicSectionDto BuildDcMeasurementsSection(JsonElement? payload, ParameterResolver? resolver)
    {
        return new DynamicSectionDto
        {
            Key = "dcMeasurements",
            Title = "DC Measurements",
            Icon = "gauge",
            Columns = 3,
            Fields = new List<SectionFieldDto>
            {
                NumField("totalDcBus", "Total DC Bus",
                    resolver?.ResolveNumber(ParameterKeys.PcsTotalDcBusVoltage)
                        ?? ExtractDouble(payload, "pcsTotalDcBusVoltage"), "V"),
                NumField("dcBusPos", "DC Bus (+)",
                    resolver?.ResolveNumber(ParameterKeys.PcsDcBusPosVoltage)
                        ?? ExtractDouble(payload, "pcsDcBusPosVoltage"), "V"),
                NumField("dcBusNeg", "DC Bus (-)",
                    resolver?.ResolveNumber(ParameterKeys.PcsDcBusNegVoltage)
                        ?? ExtractDouble(payload, "pcsDcBusNegVoltage"), "V"),
                NumField("batteryVoltage", "Battery Voltage",
                    resolver?.ResolveNumber(ParameterKeys.PcsBatteryVoltage)
                        ?? ExtractDouble(payload, "pcsBatteryVoltage"), "V"),
                NumField("batteryCurrent", "Battery Current",
                    resolver?.ResolveNumber(ParameterKeys.PcsBatteryCurrent)
                        ?? ExtractDouble(payload, "pcsBatteryCurrent"), "A"),
                NumField("dcPower", "DC Power",
                    resolver?.ResolveNumber(ParameterKeys.PcsDcPower)
                        ?? ExtractDouble(payload, "pcsDcPower"), "kW"),
                NumField("acPowerFactor", "AC Power Factor",
                    resolver?.ResolveNumber(ParameterKeys.PcsPowerFactor)
                        ?? ExtractDouble(payload, "pcsPowerFactor")),
                NumField("totalDcCurrent", "Total DC Current",
                    resolver?.ResolveNumber(ParameterKeys.PcsTotalDcCurrent)
                        ?? ExtractDouble(payload, "pcsTotalDcCurrent"), "A"),
            }
        };
    }

    private static DynamicSectionDto BuildPowerDistributionSection(JsonElement? payload, ParameterResolver? resolver)
    {
        var rows = new List<Dictionary<string, object?>>();

        // Phase A
        rows.Add(PowerRow("Phase A",
            resolver?.ResolveNumber(ParameterKeys.PcsActivePowerA) ?? ExtractDouble(payload, "pcsActivePowerA"),
            resolver?.ResolveNumber(ParameterKeys.PcsApparentPowerA) ?? ExtractDouble(payload, "pcsApparentPowerA"),
            resolver?.ResolveNumber(ParameterKeys.PcsReactivePowerA) ?? ExtractDouble(payload, "pcsReactivePowerA")));

        // Phase B
        rows.Add(PowerRow("Phase B",
            resolver?.ResolveNumber(ParameterKeys.PcsActivePowerB) ?? ExtractDouble(payload, "pcsActivePowerB"),
            resolver?.ResolveNumber(ParameterKeys.PcsApparentPowerB) ?? ExtractDouble(payload, "pcsApparentPowerB"),
            resolver?.ResolveNumber(ParameterKeys.PcsReactivePowerB) ?? ExtractDouble(payload, "pcsReactivePowerB")));

        // Phase C
        rows.Add(PowerRow("Phase C",
            resolver?.ResolveNumber(ParameterKeys.PcsActivePowerC) ?? ExtractDouble(payload, "pcsActivePowerC"),
            resolver?.ResolveNumber(ParameterKeys.PcsApparentPowerC) ?? ExtractDouble(payload, "pcsApparentPowerC"),
            resolver?.ResolveNumber(ParameterKeys.PcsReactivePowerC) ?? ExtractDouble(payload, "pcsReactivePowerC")));

        // Total
        rows.Add(PowerRow("Total",
            resolver?.ResolveNumber(ParameterKeys.PcsActivePower) ?? ExtractDouble(payload, "pcsActivePower"),
            resolver?.ResolveNumber(ParameterKeys.PcsApparentPowerTotal) ?? ExtractDouble(payload, "pcsApparentPowerTotal"),
            resolver?.ResolveNumber(ParameterKeys.PcsReactivePower) ?? ExtractDouble(payload, "pcsReactivePower")));

        return new DynamicSectionDto
        {
            Key = "powerDistribution",
            Title = "Power Distribution",
            Icon = "zap",
            Variant = "table",
            TableData = new TableDataDto
            {
                Columns = new List<TableColumnDto>
                {
                    new() { Key = "phase", Label = "Phase", Type = "string" },
                    new() { Key = "activeKw", Label = "Active (kW)", Type = "number" },
                    new() { Key = "apparentKva", Label = "Apparent (kVA)", Type = "number" },
                    new() { Key = "reactiveKvar", Label = "Reactive (kVar)", Type = "number" }
                },
                Rows = rows
            }
        };
    }

    private static DynamicSectionDto BuildTemperaturesSection(JsonElement? payload, ParameterResolver? resolver)
    {
        return new DynamicSectionDto
        {
            Key = "temperatures",
            Title = "Temperatures",
            Icon = "thermometer",
            Columns = 3,
            Fields = new List<SectionFieldDto>
            {
                NumField("igbtTemp", "IGBT",
                    resolver?.ResolveNumber(ParameterKeys.PcsIgbtTemp)
                        ?? ExtractDouble(payload, "pcsIgbtTemp"), "°C"),
                NumField("ambientTemp", "Ambient",
                    resolver?.ResolveNumber(ParameterKeys.PcsAmbientTemp)
                        ?? ExtractDouble(payload, "pcsAmbientTemp"), "°C"),
                NumField("inductanceTemp", "Inductance",
                    resolver?.ResolveNumber(ParameterKeys.PcsInductanceTemp)
                        ?? ExtractDouble(payload, "pcsInductanceTemp"), "°C"),
            }
        };
    }

    private static DynamicSectionDto BuildChargeAlgorithmSection(JsonElement? payload, ParameterResolver? resolver)
    {
        return new DynamicSectionDto
        {
            Key = "chargeAlgorithm",
            Title = "Charge Algorithm Parameters",
            Icon = "settings",
            Columns = 3,
            Fields = new List<SectionFieldDto>
            {
                NumField("ccCvRechargeV", "CC-CV Recharge V",
                    resolver?.ResolveNumber(ParameterKeys.PcsCcCvRechargeV)
                        ?? ExtractDouble(payload, "pcsCcCvRechargeV"), "V"),
                NumField("cvChargeV", "CV Charge V",
                    resolver?.ResolveNumber(ParameterKeys.PcsCvChargeV)
                        ?? ExtractDouble(payload, "pcsCvChargeV"), "V"),
                NumField("cvMaxTime", "CV Max Time",
                    resolver?.ResolveNumber(ParameterKeys.PcsCvMaxTime)
                        ?? ExtractDouble(payload, "pcsCvMaxTime"), "min"),
                NumField("cvFloatI", "CV \u2192 Float I",
                    resolver?.ResolveNumber(ParameterKeys.PcsCvFloatI)
                        ?? ExtractDouble(payload, "pcsCvFloatI"), "A"),
                NumField("floatChargeV", "Float Charge V",
                    resolver?.ResolveNumber(ParameterKeys.PcsFloatChargeV)
                        ?? ExtractDouble(payload, "pcsFloatChargeV"), "V"),
                NumField("floatCcI", "Float \u2192 CC I",
                    resolver?.ResolveNumber(ParameterKeys.PcsFloatCcI)
                        ?? ExtractDouble(payload, "pcsFloatCcI"), "A"),
            }
        };
    }

    // ── Helpers ──────────────────────────────────────────────────────────────

    private static Dictionary<string, object?> PowerRow(string phase, double? active, double? apparent, double? reactive) =>
        new()
        {
            ["phase"] = phase,
            ["activeKw"] = active,
            ["apparentKva"] = apparent,
            ["reactiveKvar"] = reactive
        };

    private static SectionFieldDto NumField(string key, string label, double? value, string? unit = null) =>
        new() { Key = key, Label = label, Value = value, Type = "number", Unit = unit };

    private static SectionFieldDto StringField(string key, string label, string? value) =>
        new() { Key = key, Label = label, Value = value, Type = "string" };

    private static SectionFieldDto GroupedNumField(string key, string label, string group, double? value, string? unit = null) =>
        new()
        {
            Key = key, Label = label, Value = value, Type = "number", Unit = unit,
            Meta = new Dictionary<string, object?> { ["group"] = group }
        };

    private static string? MapGridMode(int? code) => code switch
    {
        1 => "Grid Following",
        2 => "Grid Forming",
        3 => "Droop Control",
        4 => "Off-Grid",
        _ => null
    };

    private static double? ExtractDouble(JsonElement? element, string path)
    {
        if (!element.HasValue) return null;
        var value = PayloadDeserializer.ExtractValue(element.Value, path);
        return value switch
        {
            double d => d,
            int i => i,
            long l => l,
            _ => null
        };
    }

    private static int? ExtractInt(JsonElement? element, string path)
    {
        if (!element.HasValue) return null;
        var value = PayloadDeserializer.ExtractValue(element.Value, path);
        return value switch
        {
            int i => i,
            long l => (int)l,
            double d => (int)d,
            _ => null
        };
    }

    private static string? ExtractString(JsonElement? element, string path)
    {
        if (!element.HasValue) return null;
        return PayloadDeserializer.ExtractValue(element.Value, path) as string;
    }
}
