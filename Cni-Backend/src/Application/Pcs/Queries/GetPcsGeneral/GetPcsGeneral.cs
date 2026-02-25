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
            .Include(d => d.SpecificationTemplate)
            .Include(d => d.Rtu!).ThenInclude(r => r.Facility)
            .Where(d => d.Id == request.UnitId && d.DeviceType != null && d.DeviceType.Name == DeviceTypeNames.Pcs)
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

            if (parent?.DeviceType?.Name == DeviceTypeNames.EnergyStorage)
                return new PcsGeneralLinkedBessDto { Id = parent.Id, Name = parent.Name };
        }

        return await _context.Devices
            .Include(d => d.DeviceType)
            .Where(d => d.Rtu!.FacilityId == device.Rtu!.FacilityId
                && d.DeviceType != null
                && d.DeviceType.Name == DeviceTypeNames.EnergyStorage)
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
                tabs.Add(new TabInfoDto { Key = key, Label = label, IsEnabled = enabled, Order = order });
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
        var statusPayload = payloadsByKind.GetValueOrDefault("PcsStatusState");
        var powerPayload = payloadsByKind.GetValueOrDefault("PcsPowerState");
        var tempsPayload = payloadsByKind.GetValueOrDefault("PcsTempsState");
        var dcPayload = payloadsByKind.GetValueOrDefault("PcsDcMeasState");
        var acPayload = payloadsByKind.GetValueOrDefault("PcsAcMeasState");
        var chargePayload = payloadsByKind.GetValueOrDefault("PcsChargeAlgState");

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
                        ?? ExtractDouble(payload, "pcs_gridFreqHz"), "Hz"),
                StringField("phaseSequence", "Phase Sequence",
                    resolver?.ResolveString(ParameterKeys.PcsPhaseSequence)
                        ?? ExtractString(payload, "pcs_phaseSeq")),
                NumField("deratingCoefficient", "Derating Coefficient",
                    resolver?.ResolveNumber(ParameterKeys.PcsDeratingCoefficient)
                        ?? ExtractDouble(payload, "pcs_deratingCoef")),
                StringField("workingCondition", "Working Condition",
                    resolver?.ResolveString(ParameterKeys.PcsWorkingCondition)
                        ?? ExtractString(payload, "pcs_workCondition")),
                StringField("sourceMode", "Source Mode",
                    resolver?.ResolveString(ParameterKeys.PcsSourceMode)
                        ?? ExtractString(payload, "pcs_srcMode")),
                StringField("gridMode", "Grid Mode",
                    resolver?.ResolveString(ParameterKeys.PcsGridMode)
                        ?? MapGridMode(ExtractInt(payload, "pcs_gridMode"))),
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
                        ?? ExtractDouble(payload, "pcs_lineVoltAB"), "V"),
                GroupedNumField("lineVoltBC", "V(BC)", "Line Voltages",
                    resolver?.ResolveNumber(ParameterKeys.PcsLineVoltageBC)
                        ?? ExtractDouble(payload, "pcs_lineVoltBC"), "V"),
                GroupedNumField("lineVoltCA", "V(CA)", "Line Voltages",
                    resolver?.ResolveNumber(ParameterKeys.PcsLineVoltageCA)
                        ?? ExtractDouble(payload, "pcs_lineVoltCA"), "V"),

                // Phase Voltages
                GroupedNumField("phaseVoltA", "V(A)", "Phase Voltages",
                    resolver?.ResolveNumber(ParameterKeys.PcsPhaseVoltageA)
                        ?? ExtractDouble(payload, "pcs_phaseVoltA"), "V"),
                GroupedNumField("phaseVoltB", "V(B)", "Phase Voltages",
                    resolver?.ResolveNumber(ParameterKeys.PcsPhaseVoltageB)
                        ?? ExtractDouble(payload, "pcs_phaseVoltB"), "V"),
                GroupedNumField("phaseVoltC", "V(C)", "Phase Voltages",
                    resolver?.ResolveNumber(ParameterKeys.PcsPhaseVoltageC)
                        ?? ExtractDouble(payload, "pcs_phaseVoltC"), "V"),

                // Phase Currents
                GroupedNumField("phaseCurrentA", "I(A)", "Phase Currents",
                    resolver?.ResolveNumber(ParameterKeys.PcsPhaseCurrentA)
                        ?? ExtractDouble(payload, "pcs_phaseCurrA"), "A"),
                GroupedNumField("phaseCurrentB", "I(B)", "Phase Currents",
                    resolver?.ResolveNumber(ParameterKeys.PcsPhaseCurrentB)
                        ?? ExtractDouble(payload, "pcs_phaseCurrB"), "A"),
                GroupedNumField("phaseCurrentC", "I(C)", "Phase Currents",
                    resolver?.ResolveNumber(ParameterKeys.PcsPhaseCurrentC)
                        ?? ExtractDouble(payload, "pcs_phaseCurrC"), "A"),

                // Inductor Currents
                GroupedNumField("inductorCurrentA", "IL(A)", "Inductor Currents",
                    resolver?.ResolveNumber(ParameterKeys.PcsInductorCurrentA)
                        ?? ExtractDouble(payload, "pcs_inductCurrA"), "A"),
                GroupedNumField("inductorCurrentB", "IL(B)", "Inductor Currents",
                    resolver?.ResolveNumber(ParameterKeys.PcsInductorCurrentB)
                        ?? ExtractDouble(payload, "pcs_inductCurrB"), "A"),
                GroupedNumField("inductorCurrentC", "IL(C)", "Inductor Currents",
                    resolver?.ResolveNumber(ParameterKeys.PcsInductorCurrentC)
                        ?? ExtractDouble(payload, "pcs_inductCurrC"), "A"),
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
                        ?? ExtractDouble(payload, "pcs_totalDcBusV"), "V"),
                NumField("dcBusPos", "DC Bus (+)",
                    resolver?.ResolveNumber(ParameterKeys.PcsDcBusPosVoltage)
                        ?? ExtractDouble(payload, "pcs_dcBusPosV"), "V"),
                NumField("dcBusNeg", "DC Bus (-)",
                    resolver?.ResolveNumber(ParameterKeys.PcsDcBusNegVoltage)
                        ?? ExtractDouble(payload, "pcs_dcBusNegV"), "V"),
                NumField("batteryVoltage", "Battery Voltage",
                    resolver?.ResolveNumber(ParameterKeys.PcsBatteryVoltage)
                        ?? ExtractDouble(payload, "pcs_battVoltV"), "V"),
                NumField("batteryCurrent", "Battery Current",
                    resolver?.ResolveNumber(ParameterKeys.PcsBatteryCurrent)
                        ?? ExtractDouble(payload, "pcs_battCurrA"), "A"),
                NumField("dcPower", "DC Power",
                    resolver?.ResolveNumber(ParameterKeys.PcsDcPower)
                        ?? ExtractDouble(payload, "pcs_dcPwrKw"), "kW"),
                NumField("acPowerFactor", "AC Power Factor",
                    resolver?.ResolveNumber(ParameterKeys.PcsPowerFactor)
                        ?? ExtractDouble(payload, "pcs_acPwrFactor")),
                NumField("totalDcCurrent", "Total DC Current",
                    resolver?.ResolveNumber(ParameterKeys.PcsTotalDcCurrent)
                        ?? ExtractDouble(payload, "pcs_totalDcCurrA"), "A"),
            }
        };
    }

    private static DynamicSectionDto BuildPowerDistributionSection(JsonElement? payload, ParameterResolver? resolver)
    {
        var rows = new List<Dictionary<string, object?>>();

        // Phase A
        rows.Add(PowerRow("Phase A",
            resolver?.ResolveNumber(ParameterKeys.PcsActivePowerA) ?? ExtractDouble(payload, "pcs_actPwrKwA"),
            resolver?.ResolveNumber(ParameterKeys.PcsApparentPowerA) ?? ExtractDouble(payload, "pcs_appPwrKvaA"),
            resolver?.ResolveNumber(ParameterKeys.PcsReactivePowerA) ?? ExtractDouble(payload, "pcs_reaPwrKvarA")));

        // Phase B
        rows.Add(PowerRow("Phase B",
            resolver?.ResolveNumber(ParameterKeys.PcsActivePowerB) ?? ExtractDouble(payload, "pcs_actPwrKwB"),
            resolver?.ResolveNumber(ParameterKeys.PcsApparentPowerB) ?? ExtractDouble(payload, "pcs_appPwrKvaB"),
            resolver?.ResolveNumber(ParameterKeys.PcsReactivePowerB) ?? ExtractDouble(payload, "pcs_reaPwrKvarB")));

        // Phase C
        rows.Add(PowerRow("Phase C",
            resolver?.ResolveNumber(ParameterKeys.PcsActivePowerC) ?? ExtractDouble(payload, "pcs_actPwrKwC"),
            resolver?.ResolveNumber(ParameterKeys.PcsApparentPowerC) ?? ExtractDouble(payload, "pcs_appPwrKvaC"),
            resolver?.ResolveNumber(ParameterKeys.PcsReactivePowerC) ?? ExtractDouble(payload, "pcs_reaPwrKvarC")));

        // Total
        rows.Add(PowerRow("Total",
            resolver?.ResolveNumber(ParameterKeys.PcsActivePower) ?? ExtractDouble(payload, "pcs_outActPwrKw"),
            resolver?.ResolveNumber(ParameterKeys.PcsApparentPowerTotal) ?? ExtractDouble(payload, "pcs_outAppPwrKva"),
            resolver?.ResolveNumber(ParameterKeys.PcsReactivePower) ?? ExtractDouble(payload, "pcs_outReaPwrKvar")));

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
                        ?? ExtractDouble(payload, "pcs_igbtTempC"), "°C"),
                NumField("ambientTemp", "Ambient",
                    resolver?.ResolveNumber(ParameterKeys.PcsAmbientTemp)
                        ?? ExtractDouble(payload, "pcs_ambTempC"), "°C"),
                NumField("inductanceTemp", "Inductance",
                    resolver?.ResolveNumber(ParameterKeys.PcsInductanceTemp)
                        ?? ExtractDouble(payload, "pcs_inductTempC"), "°C"),
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
                        ?? ExtractDouble(payload, "pcs_ccCvRechargeV"), "V"),
                NumField("cvChargeV", "CV Charge V",
                    resolver?.ResolveNumber(ParameterKeys.PcsCvChargeV)
                        ?? ExtractDouble(payload, "pcs_cvChargeV"), "V"),
                NumField("cvMaxTime", "CV Max Time",
                    resolver?.ResolveNumber(ParameterKeys.PcsCvMaxTime)
                        ?? ExtractDouble(payload, "pcs_cvMaxTimeMin"), "min"),
                NumField("cvFloatI", "CV \u2192 Float I",
                    resolver?.ResolveNumber(ParameterKeys.PcsCvFloatI)
                        ?? ExtractDouble(payload, "pcs_cvFloatI"), "A"),
                NumField("floatChargeV", "Float Charge V",
                    resolver?.ResolveNumber(ParameterKeys.PcsFloatChargeV)
                        ?? ExtractDouble(payload, "pcs_floatChargeV"), "V"),
                NumField("floatCcI", "Float \u2192 CC I",
                    resolver?.ResolveNumber(ParameterKeys.PcsFloatCcI)
                        ?? ExtractDouble(payload, "pcs_floatCcI"), "A"),
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
