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

    public GetBessUnitHeaderQueryHandler(IApplicationDbContext context, IDeviceStatusService statusService, IMemoryCache cache)
    {
        _context = context;
        _statusService = statusService;
        _cache = cache;
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

        // HVAC-specific: extract chiller data
        double? outletCoolantTemp = null;
        double? inletCoolantTemp = null;
        double? ambientTemp = null;
        bool? compressorRunning = null;
        string? tmsOperatingMode = null;

        if (isHvac)
        {
            outletCoolantTemp = resolver?.ResolveNumber(ParameterKeys.ChillerCoolantOut)
                ?? ExtractDouble(payloadsByKind.GetValueOrDefault("ChillerState"), "outletCoolantTempC");
            inletCoolantTemp = resolver?.ResolveNumber(ParameterKeys.ChillerCoolantIn)
                ?? ExtractDouble(payloadsByKind.GetValueOrDefault("ChillerState"), "inletCoolantTempC");
            ambientTemp = resolver?.ResolveNumber(ParameterKeys.AmbientTemp)
                ?? ExtractDouble(payloadsByKind.GetValueOrDefault("ChillerState"), "ambientTempC");
            compressorRunning = resolver?.ResolveBool(ParameterKeys.CompressorRunning)
                ?? ExtractBool(payloadsByKind.GetValueOrDefault("ChillerState"), "compressorStatus");
            tmsOperatingMode = ExtractString(payloadsByKind.GetValueOrDefault("ChillerState"), "operatingModeStr");
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
            Status = StatusBadgeDto.FromStatus(status),
            OperatingMode = operatingMode,
            Soc = isHvac ? null
                : resolver?.ResolveNumber(ParameterKeys.Soc)
                    ?? ExtractDouble(battPayload, "socPercent"),
            Soh = isHvac ? null
                : resolver?.ResolveNumber(ParameterKeys.Soh)
                    ?? ExtractDouble(battPayload, "sohPercent"),
            Voltage = isHvac ? null
                : resolver?.ResolveNumber(ParameterKeys.Voltage)
                    ?? ExtractDouble(battPayload, "totalVoltageV"),
            Current = isHvac ? null
                : resolver?.ResolveNumber(ParameterKeys.Current)
                    ?? ExtractDouble(battPayload, "instantCurrentA"),
            Power = isHvac ? null
                : resolver?.ResolveNumber(ParameterKeys.Power)
                    ?? (ExtractDouble(battPayload, "instantCurrentA") is double current
                            && ExtractDouble(battPayload, "totalVoltageV") is double voltage
                        ? Math.Round(voltage * current / 1000.0, 1)
                        : null),
            CapacityKwh = isRootUnit
                ? resolver?.ResolveNumber(ParameterKeys.AvailableEnergy)
                    ?? ExtractDouble(battPayload, "availableEnergyKwh")
                : null,
            OutletCoolantTemp = outletCoolantTemp,
            InletCoolantTemp = inletCoolantTemp,
            AmbientTemp = ambientTemp,
            CompressorRunning = compressorRunning,
            TmsOperatingMode = tmsOperatingMode,
            ActiveAlarmCount = alarmCount,
            AvailableTabs = tabs,
            ConnectedDevices = connectedDevices,
            LastSeenUtc = isRootUnit ? device.LastSeenUtc : null
        };
    }

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

    private static string? ExtractString(JsonElement? element, string path)
    {
        if (!element.HasValue) return null;
        return PayloadDeserializer.ExtractValue(element.Value, path) as string;
    }

    private static List<TabInfoDto> GetSubUnitTabs(string? displayType)
    {
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
