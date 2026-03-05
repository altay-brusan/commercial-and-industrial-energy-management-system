using System.Text.Json;
using Backend.Application.Common.Interfaces;
using Backend.Application.Common.Models.Monitoring;
using Backend.Application.Common.Security;
using Backend.Application.Common.Services;
using Backend.Application.Common.Services.ParameterMapper;
using Backend.Domain.Constants;
using Microsoft.Extensions.Caching.Memory;

namespace Backend.Application.BatteryStorage.Queries.GetBessSummary;

[Authorize(Roles = "Administrator,Tenant.Admin")]
[RequirePermission(Backend.Domain.Constants.Permissions.BessView)]
public record GetBessSummaryQuery : IRequest<SummaryResponseDto>
{
    public Guid UnitId { get; init; }
}

public class GetBessSummaryQueryHandler : IRequestHandler<GetBessSummaryQuery, SummaryResponseDto>
{
    private readonly IApplicationDbContext _context;
    private readonly ContainerTelemetryLoader _telemetryLoader;
    private readonly IMemoryCache _cache;

    public GetBessSummaryQueryHandler(IApplicationDbContext context, IMemoryCache cache)
    {
        _context = context;
        _telemetryLoader = new ContainerTelemetryLoader(context);
        _cache = cache;
    }

    public async Task<SummaryResponseDto> Handle(GetBessSummaryQuery request, CancellationToken cancellationToken)
    {
        var device = await _context.Devices
            .Include(d => d.DeviceType)
            .Include(d => d.LogicalDevice)
            .Include(d => d.SpecificationTemplate)
            .FirstOrDefaultAsync(d => d.Id == request.UnitId, cancellationToken);

        Guard.Against.NotFound(request.UnitId, device);

        var specsJson = device.SpecificationTemplate?.SpecificationsTemplate;

        // Check if Specifications defines custom summary sections
        var specs = PayloadDeserializer.GetElement(specsJson);
        if (specs.HasValue && specs.Value.TryGetProperty("tabs", out var tabs) &&
            tabs.TryGetProperty("summary", out var summaryTab) &&
            summaryTab.TryGetProperty("sections", out var sectionsElement) &&
            sectionsElement.ValueKind == JsonValueKind.Array)
        {
            // Specs-driven: load multi-table telemetry and merge payloads
            var telemetry = await _telemetryLoader.LoadLatest(request.UnitId, cancellationToken);
            var mergedPayloads = telemetry.GetPayloadsByKind();

            // Build a merged payload element from all Kinds for schema extraction
            var payload = mergedPayloads.Values.FirstOrDefault(p => p.HasValue);
            return new SummaryResponseDto { Sections = BuildFromSchema(sectionsElement, payload) };
        }

        // Default: build based on device type (or LogicalDevice category as fallback)
        var deviceTypeName = device.DeviceType?.Name;
        var logicalCategory = device.LogicalDevice?.Category;

        var sections = deviceTypeName switch
        {
            DeviceTypeNames.EnergyStorage => await BuildBatteryContainerSummary(device, cancellationToken),
            DeviceTypeNames.EnergyStorageRack or DeviceTypeNames.EnergyStoragePack or DeviceTypeNames.EnergyStorageModule =>
                await BuildSubDeviceSummary(device, cancellationToken),
            DeviceTypeNames.EnergyStorageController => await BuildSubDeviceSummary(device, cancellationToken),
            DeviceTypeNames.EnergyStorageTMS => await BuildTmsSummary(device, cancellationToken),
            _ when logicalCategory == "BESS" => await BuildBatteryContainerSummary(device, cancellationToken),
            _ => await BuildGenericSummary(device, cancellationToken)
        };

        return new SummaryResponseDto { Sections = sections };
    }

    // ── Battery Container Summary ──────────────────────────────────────────

    private async Task<List<DynamicSectionDto>> BuildBatteryContainerSummary(
        Domain.Entities.Device device, CancellationToken ct)
    {
        // Load multi-table telemetry for the container and all child devices
        var telemetry = await _telemetryLoader.LoadLatest(device.Id, ct);
        var payloads = telemetry.GetPayloadsByKind();

        var batt = payloads.GetValueOrDefault("ContainerBatteryState");
        var chiller = payloads.GetValueOrDefault("ChillerState");
        var hvcu = payloads.GetValueOrDefault("HVCUControlState");
        var metadata = PayloadDeserializer.GetElement(device.Metadata);

        // Build parameter resolver from template's ParameterMapTemplate (if present)
        var parameterMap = ParameterMapParser.ParseCached(device.SpecificationTemplate?.ParameterMapTemplate, _cache);
        var resolver = parameterMap.Count > 0
            ? new ParameterResolver(parameterMap, payloads, metadata)
            : null;

        var sections = new List<DynamicSectionDto>();

        // TODO: Move hierarchy counting to schema-driven approach. All summary fields
        // (including hierarchy) should be resolved via ParameterResolver using delegates
        // defined in the DeviceSpecificationTemplate's ParameterMapTemplate, instead of
        // being hardcoded per device type here.
        var hierarchyCounts = new List<(string Label, int Count)>();
        var rackCount = telemetry.Devices.Count(d => d.DeviceTypeName == DeviceTypeNames.EnergyStorageRack);
        var packCount = telemetry.Devices.Count(d => d.DeviceTypeName == DeviceTypeNames.EnergyStoragePack);
        var moduleCount = telemetry.Devices.Count(d => d.DeviceTypeName == DeviceTypeNames.EnergyStorageModule);
        if (rackCount > 0) hierarchyCounts.Add(("Rack", rackCount));
        if (packCount > 0) hierarchyCounts.Add(("Pack", packCount));
        if (moduleCount > 0) hierarchyCounts.Add(("Module", moduleCount));

        sections.Add(BuildNameplateSection(device, metadata, hierarchyCounts));
        sections.Add(BuildLiveStatusSection(batt, resolver));
        sections.Add(BuildEnergyLimitsSection(batt, resolver));
        sections.Add(BuildCellExtremesSection(batt, resolver));
        sections.Add(BuildContactorsSafetySection(batt));

        if (chiller.HasValue)
            sections.Add(BuildThermalManagementSection(chiller));

        if (hvcu.HasValue)
            sections.Add(BuildHvcuControlSection(hvcu));

        sections.Add(BuildHealthPreviewSection(batt));

        // Connected devices via DeviceLink
        var connectedSection = await BuildConnectedDevicesSection(device, ct);
        if (connectedSection != null)
            sections.Add(connectedSection);

        return sections;
    }

    private static DynamicSectionDto BuildNameplateSection(
        Domain.Entities.Device device, JsonElement? metadata,
        List<(string Label, int Count)>? hierarchyCounts = null)
    {
        var fields = new List<SectionFieldDto>
        {
            StringField("brand", "Brand", device.Manufacturer),
            StringField("model", "Model", device.Model),
            StringField("serialNumber", "Serial Number", device.SerialNumber),
        };

        if (metadata.HasValue)
        {
            var chemistry = PayloadDeserializer.ExtractValue(metadata.Value, "chemistry") as string;
            if (chemistry != null)
                fields.Add(StringField("chemistry", "Chemistry", chemistry));

            var commDate = PayloadDeserializer.ExtractValue(metadata.Value, "commissioningDate") as string;
            if (commDate != null)
                fields.Add(new SectionFieldDto { Key = "commissioningDate", Label = "Commissioning Date", Value = commDate, Type = "string", Format = "date" });

            var capacity = ExtractNum(metadata, "capacityKwh");
            if (capacity.HasValue)
                fields.Add(NumField("capacityKwh", "Capacity", capacity, "kWh"));

            var maxPower = ExtractNum(metadata, "maxPowerKw");
            if (maxPower.HasValue)
                fields.Add(NumField("maxPowerKw", "Max Power", maxPower, "kW"));

            var voltMin = ExtractNum(metadata, "voltageMin");
            var voltMax = ExtractNum(metadata, "voltageMax");
            if (voltMin.HasValue && voltMax.HasValue)
                fields.Add(new SectionFieldDto { Key = "voltageRange", Label = "Voltage Range", Value = $"{voltMin} – {voltMax}", Type = "string", Unit = "V" });
        }

        if (hierarchyCounts is { Count: > 0 })
        {
            var shortLabels = hierarchyCounts.Select(h =>
            {
                var abbr = h.Label switch
                {
                    "Rack" => "R",
                    "Pack" => "P",
                    "Module" => "M",
                    "Cell" => "C",
                    _ => h.Label[..1]
                };
                return $"{h.Count}{abbr}";
            });

            fields.Add(new SectionFieldDto
            {
                Key = "hierarchy",
                Label = "Hierarchy",
                Value = string.Join(" / ", shortLabels),
                Type = "string",
                Meta = new Dictionary<string, object?>
                {
                    ["detail"] = hierarchyCounts.Select(h => new Dictionary<string, object?>
                    {
                        ["label"] = h.Label,
                        ["count"] = h.Count
                    }).ToList()
                }
            });
        }

        return new DynamicSectionDto
        {
            Key = "nameplate",
            Title = "Nameplate Information",
            Icon = "info",
            Variant = "highlighted",
            Columns = 3,
            Fields = fields
        };
    }

    private static DynamicSectionDto BuildLiveStatusSection(JsonElement? batt, ParameterResolver? resolver = null)
    {
        var soc = resolver?.ResolveNumber(ParameterKeys.Soc) ?? ExtractNum(batt, "socPercent");
        var soh = resolver?.ResolveNumber(ParameterKeys.Soh) ?? ExtractNum(batt, "sohPercent");
        var voltage = resolver?.ResolveNumber(ParameterKeys.Voltage) ?? ExtractNum(batt, "totalVoltageV");
        var current = resolver?.ResolveNumber(ParameterKeys.Current) ?? ExtractNum(batt, "instantCurrentA");
        var power = resolver?.ResolveNumber(ParameterKeys.Power)
            ?? ((voltage.HasValue && current.HasValue)
                ? Math.Round(voltage.Value * current.Value / 1000.0, 1)
                : (double?)null);
        var state = resolver?.ResolveString(ParameterKeys.OperatingMode)
            ?? (batt.HasValue ? PayloadDeserializer.ExtractValue(batt.Value, "state") as string : null);
        var minTemp = resolver?.ResolveNumber(ParameterKeys.TempMin) ?? ExtractNum(batt, "minCellTempC");
        var maxTemp = resolver?.ResolveNumber(ParameterKeys.TempMax) ?? ExtractNum(batt, "maxCellTempC");
        var avgTemp = resolver?.ResolveNumber(ParameterKeys.TempAvg)
            ?? ((minTemp.HasValue && maxTemp.HasValue) ? Math.Round((minTemp.Value + maxTemp.Value) / 2.0, 1) : (double?)null);
        var freq = ExtractNum(batt, "frequency");

        var powerDirection = resolver?.ResolveString(ParameterKeys.PowerDirection)
            ?? ((current.HasValue && current.Value < 0) ? "discharging" : (current.HasValue && current.Value > 0) ? "charging" : "idle");

        return new DynamicSectionDto
        {
            Key = "liveStatus",
            Title = "Live Status",
            Icon = "activity",
            Variant = "card",
            Columns = 3,
            Fields = new List<SectionFieldDto>
            {
                new() { Key = "soc", Label = "State of Charge", Value = soc, Type = "number", Unit = "%", Format = "progress",
                    Meta = new Dictionary<string, object?> { ["thresholds"] = new[] {
                        new Dictionary<string, object?> { ["min"] = 60, ["color"] = "green" },
                        new Dictionary<string, object?> { ["min"] = 30, ["color"] = "orange" },
                        new Dictionary<string, object?> { ["min"] = 0, ["color"] = "red" }
                    }}},
                NumField("soh", "State of Health", soh, "%"),
                new() { Key = "power", Label = "Power", Value = power != null ? Math.Abs(power.Value) : null, Type = "number", Unit = "kW",
                    Meta = new Dictionary<string, object?> { ["direction"] = powerDirection }},
                NumField("voltage", "Voltage", voltage, "V"),
                NumField("current", "Current", current, "A"),
                NumField("frequency", "Frequency", freq, "Hz"),
                new() { Key = "tempMin", Label = "Temp Min", Value = minTemp, Type = "number", Unit = "\u00b0C", Format = "temperature" },
                new() { Key = "tempMax", Label = "Temp Max", Value = maxTemp, Type = "number", Unit = "\u00b0C", Format = "temperature" },
                new() { Key = "tempAvg", Label = "Temp Avg", Value = avgTemp, Type = "number", Unit = "\u00b0C", Format = "temperature" },
                new() { Key = "operatingMode", Label = "Operating Mode", Value = state?.ToLowerInvariant() switch {
                        "discharge" => "discharging", "charge" => "charging", _ => state?.ToLowerInvariant() ?? "unknown" },
                    Type = "string", Format = "badge",
                    Meta = StatusBadgeDto.FromOperatingMode(state) is var badge
                        ? new Dictionary<string, object?> { ["value"] = badge.Value, ["label"] = badge.Label, ["color"] = badge.Color }
                        : null }
            }
        };
    }

    private static DynamicSectionDto BuildEnergyLimitsSection(JsonElement? batt, ParameterResolver? resolver = null)
    {
        return new DynamicSectionDto
        {
            Key = "energyLimits",
            Title = "Energy & Limits",
            Icon = "gauge",
            Variant = "card",
            Columns = 3,
            Fields = new List<SectionFieldDto>
            {
                new() { Key = "remainingEnergy", Label = "Remaining Energy",
                    Value = resolver?.ResolveNumber(ParameterKeys.RemainingEnergy) ?? ExtractNum(batt, "remainingEnergyKwh"),
                    Type = "number", Unit = "kWh", Format = "progress",
                    Meta = new Dictionary<string, object?> { ["thresholds"] = new[] {
                        new Dictionary<string, object?> { ["min"] = 60, ["color"] = "green" },
                        new Dictionary<string, object?> { ["min"] = 30, ["color"] = "orange" },
                        new Dictionary<string, object?> { ["min"] = 0, ["color"] = "red" }
                    }}},
                NumField("availableEnergy", "Available Energy",
                    resolver?.ResolveNumber(ParameterKeys.AvailableEnergy) ?? ExtractNum(batt, "availableEnergyKwh"), "kWh"),
                NumField("dmccA", "DMCC", ExtractNum(batt, "dmccA"), "A"),
                NumField("dmdcA", "DMDC", ExtractNum(batt, "dmdcA"), "A"),
                NumField("outVoltPosV", "Output V (+)", ExtractNum(batt, "outVoltPosV"), "V"),
                NumField("outVoltNegV", "Output V (\u2212)", ExtractNum(batt, "outVoltNegV"), "V"),
                NumField("stateCode", "State Code", ExtractNum(batt, "stateCode"), ""),
            }
        };
    }

    private static DynamicSectionDto BuildCellExtremesSection(JsonElement? batt, ParameterResolver? resolver = null)
    {
        var maxV = resolver?.ResolveNumber(ParameterKeys.CellVoltageMax) ?? ExtractNum(batt, "maxCellVoltageMv");
        var minV = resolver?.ResolveNumber(ParameterKeys.CellVoltageMin) ?? ExtractNum(batt, "minCellVoltageMv");
        var avgV = resolver?.ResolveNumber(ParameterKeys.CellVoltageAvg) ?? ExtractNum(batt, "avgCellVoltageMv");
        var maxT = ExtractNum(batt, "maxCellTempC");
        var minT = ExtractNum(batt, "minCellTempC");
        var spread = resolver?.ResolveNumber(ParameterKeys.VoltageSpread)
            ?? ((maxV.HasValue && minV.HasValue) ? maxV.Value - minV.Value : (double?)null);

        return new DynamicSectionDto
        {
            Key = "cellExtremes",
            Title = "Cell Extremes",
            Icon = "bar-chart-3",
            Variant = "card",
            Columns = 3,
            Fields = new List<SectionFieldDto>
            {
                new() { Key = "cellVoltageMax", Label = "Max Cell Voltage", Value = maxV, Type = "number", Unit = "mV",
                    Meta = LocationMeta(batt, "maxCellVoltageBmuLoc", "maxCellVoltageCellLoc") },
                new() { Key = "cellVoltageMin", Label = "Min Cell Voltage", Value = minV, Type = "number", Unit = "mV",
                    Meta = LocationMeta(batt, "minCellVoltageBmuLoc", "minCellVoltageCellLoc") },
                NumField("avgCellVoltage", "Avg Cell Voltage", avgV, "mV"),
                new() { Key = "cellTempMax", Label = "Max Cell Temp", Value = maxT, Type = "number", Unit = "\u00b0C", Format = "temperature",
                    Meta = LocationMeta(batt, "maxCellTempBmuLoc", "maxCellTempCellLoc") },
                new() { Key = "cellTempMin", Label = "Min Cell Temp", Value = minT, Type = "number", Unit = "\u00b0C", Format = "temperature",
                    Meta = LocationMeta(batt, "minCellTempBmuLoc", "minCellTempCellLoc") },
                NumField("voltageSpread", "Voltage Spread", spread, "mV"),
            }
        };
    }

    private static DynamicSectionDto BuildContactorsSafetySection(JsonElement? batt)
    {
        return new DynamicSectionDto
        {
            Key = "contactorsSafety",
            Title = "Contactors & Safety",
            Icon = "shield",
            Variant = "card",
            Columns = 3,
            Fields = new List<SectionFieldDto>
            {
                BoolBadgeField("contactorPositive", "Positive Contactor", ExtractBool(batt, "contactFbkPos"),
                    "Closed", "green", "Open", "grey"),
                BoolBadgeField("contactorNegative", "Negative Contactor", ExtractBool(batt, "contactFbkNeg"),
                    "Closed", "green", "Open", "grey"),
                NumField("isolationResistance", "Isolation Resistance", ExtractNum(batt, "isolationStatus"), "k\u03a9"),
            }
        };
    }

    private static DynamicSectionDto BuildThermalManagementSection(JsonElement? chiller)
    {
        var modeStr = chiller.HasValue ? PayloadDeserializer.ExtractValue(chiller.Value, "operatingModeStr") as string : null;

        return new DynamicSectionDto
        {
            Key = "thermalManagement",
            Title = "Thermal Management (TMS)",
            Icon = "thermometer",
            Variant = "card",
            Columns = 3,
            Optional = true,
            Fields = new List<SectionFieldDto>
            {
                new() { Key = "tmsOperatingMode", Label = "Operating Mode", Value = modeStr, Type = "string", Format = "badge",
                    Meta = new Dictionary<string, object?> { ["badgeLabel"] = modeStr, ["badgeColor"] = "sky" } },
                NumField("coolantSupplyTemp", "Coolant Supply Temp", ExtractNum(chiller, "outletCoolantTempC"), "\u00b0C"),
                NumField("coolantReturnTemp", "Coolant Return Temp", ExtractNum(chiller, "inletCoolantTempC"), "\u00b0C"),
                NumField("coolantPressureIn", "Coolant Pressure In", ExtractNum(chiller, "inletCoolantPressure"), "bar"),
                NumField("coolantPressureOut", "Coolant Pressure Out", ExtractNum(chiller, "outletCoolantPressure"), "bar"),
                NumField("ambientTemp", "Ambient Temp", ExtractNum(chiller, "ambientTempC"), "\u00b0C"),
                BoolBadgeField("compressorStatus", "Compressor", ExtractBool(chiller, "compressorStatus"),
                    "Running", "green", "Off", "grey"),
                BoolBadgeField("pumpStatus", "Pump", ExtractBool(chiller, "pumpStatus"),
                    "Running", "green", "Off", "grey"),
                BoolBadgeField("fanStatus", "Fan", ExtractBool(chiller, "fanRunStatus"),
                    "Running", "green", "Off", "grey"),
                BoolBadgeField("electricHeater", "Electric Heater", ExtractBool(chiller, "electricHeaterStatus"),
                    "On", "orange", "Off", "grey"),
            }
        };
    }

    private static DynamicSectionDto BuildHvcuControlSection(JsonElement? hvcu)
    {
        var isolationCtrl = ExtractBool(hvcu, "pack1IsolationControl");

        // Build BCU table rows
        var rows = new List<Dictionary<string, object?>>();
        for (int i = 1; i <= 4; i++)
        {
            var stateCode = ExtractNum(hvcu, $"bcu0{i}ControlState");
            var stateStr = hvcu.HasValue ? PayloadDeserializer.ExtractValue(hvcu.Value, $"bcu0{i}StateStr") as string : null;

            if (stateStr == null && !stateCode.HasValue) continue;

            var badgeColor = stateStr?.ToLowerInvariant() switch
            {
                "active" or "discharge" or "charge" => "green",
                "fault" or "error" => "red",
                "n/a" or "off" => "grey",
                _ => "yellow"
            };

            rows.Add(new Dictionary<string, object?>
            {
                ["id"] = i,
                ["controlState"] = stateCode,
                ["stateStr"] = new Dictionary<string, object?>
                {
                    ["value"] = stateStr,
                    ["label"] = stateStr,
                    ["color"] = badgeColor
                }
            });
        }

        return new DynamicSectionDto
        {
            Key = "hvcuControl",
            Title = "HVCU Control",
            Icon = "cpu",
            Variant = "table",
            Optional = true,
            Fields = new List<SectionFieldDto>
            {
                BoolBadgeField("isolationControl", "Isolation Control", isolationCtrl,
                    "Active", "green", "Inactive", "grey"),
            },
            TableData = new TableDataDto
            {
                Columns = new List<TableColumnDto>
                {
                    new() { Key = "id", Label = "BCU #", Type = "number" },
                    new() { Key = "controlState", Label = "Control State", Type = "number" },
                    new() { Key = "stateStr", Label = "State", Type = "badge" },
                },
                Rows = rows
            }
        };
    }

    private static DynamicSectionDto BuildHealthPreviewSection(JsonElement? batt)
    {
        return new DynamicSectionDto
        {
            Key = "healthPreview",
            Title = "Health",
            Icon = "heart-pulse",
            Variant = "card",
            Columns = 3,
            Fields = new List<SectionFieldDto>
            {
                NumField("cycleCount", "Cycle Count", ExtractNum(batt, "cycleCount"), ""),
                NumField("roundTripEfficiency", "RTE", ExtractNum(batt, "roundTripEfficiency"), "%"),
                NumField("equivalentFullCycles", "Equiv. Full Cycles", ExtractNum(batt, "equivalentFullCycles"), ""),
            }
        };
    }

    private async Task<DynamicSectionDto?> BuildConnectedDevicesSection(
        Domain.Entities.Device device, CancellationToken ct)
    {
        // Find devices linked via DeviceLink in same facility
        var facilityId = device.Rtu?.FacilityId;
        if (!facilityId.HasValue) return null;

        var links = await _context.DeviceLinks
            .Where(dl => dl.FacilityId == facilityId.Value)
            .ToListAsync(ct);

        if (links.Count == 0) return null;

        // Find links referencing this device
        var deviceIdStr = device.Id.ToString();
        var connectedFields = new List<SectionFieldDto>();

        foreach (var link in links)
        {
            if (!link.ConnectedPorts.Contains(deviceIdStr)) continue;

            // Find the OTHER device in the link
            var ports = PayloadDeserializer.GetElement(link.ConnectedPorts);
            if (!ports.HasValue || ports.Value.ValueKind != JsonValueKind.Array) continue;

            foreach (var port in ports.Value.EnumerateArray())
            {
                var portStr = port.GetString();
                if (portStr == null || portStr.StartsWith(deviceIdStr)) continue;

                var otherDeviceId = portStr.Split('.').FirstOrDefault();
                if (otherDeviceId == null) continue;

                if (Guid.TryParse(otherDeviceId, out var otherId))
                {
                    var otherDevice = await _context.Devices
                        .Include(d => d.DeviceType)
                        .FirstOrDefaultAsync(d => d.Id == otherId, ct);

                    if (otherDevice != null)
                    {
                        var typeKey = otherDevice.DeviceType?.Name?.ToLowerInvariant() switch
                        {
                            "pcs" => "inverter",
                            "smartmeter" => "meter",
                            _ => otherDevice.DeviceType?.Name?.ToLowerInvariant() ?? "device"
                        };
                        connectedFields.Add(StringField(typeKey, otherDevice.DeviceType?.Name ?? "Device", otherDevice.Name));
                    }
                }
            }
        }

        if (connectedFields.Count == 0) return null;

        return new DynamicSectionDto
        {
            Key = "connectedDevices",
            Title = "Connected Devices",
            Icon = "cable",
            Variant = "card",
            Columns = 2,
            Fields = connectedFields
        };
    }

    // ── Sub-device (Rack, Pack, Module, ESC) Summary ──────────────────────

    private async Task<List<DynamicSectionDto>> BuildSubDeviceSummary(
        Domain.Entities.Device device, CancellationToken ct)
    {
        // Walk up parent chain to find root container and inherit template
        var rootId = device.Id;
        var currentId = device.ParentDeviceId;
        var inheritedTemplate = device.SpecificationTemplate;

        while (currentId.HasValue)
        {
            var parent = await _context.Devices
                .Include(d => d.DeviceType)
                .Include(d => d.SpecificationTemplate)
                .FirstOrDefaultAsync(d => d.Id == currentId.Value, ct);

            if (parent == null) break;

            inheritedTemplate ??= parent.SpecificationTemplate;
            rootId = parent.Id;
            currentId = parent.ParentDeviceId;
        }

        // Load telemetry from root container (all data is stored there)
        var telemetry = await _telemetryLoader.LoadLatest(rootId, ct);
        var payloads = telemetry.GetPayloadsByKind();
        var batt = payloads.GetValueOrDefault("ContainerBatteryState");
        var metadata = PayloadDeserializer.GetElement(device.Metadata);

        // Build parameter resolver from inherited template
        var parameterMap = ParameterMapParser.ParseCached(inheritedTemplate?.ParameterMapTemplate, _cache);
        var resolver = parameterMap.Count > 0
            ? new ParameterResolver(parameterMap, payloads, metadata)
            : null;

        var deviceTypeName = device.DeviceType?.Name;
        var sections = new List<DynamicSectionDto>();

        // SoC progress bar
        sections.Add(BuildSocSection(resolver, batt));

        // Electrical section: SoH, Voltage, Current, Power
        sections.Add(BuildElectricalSection(resolver, batt));

        // Temperature section: Temp Min, Max, Avg
        sections.Add(BuildTemperatureSection(resolver, batt));

        // Composition section: child counts + status breakdown
        sections.Add(await BuildCompositionSection(device, deviceTypeName, metadata, ct));

        // Module-level: cell voltage distribution chart (if cell data in payload)
        if (deviceTypeName == DeviceTypeNames.EnergyStorageModule)
        {
            var voltageChart = BuildCellVoltageDistribution(batt);
            if (voltageChart != null)
                sections.Add(voltageChart);

            var tempHeatmap = BuildCellTemperatureHeatmap(batt);
            if (tempHeatmap != null)
                sections.Add(tempHeatmap);
        }

        return sections;
    }

    // ── Sub-device section builders ───────────────────────────────────────

    private static DynamicSectionDto BuildSocSection(ParameterResolver? resolver, JsonElement? batt)
    {
        var soc = resolver?.ResolveNumber(ParameterKeys.Soc) ?? ExtractNum(batt, "socPercent");
        return new DynamicSectionDto
        {
            Key = "soc",
            Title = "State of Charge",
            Icon = "battery-charging",
            Variant = "card",
            Columns = 1,
            Fields = new List<SectionFieldDto>
            {
                new() { Key = "soc", Label = "State of Charge", Value = soc, Type = "number", Unit = "%", Format = "progress",
                    Meta = new Dictionary<string, object?> { ["thresholds"] = new[] {
                        new Dictionary<string, object?> { ["min"] = 60, ["color"] = "green" },
                        new Dictionary<string, object?> { ["min"] = 30, ["color"] = "orange" },
                        new Dictionary<string, object?> { ["min"] = 0, ["color"] = "red" }
                    }}}
            }
        };
    }

    private static DynamicSectionDto BuildElectricalSection(ParameterResolver? resolver, JsonElement? batt)
    {
        var soh = resolver?.ResolveNumber(ParameterKeys.Soh) ?? ExtractNum(batt, "sohPercent");
        var voltage = resolver?.ResolveNumber(ParameterKeys.Voltage) ?? ExtractNum(batt, "totalVoltageV");
        var current = resolver?.ResolveNumber(ParameterKeys.Current) ?? ExtractNum(batt, "instantCurrentA");

        return new DynamicSectionDto
        {
            Key = "electrical",
            Title = "Electrical",
            Icon = "zap",
            Variant = "card",
            Columns = 2,
            Fields = new List<SectionFieldDto>
            {
                NumField("soh", "State of Health", soh, "%"),
                NumField("voltage", "Voltage", voltage, "V"),
                NumField("current", "Current", current, "A"),
            }
        };
    }

    private static DynamicSectionDto BuildTemperatureSection(ParameterResolver? resolver, JsonElement? batt)
    {
        var minTemp = resolver?.ResolveNumber(ParameterKeys.TempMin) ?? ExtractNum(batt, "minCellTempC");
        var maxTemp = resolver?.ResolveNumber(ParameterKeys.TempMax) ?? ExtractNum(batt, "maxCellTempC");
        var avgTemp = resolver?.ResolveNumber(ParameterKeys.TempAvg)
            ?? ((minTemp.HasValue && maxTemp.HasValue) ? Math.Round((minTemp.Value + maxTemp.Value) / 2.0, 1) : (double?)null);

        return new DynamicSectionDto
        {
            Key = "temperature",
            Title = "Temperature",
            Icon = "thermometer",
            Variant = "card",
            Columns = 3,
            Fields = new List<SectionFieldDto>
            {
                new() { Key = "tempMin", Label = "Temp Min", Value = minTemp, Type = "number", Unit = "\u00b0C", Format = "temperature" },
                new() { Key = "tempMax", Label = "Temp Max", Value = maxTemp, Type = "number", Unit = "\u00b0C", Format = "temperature" },
                new() { Key = "tempAvg", Label = "Temp Avg", Value = avgTemp, Type = "number", Unit = "\u00b0C", Format = "temperature" },
            }
        };
    }

    private async Task<DynamicSectionDto> BuildCompositionSection(
        Domain.Entities.Device device, string? deviceTypeName, JsonElement? metadata,
        CancellationToken ct)
    {
        // Load all descendants for counting
        var directChildren = await _context.Devices
            .Include(d => d.DeviceType)
            .Where(d => d.ParentDeviceId == device.Id)
            .ToListAsync(ct);

        var allDescendants = new List<Domain.Entities.Device>(directChildren);

        if (directChildren.Count > 0)
        {
            var childIds = directChildren.Select(d => d.Id).ToList();
            var grandchildren = await _context.Devices
                .Include(d => d.DeviceType)
                .Where(d => d.ParentDeviceId.HasValue && childIds.Contains(d.ParentDeviceId.Value))
                .ToListAsync(ct);
            allDescendants.AddRange(grandchildren);

            if (grandchildren.Count > 0)
            {
                var gcIds = grandchildren.Select(d => d.Id).ToList();
                var greatGrandchildren = await _context.Devices
                    .Include(d => d.DeviceType)
                    .Where(d => d.ParentDeviceId.HasValue && gcIds.Contains(d.ParentDeviceId.Value))
                    .ToListAsync(ct);
                allDescendants.AddRange(greatGrandchildren);
            }
        }

        var packCount = allDescendants.Count(d => d.DeviceType?.Name == DeviceTypeNames.EnergyStoragePack);
        var moduleCount = allDescendants.Count(d => d.DeviceType?.Name == DeviceTypeNames.EnergyStorageModule);

        // Try to get cell count from metadata
        var cellsPerModule = ExtractNum(metadata, "cellsPerModule");
        int? totalCells = null;

        if (cellsPerModule.HasValue)
        {
            var cellsPer = (int)cellsPerModule.Value;
            totalCells = deviceTypeName switch
            {
                DeviceTypeNames.EnergyStorageModule => cellsPer,
                DeviceTypeNames.EnergyStoragePack => Math.Max(moduleCount, 1) * cellsPer,
                DeviceTypeNames.EnergyStorageRack => Math.Max(packCount, 1) * Math.Max(moduleCount / Math.Max(packCount, 1), 1) * cellsPer,
                _ => null
            };
        }

        // Query active alarm counts by severity for this device and all descendants
        var allIds = new List<Guid> { device.Id };
        allIds.AddRange(allDescendants.Select(d => d.Id));

        var alarmCounts = await _context.DeviceAlarms
            .Where(a => a.IsActive && allIds.Contains(a.DeviceId))
            .Select(a => a.AlarmType!.Severity)
            .GroupBy(s => s)
            .Select(g => new { Severity = g.Key, Count = g.Count() })
            .ToListAsync(ct);

        var warningCount = alarmCounts
            .Where(a => a.Severity is "Warning" or "Major")
            .Sum(a => a.Count);
        var faultCount = alarmCounts
            .Where(a => a.Severity == "Critical")
            .Sum(a => a.Count);

        var fields = new List<SectionFieldDto>();

        // Level-appropriate child counts
        if (deviceTypeName == DeviceTypeNames.EnergyStorageRack && packCount > 0)
            fields.Add(NumField("packCount", "Packs", packCount, ""));
        if (moduleCount > 0)
            fields.Add(NumField("moduleCount", "Modules", moduleCount, ""));

        if (totalCells.HasValue)
            fields.Add(NumField("totalCells", "Total Cells", totalCells.Value, ""));

        fields.Add(new SectionFieldDto { Key = "faultCells", Label = "Fault Cells", Value = faultCount, Type = "number",
            Meta = new Dictionary<string, object?> { ["color"] = "red" } });
        fields.Add(new SectionFieldDto { Key = "warningCells", Label = "Warning Cells", Value = warningCount, Type = "number",
            Meta = new Dictionary<string, object?> { ["color"] = "orange" } });

        return new DynamicSectionDto
        {
            Key = "composition",
            Title = "Composition",
            Icon = "layers",
            Variant = "card",
            Columns = 2,
            Fields = fields
        };
    }

    // ── Module-level cell visualizations ──────────────────────────────────

    private static DynamicSectionDto? BuildCellVoltageDistribution(JsonElement? payload)
    {
        if (!payload.HasValue) return null;

        if (!payload.Value.TryGetProperty("cellVoltages", out var cellVoltages) ||
            cellVoltages.ValueKind != JsonValueKind.Array)
            return null;

        var chartData = new List<Dictionary<string, object?>>();
        int index = 1;
        foreach (var v in cellVoltages.EnumerateArray())
        {
            if (v.ValueKind == JsonValueKind.Number)
            {
                chartData.Add(new Dictionary<string, object?>
                {
                    ["cell"] = $"C{index}",
                    ["voltage"] = v.GetDouble()
                });
            }
            index++;
        }

        if (chartData.Count == 0) return null;

        return new DynamicSectionDto
        {
            Key = "voltageDistribution",
            Title = "Voltage Distribution",
            Icon = "bar-chart-3",
            Variant = "chart",
            ChartConfig = new ChartConfigDto
            {
                Type = "bar",
                XAxis = new ChartAxisDto { Key = "cell", Label = "Cell" },
                YAxis = new ChartAxisDto { Key = "voltage", Label = "Voltage", Unit = "V" },
                Color = "#22c55e"
            },
            ChartData = chartData
        };
    }

    private static DynamicSectionDto? BuildCellTemperatureHeatmap(JsonElement? payload)
    {
        if (!payload.HasValue) return null;

        if (!payload.Value.TryGetProperty("cellTemperatures", out var cellTemps) ||
            cellTemps.ValueKind != JsonValueKind.Array)
            return null;

        var chartData = new List<Dictionary<string, object?>>();
        int index = 1;
        foreach (var t in cellTemps.EnumerateArray())
        {
            if (t.ValueKind == JsonValueKind.Number)
            {
                var temp = t.GetDouble();
                var color = temp switch
                {
                    < 25 => "yellow",
                    < 30 => "orange",
                    < 35 => "purple",
                    < 40 => "indigo",
                    _ => "blue"
                };
                chartData.Add(new Dictionary<string, object?>
                {
                    ["cell"] = $"C{index}",
                    ["temperature"] = temp,
                    ["color"] = color
                });
            }
            index++;
        }

        if (chartData.Count == 0) return null;

        return new DynamicSectionDto
        {
            Key = "temperatureHeatmap",
            Title = "Temperature Heatmap",
            Icon = "grid-3x3",
            Variant = "heatmap",
            Fields = new List<SectionFieldDto>
            {
                new() { Key = "legend", Label = "Legend", Type = "string",
                    Meta = new Dictionary<string, object?>
                    {
                        ["bands"] = new[]
                        {
                            new Dictionary<string, object?> { ["max"] = 25, ["color"] = "yellow", ["label"] = "<25\u00b0C" },
                            new Dictionary<string, object?> { ["max"] = 30, ["color"] = "orange", ["label"] = "25-30\u00b0C" },
                            new Dictionary<string, object?> { ["max"] = 35, ["color"] = "purple", ["label"] = "30-35\u00b0C" },
                            new Dictionary<string, object?> { ["max"] = 40, ["color"] = "indigo", ["label"] = "35-40\u00b0C" },
                            new Dictionary<string, object?> { ["color"] = "blue", ["label"] = ">40\u00b0C" },
                        }
                    }}
            },
            ChartData = chartData
        };
    }

    // ── TMS Summary ───────────────────────────────────────────────────────

    private async Task<List<DynamicSectionDto>> BuildTmsSummary(
        Domain.Entities.Device device, CancellationToken ct)
    {
        // Walk up to find root container (ChillerState data is stored there)
        var rootId = device.Id;
        var currentId = device.ParentDeviceId;

        while (currentId.HasValue)
        {
            var parent = await _context.Devices
                .Include(d => d.DeviceType)
                .FirstOrDefaultAsync(d => d.Id == currentId.Value, ct);
            if (parent == null) break;
            rootId = parent.Id;
            currentId = parent.ParentDeviceId;
        }

        var telemetry = await _telemetryLoader.LoadLatest(rootId, ct);
        var payloads = telemetry.GetPayloadsByKind();
        var chiller = payloads.GetValueOrDefault("ChillerState");
        var metadata = PayloadDeserializer.GetElement(device.Metadata);

        var sections = new List<DynamicSectionDto>
        {
            BuildTmsDeviceInfoSection(device, metadata),
            BuildTmsChillerStateSection(chiller),
            BuildTmsSetpointsSection(chiller),
        };

        // Status section with active alarm count
        var alarmCount = await _context.DeviceAlarms
            .Where(a => a.DeviceId == device.Id && a.IsActive)
            .CountAsync(ct);
        sections.Add(BuildTmsStatusSection(alarmCount));

        return sections;
    }

    private static DynamicSectionDto BuildTmsDeviceInfoSection(
        Domain.Entities.Device device, JsonElement? metadata)
    {
        var fields = new List<SectionFieldDto>
        {
            StringField("model", "Model", device.Model),
            StringField("serialNumber", "Serial Number", device.SerialNumber),
        };

        if (metadata.HasValue)
        {
            var firmware = PayloadDeserializer.ExtractValue(metadata.Value, "firmware") as string;
            if (firmware != null)
                fields.Add(StringField("firmware", "Firmware", firmware));

            var protocol = PayloadDeserializer.ExtractValue(metadata.Value, "protocol") as string;
            if (protocol != null)
                fields.Add(StringField("protocol", "Protocol", protocol));

            var modbusAddress = ExtractNum(metadata, "modbusAddress");
            if (modbusAddress.HasValue)
                fields.Add(NumField("modbusAddress", "Modbus Address", modbusAddress, ""));
        }

        return new DynamicSectionDto
        {
            Key = "deviceInfo",
            Title = "Device Information",
            Icon = "info",
            Variant = "highlighted",
            Columns = 3,
            Fields = fields
        };
    }

    private static DynamicSectionDto BuildTmsChillerStateSection(JsonElement? chiller)
    {
        var modeStr = chiller.HasValue
            ? PayloadDeserializer.ExtractValue(chiller.Value, "operatingModeStr") as string
            : null;

        return new DynamicSectionDto
        {
            Key = "chillerState",
            Title = "Chiller State",
            Icon = "thermometer",
            Variant = "card",
            Columns = 3,
            Fields = new List<SectionFieldDto>
            {
                new() { Key = "operatingMode", Label = "Operating Mode", Value = modeStr, Type = "string", Format = "badge",
                    Meta = new Dictionary<string, object?> { ["badgeLabel"] = modeStr, ["badgeColor"] = "sky" } },
                NumField("outletCoolantTemp", "Outlet Coolant Temp", ExtractNum(chiller, "outletCoolantTempC"), "\u00b0C"),
                NumField("inletCoolantTemp", "Inlet Coolant Temp", ExtractNum(chiller, "inletCoolantTempC"), "\u00b0C"),
                NumField("ambientTemp", "Ambient Temp", ExtractNum(chiller, "ambientTempC"), "\u00b0C"),
                NumField("inletPressure", "Inlet Coolant Pressure", ExtractNum(chiller, "inletCoolantPressure"), "bar"),
                NumField("outletPressure", "Outlet Coolant Pressure", ExtractNum(chiller, "outletCoolantPressure"), "bar"),
                BoolBadgeFieldWithExtra("compressorStatus", "Compressor", ExtractBool(chiller, "compressorStatus"),
                    "Running", "green", "Off", "grey",
                    ExtractNum(chiller, "compressorSpeedPercent"), "%", "Speed"),
                BoolBadgeFieldWithExtra("pumpStatus", "Pump", ExtractBool(chiller, "pumpStatus"),
                    "Running", "green", "Off", "grey",
                    ExtractNum(chiller, "pumpSpeedPercent"), "%", "Speed"),
                BoolBadgeField("fanStatus", "Fan", ExtractBool(chiller, "fanRunStatus"),
                    "Running", "green", "Off", "grey"),
                BoolBadgeField("electricHeater", "Electric Heater", ExtractBool(chiller, "electricHeaterStatus"),
                    "On", "orange", "Off", "grey"),
            }
        };
    }

    private static DynamicSectionDto BuildTmsSetpointsSection(JsonElement? chiller)
    {
        var modeStr = chiller.HasValue
            ? PayloadDeserializer.ExtractValue(chiller.Value, "operatingModeStr") as string
            : null;

        return new DynamicSectionDto
        {
            Key = "setpoints",
            Title = "Setpoints",
            Icon = "sliders",
            Variant = "card",
            Columns = 2,
            Fields = new List<SectionFieldDto>
            {
                NumField("coolantTempSetpoint", "Coolant Temp Setpoint", ExtractNum(chiller, "coolantTempSetpointC"), "\u00b0C"),
                NumField("highAlarm", "High Alarm", ExtractNum(chiller, "highAlarmTempC"), "\u00b0C"),
                NumField("lowAlarm", "Low Alarm", ExtractNum(chiller, "lowAlarmTempC"), "\u00b0C"),
                StringField("operatingMode", "Operating Mode", modeStr),
            }
        };
    }

    private static DynamicSectionDto BuildTmsStatusSection(int activeAlarmCount)
    {
        var statusLabel = activeAlarmCount > 0 ? "Warning" : "Normal";
        var statusColor = activeAlarmCount > 0 ? "orange" : "green";
        var statusValue = activeAlarmCount > 0 ? "warning" : "normal";

        return new DynamicSectionDto
        {
            Key = "status",
            Title = "Status",
            Icon = "shield",
            Variant = "card",
            Columns = 2,
            Fields = new List<SectionFieldDto>
            {
                new() { Key = "componentStatus", Label = "Component Status", Value = statusValue, Type = "string", Format = "badge",
                    Meta = new Dictionary<string, object?> { ["value"] = statusValue, ["label"] = statusLabel, ["color"] = statusColor } },
                NumField("activeAlarmCount", "Active Alarms", activeAlarmCount, ""),
            }
        };
    }

    // ── Generic fallback ──────────────────────────────────────────────────

    private async Task<List<DynamicSectionDto>> BuildGenericSummary(
        Domain.Entities.Device device, CancellationToken ct)
    {
        var latestData = await _context.DeviceBatteryData
            .Where(dd => dd.DeviceId == device.Id)
            .OrderByDescending(dd => dd.DeviceLocalTime)
            .FirstOrDefaultAsync(ct);

        var payload = PayloadDeserializer.GetElement(latestData?.Payload);
        var sections = new List<DynamicSectionDto>
        {
            BuildNameplateSection(device, PayloadDeserializer.GetElement(device.Metadata)),
        };

        if (payload.HasValue)
        {
            sections.Add(new DynamicSectionDto
            {
                Key = "liveStatus",
                Title = "Live Status",
                Icon = "activity",
                Variant = "card",
                Columns = 3,
                Fields = BuildFieldsFromPayload(payload.Value)
            });
        }

        return sections;
    }

    // ── Schema-driven builder ─────────────────────────────────────────────

    private static List<DynamicSectionDto> BuildFromSchema(JsonElement sectionsElement, JsonElement? payload)
    {
        var sections = new List<DynamicSectionDto>();

        foreach (var section in sectionsElement.EnumerateArray())
        {
            var key = section.TryGetProperty("key", out var k) ? k.GetString() ?? "" : "";
            var title = section.TryGetProperty("title", out var t) ? t.GetString() ?? key : key;
            var icon = section.TryGetProperty("icon", out var ic) ? ic.GetString() : null;
            var variant = section.TryGetProperty("variant", out var v) ? v.GetString() ?? "card" : "card";
            var columns = section.TryGetProperty("columns", out var c) && c.TryGetInt32(out var cols) ? cols : 3;
            var optional = section.TryGetProperty("optional", out var o) && o.GetBoolean();

            var fields = new List<SectionFieldDto>();
            if (section.TryGetProperty("fields", out var fieldsElement) &&
                fieldsElement.ValueKind == JsonValueKind.Array)
            {
                foreach (var field in fieldsElement.EnumerateArray())
                {
                    var fKey = field.TryGetProperty("key", out var fk) ? fk.GetString() ?? "" : "";
                    var fLabel = field.TryGetProperty("label", out var fl) ? fl.GetString() ?? fKey : fKey;
                    var fType = field.TryGetProperty("type", out var ft) ? ft.GetString() ?? "string" : "string";
                    var fUnit = field.TryGetProperty("unit", out var fu) ? fu.GetString() : null;
                    var fFormat = field.TryGetProperty("format", out var ff) ? ff.GetString() : null;
                    var sourcePath = field.TryGetProperty("sourcePath", out var sp) ? sp.GetString() : null;

                    object? value = null;
                    if (payload.HasValue && !string.IsNullOrEmpty(sourcePath))
                        value = PayloadDeserializer.ExtractValue(payload.Value, sourcePath);

                    fields.Add(new SectionFieldDto
                    {
                        Key = fKey,
                        Label = fLabel,
                        Value = value,
                        Type = fType,
                        Unit = fUnit,
                        Format = fFormat,
                        SourcePath = sourcePath
                    });
                }
            }

            sections.Add(new DynamicSectionDto
            {
                Key = key,
                Title = title,
                Icon = icon,
                Variant = variant,
                Columns = columns,
                Optional = optional,
                Fields = fields
            });
        }

        return sections;
    }

    // ── Auto-extract fields from payload ──────────────────────────────────

    private static List<SectionFieldDto> BuildFieldsFromPayload(JsonElement payload)
    {
        var fields = new List<SectionFieldDto>();
        foreach (var prop in payload.EnumerateObject())
        {
            if (prop.Name is "kind" or "timestampUtc" or "version" or "DeviceId") continue;
            if (prop.Value.ValueKind is JsonValueKind.Array or JsonValueKind.Object) continue;

            var type = prop.Value.ValueKind switch
            {
                JsonValueKind.Number => "number",
                JsonValueKind.True or JsonValueKind.False => "boolean",
                _ => "string"
            };

            fields.Add(new SectionFieldDto
            {
                Key = prop.Name,
                Label = FormatLabel(prop.Name),
                Value = PayloadDeserializer.ExtractValue(payload, prop.Name),
                Type = type
            });
        }
        return fields;
    }

    private static string FormatLabel(string camelCase)
    {
        var result = System.Text.RegularExpressions.Regex.Replace(camelCase, "([A-Z])", " $1");
        return char.ToUpper(result[0]) + result[1..];
    }

    // ── Field helpers ─────────────────────────────────────────────────────

    private static SectionFieldDto StringField(string key, string label, string? value) =>
        new() { Key = key, Label = label, Value = value, Type = "string" };

    private static SectionFieldDto NumField(string key, string label, double? value, string? unit) =>
        new() { Key = key, Label = label, Value = value, Type = "number", Unit = unit };

    private static SectionFieldDto BoolBadgeField(string key, string label, bool? value,
        string trueLabel, string trueColor, string falseLabel, string falseColor) =>
        new()
        {
            Key = key, Label = label, Value = value, Type = "boolean", Format = "badge",
            Meta = new Dictionary<string, object?>
            {
                ["trueLabel"] = trueLabel, ["trueColor"] = trueColor,
                ["falseLabel"] = falseLabel, ["falseColor"] = falseColor
            }
        };

    private static SectionFieldDto BoolBadgeFieldWithExtra(string key, string label, bool? value,
        string trueLabel, string trueColor, string falseLabel, string falseColor,
        double? additionalValue, string? additionalUnit, string? additionalLabel) =>
        new()
        {
            Key = key, Label = label, Value = value, Type = "boolean", Format = "badge",
            Meta = new Dictionary<string, object?>
            {
                ["trueLabel"] = trueLabel, ["trueColor"] = trueColor,
                ["falseLabel"] = falseLabel, ["falseColor"] = falseColor,
                ["additionalValue"] = additionalValue,
                ["additionalUnit"] = additionalUnit,
                ["additionalLabel"] = additionalLabel
            }
        };

    private static Dictionary<string, object?>? LocationMeta(JsonElement? payload, string bmuPath, string cellPath)
    {
        var bmu = ExtractNum(payload, bmuPath);
        var cell = ExtractNum(payload, cellPath);
        if (!bmu.HasValue && !cell.HasValue) return null;
        return new Dictionary<string, object?>
        {
            ["location"] = new Dictionary<string, object?> { ["bmu"] = bmu, ["cell"] = cell }
        };
    }

    private static double? ExtractNum(JsonElement? element, string path)
    {
        if (!element.HasValue) return null;
        var value = PayloadDeserializer.ExtractValue(element.Value, path);
        return value switch { double d => d, int i => i, long l => l, _ => null };
    }

    private static bool? ExtractBool(JsonElement? element, string path)
    {
        if (!element.HasValue) return null;
        var value = PayloadDeserializer.ExtractValue(element.Value, path);
        return value switch
        {
            bool b => b,
            int i => i != 0,
            long l => l != 0,
            double d => d != 0,
            _ => null
        };
    }
}
