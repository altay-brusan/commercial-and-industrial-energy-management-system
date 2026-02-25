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

        // Load all devices under the same RTU for alarm counts and connected devices
        var allDevices = await _context.Devices
            .Include(d => d.DeviceType)
            .Where(d => d.RtuId == device.RtuId)
            .ToListAsync(cancellationToken);

        // Alarm count: this device + all descendants
        var descendantIds = GetDescendantIds(device.Id, allDevices);
        var scopeIds = new List<Guid> { device.Id };
        scopeIds.AddRange(descendantIds);

        var alarmCount = await _context.DeviceAlarms
            .Where(a => scopeIds.Contains(a.DeviceId) && a.IsActive)
            .CountAsync(cancellationToken);

        // Status from alarms
        var status = await _statusService.GetStatusAsync(device.Id, cancellationToken);

        // Tabs from specification template
        var tabs = GetAvailableTabs(device.SpecificationTemplate?.SpecificationsTemplate);

        // Load telemetry for parameter resolution
        var batteryEntries = await _context.DeviceBatteryData
            .Where(dd => dd.DeviceId == device.Id)
            .GroupBy(dd => dd.Kind)
            .Select(g => g.OrderByDescending(dd => dd.DeviceLocalTime).First())
            .ToListAsync(cancellationToken);

        var pcsEntries = await _context.DevicePcsData
            .Where(dd => dd.DeviceId == device.Id)
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

        // Build resolver
        var parameterMap = ParameterMapParser.ParseCached(device.SpecificationTemplate?.ParameterMapTemplate, _cache);
        var metadata = PayloadDeserializer.GetElement(device.Metadata);
        var resolver = parameterMap.Count > 0 && payloadsByKind.Count > 0
            ? new ParameterResolver(parameterMap, payloadsByKind, metadata)
            : null;

        var battPayload = payloadsByKind.GetValueOrDefault("ContainerBatteryState");

        var stateStr = resolver?.ResolveString(ParameterKeys.OperatingMode)
            ?? (battPayload.HasValue ? PayloadDeserializer.ExtractValue(battPayload.Value, "state") as string : null);

        // Connected devices: direct children grouped by display type (esc, hvac, pcs)
        var connectedDevices = new Dictionary<string, List<ConnectedDeviceDto>>();
        var children = allDevices.Where(d => d.ParentDeviceId == device.Id).ToList();
        foreach (var child in children)
        {
            var displayType = BessDisplayTypeMapper.MapToDisplayType(child.DeviceType?.Name);
            if (displayType is null or "battery" or "rack" or "pack" or "module")
                continue; // skip hierarchy devices, only include siblings like esc, hvac, pcs

            if (!connectedDevices.TryGetValue(displayType, out var list))
            {
                list = new List<ConnectedDeviceDto>();
                connectedDevices[displayType] = list;
            }
            list.Add(new ConnectedDeviceDto { Id = child.Id, Name = child.Name });
        }

        return new BessUnitHeaderDto
        {
            Id = device.Id,
            Name = device.Name,
            SerialNumber = device.SerialNumber,
            ModelName = device.Model,
            Manufacturer = device.Manufacturer,
            FacilityId = device.Rtu!.FacilityId,
            FacilityName = device.Rtu?.Facility?.Name,
            Status = StatusBadgeDto.FromStatus(status),
            OperatingMode = stateStr != null ? StatusBadgeDto.FromOperatingMode(stateStr) : null,
            Soc = resolver?.ResolveNumber(ParameterKeys.Soc)
                ?? ExtractDouble(battPayload, "socPercent"),
            Soh = resolver?.ResolveNumber(ParameterKeys.Soh)
                ?? ExtractDouble(battPayload, "sohPercent"),
            Power = resolver?.ResolveNumber(ParameterKeys.Power)
                ?? (ExtractDouble(battPayload, "instantCurrentA") is double current
                        && ExtractDouble(battPayload, "totalVoltageV") is double voltage
                    ? Math.Round(voltage * current / 1000.0, 1)
                    : null),
            CapacityKwh = resolver?.ResolveNumber(ParameterKeys.AvailableEnergy)
                ?? ExtractDouble(battPayload, "availableEnergyKwh"),
            ActiveAlarmCount = alarmCount,
            AvailableTabs = tabs,
            ConnectedDevices = connectedDevices,
            LastSeenUtc = device.LastSeenUtc
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
