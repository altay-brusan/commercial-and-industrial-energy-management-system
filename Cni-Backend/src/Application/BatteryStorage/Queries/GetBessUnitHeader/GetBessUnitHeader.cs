using System.Text.Json;
using Backend.Application.BatteryStorage.Queries.GetBessDeviceTree;
using Backend.Application.Common.Interfaces;
using Backend.Application.Common.Models.Monitoring;
using Backend.Application.Common.Security;
using Backend.Application.Common.Services;
using Backend.Application.Common.Services.ParameterMapper;
using Backend.Domain.Constants;
using Microsoft.Extensions.Caching.Memory;

namespace Backend.Application.BatteryStorage.Queries.GetBessUnitHeader;

[Authorize(Roles = "Administrator,Tenant.Admin")]
[RequirePermission(Backend.Domain.Constants.Permissions.BessView)]
public record GetBessUnitHeaderQuery : IRequest<BessUnitHeaderDto>
{
    public Guid UnitId { get; init; }
}

public class GetBessUnitHeaderQueryHandler : IRequestHandler<GetBessUnitHeaderQuery, BessUnitHeaderDto>
{
    private readonly IApplicationDbContext _context;
    private readonly IDeviceStatusService _statusService;
    private readonly IMemoryCache _cache;
    private readonly ContainerTelemetryLoader _telemetryLoader;

    public GetBessUnitHeaderQueryHandler(IApplicationDbContext context, IDeviceStatusService statusService, IMemoryCache cache)
    {
        _context = context;
        _statusService = statusService;
        _cache = cache;
        _telemetryLoader = new ContainerTelemetryLoader(context);
    }

    public async Task<BessUnitHeaderDto> Handle(GetBessUnitHeaderQuery request, CancellationToken cancellationToken)
    {
        var device = await _context.Devices
            .Include(d => d.DeviceType)
            .Include(d => d.SpecificationTemplate)
            .Include(d => d.Rtu!).ThenInclude(r => r.Facility)
            .FirstOrDefaultAsync(d => d.Id == request.UnitId, cancellationToken);

        Guard.Against.NotFound(request.UnitId, device);

        // Detect hierarchy level
        var displayType = BessDisplayTypeMapper.MapToDisplayType(device.DeviceType?.Name);
        var isRootUnit = displayType == "unit";
        var isHvac = displayType == "hvac";
        var isEsc = displayType == "esc";

        // Load all devices under the same RTU (with templates for ancestor lookup)
        var allDevices = await _context.Devices
            .Include(d => d.DeviceType)
            .Include(d => d.SpecificationTemplate)
            .Where(d => d.RtuId == device.RtuId)
            .ToListAsync(cancellationToken);

        // Walk up parent chain to find ancestors and resolve template
        Guid? unitId = null;
        string? unitName = null;
        Guid? rackId = null;
        string? rackName = null;
        Guid? packId = null;
        string? packName = null;
        var hierarchyNumber = device.HierarchyIndex.HasValue ? device.HierarchyIndex.Value + 1 : (int?)null;
        var specTemplate = device.SpecificationTemplate;

        if (!isRootUnit)
        {
            var deviceLookup = allDevices.ToDictionary(d => d.Id);
            var ancestor = device;
            while (ancestor.ParentDeviceId.HasValue && deviceLookup.TryGetValue(ancestor.ParentDeviceId.Value, out var parent))
            {
                var parentType = BessDisplayTypeMapper.MapToDisplayType(parent.DeviceType?.Name);
                switch (parentType)
                {
                    case "unit":
                        unitId = parent.Id;
                        unitName = parent.Name;
                        specTemplate ??= parent.SpecificationTemplate;
                        break;
                    case "rack":
                        rackId = parent.Id;
                        rackName = parent.Name;
                        break;
                    case "pack":
                        packId = parent.Id;
                        packName = parent.Name;
                        break;
                }
                ancestor = parent;
            }
        }

        // Alarm count: this device + all descendants
        var descendantIds = GetDescendantIds(device.Id, allDevices);
        var scopeIds = new List<Guid> { device.Id };
        scopeIds.AddRange(descendantIds);

        var alarmCount = await _context.DeviceAlarms
            .Where(a => scopeIds.Contains(a.DeviceId) && a.IsActive)
            .CountAsync(cancellationToken);

        // Status from alarms
        var status = await _statusService.GetStatusAsync(device.Id, cancellationToken);

        // HVAC/ESC: return sections-based header
        if (isHvac || isEsc)
        {
            var sectionsTabs = GetSubUnitTabs(displayType);
            var sections = isHvac
                ? await BuildHvacHeaderAsync(device, allDevices, alarmCount, cancellationToken)
                : await BuildEscHeaderAsync(device, allDevices, alarmCount, cancellationToken);

            return new BessUnitHeaderDto
            {
                Id = device.Id,
                Name = device.Name,
                FacilityId = device.Rtu!.FacilityId,
                FacilityName = device.Rtu?.Facility?.Name,
                UnitId = unitId,
                UnitName = unitName,
                HierarchyNumber = hierarchyNumber,
                DisplayType = displayType,
                Status = StatusBadgeDto.FromStatus(status),
                ActiveAlarmCount = alarmCount,
                AvailableTabs = sectionsTabs,
                Sections = sections,
            };
        }

        // Tabs: root uses template/defaults, sub-units get level-specific tabs
        var tabs = isRootUnit
            ? GetAvailableTabs(specTemplate?.SpecificationsTemplate)
            : GetSubUnitTabs(displayType);

        // For all sub-devices, telemetry is stored under the root container
        var telemetryDeviceId = !isRootUnit && unitId.HasValue
            ? unitId.Value
            : device.Id;

        // Load telemetry for parameter resolution
        var batteryEntries = await _context.DeviceBatteryData
            .Where(dd => dd.DeviceId == telemetryDeviceId)
            .GroupBy(dd => dd.Kind)
            .Select(g => g.OrderByDescending(dd => dd.DeviceLocalTime).First())
            .ToListAsync(cancellationToken);

        var pcsEntries = await _context.DevicePcsData
            .Where(dd => dd.DeviceId == telemetryDeviceId)
            .GroupBy(dd => dd.Kind)
            .Select(g => g.OrderByDescending(dd => dd.DeviceLocalTime).First())
            .ToListAsync(cancellationToken);

        var payloadsByKind = new Dictionary<string, JsonElement?>();
        foreach (var entry in batteryEntries)
        {
            if (entry.Kind != null)
                payloadsByKind[entry.Kind] = PayloadDeserializer.GetElement(entry.Payload);
        }
        foreach (var entry in pcsEntries)
        {
            if (entry.Kind != null)
                payloadsByKind[entry.Kind] = PayloadDeserializer.GetElement(entry.Payload);
        }

        // Build resolver using resolved template (own or inherited from root)
        var parameterMap = ParameterMapParser.ParseCached(specTemplate?.ParameterMapTemplate, _cache);
        var metadata = PayloadDeserializer.GetElement(device.Metadata);
        var resolver = parameterMap.Count > 0 && payloadsByKind.Count > 0
            ? new ParameterResolver(parameterMap, payloadsByKind, metadata)
            : null;

        var battPayload = payloadsByKind.GetValueOrDefault("ContainerBatteryState");

        // Operating mode (root only)
        StatusBadgeDto? operatingMode = null;
        if (isRootUnit)
        {
            var stateStr = resolver?.ResolveString(ParameterKeys.OperatingMode)
                ?? (battPayload.HasValue ? PayloadDeserializer.ExtractValue(battPayload.Value, "state") as string : null);
            operatingMode = stateStr != null ? StatusBadgeDto.FromOperatingMode(stateStr) : null;
        }

        // Connected devices: root only, direct children grouped by display type (esc, hvac, pcs)
        var connectedDevices = new Dictionary<string, List<ConnectedDeviceDto>>();
        if (isRootUnit)
        {
            var children = allDevices.Where(d => d.ParentDeviceId == device.Id).ToList();
            foreach (var child in children)
            {
                var childDisplayType = BessDisplayTypeMapper.MapToDisplayType(child.DeviceType?.Name);
                if (childDisplayType is null or "battery" or "rack" or "pack" or "module")
                    continue; // skip hierarchy devices, only include siblings like esc, hvac, pcs

                if (!connectedDevices.TryGetValue(childDisplayType, out var list))
                {
                    list = new List<ConnectedDeviceDto>();
                    connectedDevices[childDisplayType] = list;
                }
                list.Add(new ConnectedDeviceDto { Id = child.Id, Name = child.Name });
            }
        }

        return new BessUnitHeaderDto
        {
            Id = device.Id,
            Name = device.Name,
            SerialNumber = isRootUnit ? device.SerialNumber : null,
            ModelName = isRootUnit ? device.Model : null,
            Manufacturer = isRootUnit ? device.Manufacturer : null,
            FacilityId = device.Rtu!.FacilityId,
            FacilityName = device.Rtu?.Facility?.Name,
            UnitId = unitId,
            UnitName = unitName,
            RackId = rackId,
            RackName = rackName,
            PackId = packId,
            PackName = packName,
            HierarchyNumber = hierarchyNumber,
            DisplayType = displayType,
            Status = StatusBadgeDto.FromStatus(status),
            OperatingMode = operatingMode,
            Soc = resolver?.ResolveNumber(ParameterKeys.Soc)
                ?? ExtractDouble(battPayload, "socPercent"),
            Soh = resolver?.ResolveNumber(ParameterKeys.Soh)
                ?? ExtractDouble(battPayload, "sohPercent"),
            Voltage = resolver?.ResolveNumber(ParameterKeys.Voltage)
                ?? ExtractDouble(battPayload, "totalVoltageV"),
            Current = resolver?.ResolveNumber(ParameterKeys.Current)
                ?? ExtractDouble(battPayload, "instantCurrentA"),
            Power = resolver?.ResolveNumber(ParameterKeys.Power)
                ?? (ExtractDouble(battPayload, "instantCurrentA") is double current
                        && ExtractDouble(battPayload, "totalVoltageV") is double voltage
                    ? Math.Round(voltage * current / 1000.0, 1)
                    : null),
            CapacityKwh = isRootUnit
                ? resolver?.ResolveNumber(ParameterKeys.AvailableEnergy)
                    ?? ExtractDouble(battPayload, "availableEnergyKwh")
                : null,
            ActiveAlarmCount = alarmCount,
            AvailableTabs = tabs,
            ConnectedDevices = connectedDevices,
            LastSeenUtc = isRootUnit ? device.LastSeenUtc : null
        };
    }

    // ── HVAC sections-based header ──────────────────────────────────────

    private async Task<List<DynamicSectionDto>> BuildHvacHeaderAsync(
        Domain.Entities.Device device, List<Domain.Entities.Device> allDevices,
        int alarmCount, CancellationToken ct)
    {
        var rootId = FindRootDeviceId(device, allDevices);
        var telemetry = await _telemetryLoader.LoadLatest(rootId, ct);
        var payloads = telemetry.GetPayloadsByKind();
        var chiller = payloads.GetValueOrDefault("ChillerState");
        var metadata = PayloadDeserializer.GetElement(device.Metadata);

        var modeStr = chiller.HasValue
            ? PayloadDeserializer.ExtractValue(chiller.Value, "operatingModeStr") as string
            : null;

        return new List<DynamicSectionDto>
        {
            BuildDeviceInfoSection(device, metadata),
            BuildChillerStateSection(chiller, modeStr),
            BuildSetpointsSection(chiller, modeStr),
            BuildStatusSection(alarmCount),
        };
    }

    // ── ESC sections-based header ───────────────────────────────────────

    private async Task<List<DynamicSectionDto>> BuildEscHeaderAsync(
        Domain.Entities.Device device, List<Domain.Entities.Device> allDevices,
        int alarmCount, CancellationToken ct)
    {
        var rootId = FindRootDeviceId(device, allDevices);
        var telemetry = await _telemetryLoader.LoadLatest(rootId, ct);
        var payloads = telemetry.GetPayloadsByKind();
        var metadata = PayloadDeserializer.GetElement(device.Metadata);

        // ESC telemetry may come from PcsState or ControllerState kinds
        var pcsState = payloads.GetValueOrDefault("PcsState");
        var controllerState = payloads.GetValueOrDefault("ControllerState");
        var escPayload = controllerState ?? pcsState;

        return new List<DynamicSectionDto>
        {
            BuildDeviceInfoSection(device, metadata),
            BuildEscLiveDataSection(escPayload, metadata),
            BuildStatusSection(alarmCount),
        };
    }

    // ── Shared section builders ─────────────────────────────────────────

    private static Guid FindRootDeviceId(Domain.Entities.Device device, List<Domain.Entities.Device> allDevices)
    {
        var deviceLookup = allDevices.ToDictionary(d => d.Id);
        var current = device;
        while (current.ParentDeviceId.HasValue && deviceLookup.TryGetValue(current.ParentDeviceId.Value, out var parent))
            current = parent;
        return current.Id;
    }

    private static DynamicSectionDto BuildDeviceInfoSection(Domain.Entities.Device device, JsonElement? metadata)
    {
        var fields = new List<SectionFieldDto>
        {
            new() { Key = "model", Label = "Model", Value = device.Model, Type = "string" },
            new() { Key = "serialNumber", Label = "Serial Number", Value = device.SerialNumber, Type = "string" },
        };

        if (metadata.HasValue)
        {
            var firmware = PayloadDeserializer.ExtractValue(metadata.Value, "firmware") as string;
            if (firmware != null)
                fields.Add(new SectionFieldDto { Key = "firmware", Label = "Firmware", Value = firmware, Type = "string" });

            var protocol = PayloadDeserializer.ExtractValue(metadata.Value, "protocol") as string;
            if (protocol != null)
                fields.Add(new SectionFieldDto { Key = "protocol", Label = "Protocol", Value = protocol, Type = "string" });

            var modbusAddress = ExtractDouble(metadata, "modbusAddress");
            if (modbusAddress.HasValue)
                fields.Add(new SectionFieldDto { Key = "modbusAddress", Label = "Modbus Address", Value = modbusAddress, Type = "number" });
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

    private static DynamicSectionDto BuildChillerStateSection(JsonElement? chiller, string? modeStr)
    {
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
                new() { Key = "outletCoolantTemp", Label = "Outlet Coolant Temp", Value = ExtractDouble(chiller, "outletCoolantTempC"), Type = "number", Unit = "\u00b0C" },
                new() { Key = "inletCoolantTemp", Label = "Inlet Coolant Temp", Value = ExtractDouble(chiller, "inletCoolantTempC"), Type = "number", Unit = "\u00b0C" },
                new() { Key = "ambientTemp", Label = "Ambient Temp", Value = ExtractDouble(chiller, "ambientTempC"), Type = "number", Unit = "\u00b0C" },
                new() { Key = "inletPressure", Label = "Inlet Coolant Pressure", Value = ExtractDouble(chiller, "inletCoolantPressure"), Type = "number", Unit = "bar" },
                new() { Key = "outletPressure", Label = "Outlet Coolant Pressure", Value = ExtractDouble(chiller, "outletCoolantPressure"), Type = "number", Unit = "bar" },
                BoolBadgeFieldWithExtra("compressorStatus", "Compressor", ExtractBool(chiller, "compressorStatus"),
                    "Running", "green", "Off", "grey",
                    ExtractDouble(chiller, "compressorSpeedPercent"), "%", "Speed"),
                BoolBadgeFieldWithExtra("pumpStatus", "Pump", ExtractBool(chiller, "pumpStatus"),
                    "Running", "green", "Off", "grey",
                    ExtractDouble(chiller, "pumpSpeedPercent"), "%", "Speed"),
                BoolBadgeField("fanStatus", "Fan", ExtractBool(chiller, "fanRunStatus"),
                    "Running", "green", "Off", "grey"),
                BoolBadgeField("electricHeater", "Electric Heater", ExtractBool(chiller, "electricHeaterStatus"),
                    "On", "orange", "Off", "grey"),
            }
        };
    }

    private static DynamicSectionDto BuildSetpointsSection(JsonElement? chiller, string? modeStr)
    {
        return new DynamicSectionDto
        {
            Key = "setpoints",
            Title = "Setpoints",
            Icon = "sliders",
            Variant = "card",
            Columns = 2,
            Fields = new List<SectionFieldDto>
            {
                new() { Key = "coolantTempSetpoint", Label = "Coolant Temp Setpoint", Value = ExtractDouble(chiller, "coolantTempSetpointC"), Type = "number", Unit = "\u00b0C" },
                new() { Key = "highAlarm", Label = "High Alarm", Value = ExtractDouble(chiller, "highAlarmTempC"), Type = "number", Unit = "\u00b0C" },
                new() { Key = "lowAlarm", Label = "Low Alarm", Value = ExtractDouble(chiller, "lowAlarmTempC"), Type = "number", Unit = "\u00b0C" },
                new() { Key = "operatingMode", Label = "Operating Mode", Value = modeStr, Type = "string" },
            }
        };
    }

    private static DynamicSectionDto BuildEscLiveDataSection(JsonElement? escPayload, JsonElement? metadata)
    {
        var operatingState = escPayload.HasValue
            ? PayloadDeserializer.ExtractValue(escPayload.Value, "operatingStateStr") as string
                ?? PayloadDeserializer.ExtractValue(escPayload.Value, "state") as string
            : null;

        var badgeColor = operatingState?.ToLowerInvariant() switch
        {
            "running" or "active" => "green",
            "standby" or "idle" => "sky",
            "fault" or "error" => "red",
            _ => "grey"
        };

        return new DynamicSectionDto
        {
            Key = "liveData",
            Title = "Live Data",
            Icon = "activity",
            Variant = "card",
            Columns = 3,
            Fields = new List<SectionFieldDto>
            {
                new() { Key = "dcVoltageIn", Label = "DC Voltage In", Value = ExtractDouble(escPayload, "dcVoltageIn", "dcVoltageInV"), Type = "number", Unit = "V" },
                new() { Key = "dcVoltageOut", Label = "DC Voltage Out", Value = ExtractDouble(escPayload, "dcVoltageOut", "dcVoltageOutV"), Type = "number", Unit = "V" },
                new() { Key = "dcCurrent", Label = "DC Current", Value = ExtractDouble(escPayload, "dcCurrent", "dcCurrentA"), Type = "number", Unit = "A" },
                new() { Key = "power", Label = "Power", Value = ExtractDouble(escPayload, "power", "powerKw"), Type = "number", Unit = "kW" },
                new() { Key = "efficiency", Label = "Efficiency", Value = ExtractDouble(escPayload, "efficiency", "efficiencyPercent"), Type = "number", Unit = "%" },
                new() { Key = "temperature", Label = "Temperature", Value = ExtractDouble(escPayload, "temperature", "temperatureC"), Type = "number", Unit = "\u00b0C", Format = "temperature" },
                new() { Key = "isolationResistance", Label = "Isolation Resistance", Value = ExtractDouble(escPayload, "isolationResistance", "isolationResistanceKohm"), Type = "number", Unit = "k\u03a9" },
                new() { Key = "operatingState", Label = "Operating State", Value = operatingState, Type = "string", Format = "badge",
                    Meta = new Dictionary<string, object?> { ["badgeLabel"] = operatingState, ["badgeColor"] = badgeColor } },
            }
        };
    }

    private static DynamicSectionDto BuildStatusSection(int activeAlarmCount)
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
                new() { Key = "activeAlarmCount", Label = "Active Alarms", Value = activeAlarmCount, Type = "number" },
            }
        };
    }

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

    private static List<Guid> GetDescendantIds(Guid parentId, List<Domain.Entities.Device> allDevices)
    {
        var result = new List<Guid>();
        var children = allDevices.Where(d => d.ParentDeviceId == parentId).Select(d => d.Id).ToList();
        result.AddRange(children);
        foreach (var childId in children)
            result.AddRange(GetDescendantIds(childId, allDevices));
        return result;
    }

    private static double? ExtractDouble(JsonElement? element, params string[] paths)
    {
        if (!element.HasValue) return null;
        foreach (var path in paths)
        {
            var value = PayloadDeserializer.ExtractValue(element.Value, path);
            var result = value switch
            {
                double d => d,
                int i => i,
                long l => l,
                _ => (double?)null
            };
            if (result.HasValue) return result;
        }
        return null;
    }

    private static bool? ExtractBool(JsonElement? element, string path)
    {
        if (!element.HasValue) return null;
        var value = PayloadDeserializer.ExtractValue(element.Value, path);
        return value switch
        {
            bool b => b,
            _ => null
        };
    }

    private static List<TabInfoDto> GetSubUnitTabs(string? displayType)
    {
        if (displayType is "hvac" or "esc")
        {
            return new List<TabInfoDto>
            {
                new() { Key = "settings", Label = "Settings", Order = 1 },
                new() { Key = "alarms", Label = "Alarms", Order = 2 }
            };
        }

        var tabs = new List<TabInfoDto>
        {
            new() { Key = "summary", Label = "Summary", Order = 1 }
        };

        if (displayType == "module")
            tabs.Add(new TabInfoDto { Key = "cells", Label = "Cells", Order = 2 });

        tabs.Add(new TabInfoDto { Key = "alarms", Label = "Alarms", Order = tabs.Count + 1 });

        return tabs;
    }

    private static List<TabInfoDto> GetAvailableTabs(string? specifications)
    {
        var defaultTabs = new List<TabInfoDto>
        {
            new() { Key = "summary", Label = "Summary", Order = 1 },
            new() { Key = "health", Label = "Health", Order = 2 },
            new() { Key = "curves", Label = "Curves", Order = 3 },
            new() { Key = "hierarchy", Label = "Hierarchy", Order = 4 },
            new() { Key = "faultFlags", Label = "Fault Flags", Order = 5 },
            new() { Key = "faults", Label = "Faults", Order = 6 }
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
                var enabled = !tab.TryGetProperty("isEnabled", out var e) || e.GetBoolean();
                tabs.Add(new TabInfoDto { Key = key, Label = label, IsEnabled = enabled, Order = order });
            }
            return tabs.Count > 0 ? tabs : defaultTabs;
        }

        return defaultTabs;
    }
}
