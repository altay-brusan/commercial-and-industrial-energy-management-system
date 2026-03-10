using Backend.Application.Common.Interfaces;
using Backend.Application.Common.Security;
using Backend.Domain.Constants;

namespace Backend.Application.BatteryStorage.Queries.GetBessDeviceTree;

[Authorize(Roles = "Administrator,Tenant.Admin")]
[RequirePermission(Backend.Domain.Constants.Permissions.BessView)]
public record GetBessDeviceTreeQuery : IRequest<BessDeviceTreeResponseDto>
{
    public Guid? FacilityId { get; init; }
}

public class GetBessDeviceTreeQueryHandler : IRequestHandler<GetBessDeviceTreeQuery, BessDeviceTreeResponseDto>
{
    private readonly IApplicationDbContext _context;
    private readonly IDeviceStatusService _statusService;

    public GetBessDeviceTreeQueryHandler(IApplicationDbContext context, IDeviceStatusService statusService)
    {
        _context = context;
        _statusService = statusService;
    }

    public async Task<BessDeviceTreeResponseDto> Handle(GetBessDeviceTreeQuery request, CancellationToken cancellationToken)
    {
        // Find all root EnergyStorage devices
        var rootQuery = _context.Devices
            .Include(d => d.DeviceType)
            .Include(d => d.LogicalDevice)
            .Where(d =>
                (d.DeviceType != null && d.DeviceType.Name == DeviceTypeNames.EnergyStorage) ||
                (d.LogicalDevice != null && d.LogicalDevice.Category == "BESS"));

        if (request.FacilityId.HasValue)
            rootQuery = rootQuery.Where(d => d.Rtu!.FacilityId == request.FacilityId.Value);

        var rootDevices = await rootQuery
            .OrderBy(d => d.Name)
            .ToListAsync(cancellationToken);

        if (rootDevices.Count == 0)
            return new BessDeviceTreeResponseDto();

        // Load all devices under the same RTUs to get full hierarchy
        var rtuIds = rootDevices.Select(d => d.RtuId).Distinct().ToList();
        var allDevices = await _context.Devices
            .Include(d => d.DeviceType)
            .Include(d => d.LogicalDevice)
            .Where(d => rtuIds.Contains(d.RtuId))
            .ToListAsync(cancellationToken);

        var deviceIds = allDevices.Select(d => d.Id).ToList();

        // Batch-resolve statuses from active alarms
        var statuses = await _statusService.GetStatusBatchAsync(deviceIds, cancellationToken);

        // Load alarm counts for all devices in one query
        var alarmCounts = await _context.DeviceAlarms
            .Where(a => deviceIds.Contains(a.DeviceId) && a.IsActive)
            .GroupBy(a => a.DeviceId)
            .Select(g => new { DeviceId = g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.DeviceId, x => x.Count, cancellationToken);

        // Build parent→children lookup
        var childrenMap = allDevices
            .Where(d => d.ParentDeviceId.HasValue)
            .GroupBy(d => d.ParentDeviceId!.Value)
            .ToDictionary(g => g.Key, g => g.OrderBy(d => d.HierarchyIndex).ThenBy(d => d.Name).ToList());

        // Build a tree for each root BESS unit
        var trees = new List<BessTreeNodeDto>();
        foreach (var root in rootDevices)
        {
            var node = BuildBessTreeNode(root, childrenMap, statuses, alarmCounts);
            if (node != null)
                trees.Add(node);
        }

        return new BessDeviceTreeResponseDto { Tree = trees };
    }

    private static BessTreeNodeDto? BuildBessTreeNode(
        Domain.Entities.Device device,
        Dictionary<Guid, List<Domain.Entities.Device>> childrenMap,
        Dictionary<Guid, string> statuses,
        Dictionary<Guid, int> alarmCounts)
    {
        var displayType = BessDisplayTypeMapper.MapToDisplayType(device.DeviceType?.Name);
        if (displayType is null)
            return null;

        var children = new List<BessTreeNodeDto>();
        if (childrenMap.TryGetValue(device.Id, out var childDevices))
        {
            foreach (var child in childDevices)
            {
                var childNode = BuildBessTreeNode(child, childrenMap, statuses, alarmCounts);
                if (childNode != null)
                    children.Add(childNode);
            }
        }

        var directAlarms = alarmCounts.GetValueOrDefault(device.Id, 0);
        var descendantAlarms = children.Sum(c => c.AlarmCount);

        return new BessTreeNodeDto
        {
            Id = device.Id,
            Name = device.Name,
            Type = displayType,
            Status = statuses.GetValueOrDefault(device.Id, "normal"),
            ParentDeviceId = device.ParentDeviceId,
            AlarmCount = directAlarms + descendantAlarms,
            Children = children
        };
    }
}

internal static class BessDisplayTypeMapper
{
    public static string? MapToDisplayType(string? deviceTypeName) => deviceTypeName switch
    {
        DeviceTypeNames.EnergyStorage => "unit",
        DeviceTypeNames.EnergyStorageBattery => "battery",
        DeviceTypeNames.EnergyStorageRack => "rack",
        DeviceTypeNames.EnergyStoragePack => "pack",
        DeviceTypeNames.EnergyStorageModule => "module",
        DeviceTypeNames.EnergyStorageController => "esc",
        DeviceTypeNames.EnergyStorageTMS => "hvac",
        DeviceTypeNames.Pcs => "pcs",
        DeviceTypeNames.Solar => "solar",
        DeviceTypeNames.SmartMeter or "SmartMeter" => "meter",
        _ => null
    };
}

public record BessDeviceTreeResponseDto
{
    public List<BessTreeNodeDto> Tree { get; init; } = new();
}

public record BessTreeNodeDto
{
    public Guid Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string Type { get; init; } = string.Empty;
    public string Status { get; init; } = "normal";
    public Guid? ParentDeviceId { get; init; }
    public int AlarmCount { get; init; }
    public List<BessTreeNodeDto> Children { get; init; } = new();
}
