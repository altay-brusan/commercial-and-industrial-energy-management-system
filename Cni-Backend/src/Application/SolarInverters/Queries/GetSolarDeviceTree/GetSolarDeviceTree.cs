using Backend.Application.Common.Interfaces;
using Backend.Application.Common.Security;
using Backend.Domain.Constants;

namespace Backend.Application.SolarInverters.Queries.GetSolarDeviceTree;

[Authorize(Roles = "Administrator,Tenant.Admin")]
[RequirePermission(Backend.Domain.Constants.Permissions.SolarView)]
public record GetSolarDeviceTreeQuery : IRequest<SolarDeviceTreeResponseDto>
{
    public Guid? FacilityId { get; init; }
}

public class GetSolarDeviceTreeQueryHandler : IRequestHandler<GetSolarDeviceTreeQuery, SolarDeviceTreeResponseDto>
{
    private readonly IApplicationDbContext _context;
    private readonly IDeviceStatusService _statusService;

    public GetSolarDeviceTreeQueryHandler(IApplicationDbContext context, IDeviceStatusService statusService)
    {
        _context = context;
        _statusService = statusService;
    }

    public async Task<SolarDeviceTreeResponseDto> Handle(GetSolarDeviceTreeQuery request, CancellationToken cancellationToken)
    {
        // Find root Solar/Inverter devices
        var rootQuery = _context.Devices
            .Include(d => d.DeviceType)
            .Include(d => d.LogicalDevice)
            .Where(d =>
                (d.DeviceType != null && (d.DeviceType.Name == DeviceTypeNames.Solar || d.DeviceType.Name == DeviceTypeNames.Inverter)) ||
                (d.LogicalDevice != null && d.LogicalDevice.Category == "Solar"));

        if (request.FacilityId.HasValue)
            rootQuery = rootQuery.Where(d => d.Rtu!.FacilityId == request.FacilityId.Value);

        var rootDevices = await rootQuery
            .OrderBy(d => d.Name)
            .ToListAsync(cancellationToken);

        if (rootDevices.Count == 0)
            return new SolarDeviceTreeResponseDto();

        // Load all devices under the same RTUs to get child MPPT/String devices
        var rtuIds = rootDevices.Select(d => d.RtuId).Distinct().ToList();
        var allDevices = await _context.Devices
            .Include(d => d.DeviceType)
            .Include(d => d.LogicalDevice)
            .Where(d => rtuIds.Contains(d.RtuId))
            .ToListAsync(cancellationToken);

        var deviceIds = allDevices.Select(d => d.Id).ToList();

        var statuses = await _statusService.GetStatusBatchAsync(deviceIds, cancellationToken);

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

        var trees = new List<SolarTreeNodeDto>();
        foreach (var root in rootDevices)
        {
            var node = BuildTreeNode(root, childrenMap, statuses, alarmCounts);
            trees.Add(node);
        }

        return new SolarDeviceTreeResponseDto { Tree = trees };
    }

    private static SolarTreeNodeDto BuildTreeNode(
        Domain.Entities.Device device,
        Dictionary<Guid, List<Domain.Entities.Device>> childrenMap,
        Dictionary<Guid, string> statuses,
        Dictionary<Guid, int> alarmCounts)
    {
        var children = new List<SolarTreeNodeDto>();
        if (childrenMap.TryGetValue(device.Id, out var childDevices))
        {
            foreach (var child in childDevices)
            {
                children.Add(BuildTreeNode(child, childrenMap, statuses, alarmCounts));
            }
        }

        var directAlarms = alarmCounts.GetValueOrDefault(device.Id, 0);
        var descendantAlarms = children.Sum(c => c.AlarmCount);

        var displayType = MapDisplayType(device.DeviceType?.Name, device.LogicalDevice?.Name);

        return new SolarTreeNodeDto
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

    private static string MapDisplayType(string? deviceTypeName, string? logicalDeviceName) =>
        deviceTypeName switch
        {
            DeviceTypeNames.Solar or DeviceTypeNames.Inverter => "inverter",
            _ => logicalDeviceName?.ToLowerInvariant() switch
            {
                "solarinverter" => "inverter",
                "mppttracker" => "mppt",
                "solarstring" => "string",
                _ => "device"
            }
        };
}

public record SolarDeviceTreeResponseDto
{
    public List<SolarTreeNodeDto> Tree { get; init; } = new();
}

public record SolarTreeNodeDto
{
    public Guid Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string Type { get; init; } = "inverter";
    public string Status { get; init; } = "normal";
    public Guid? ParentDeviceId { get; init; }
    public int AlarmCount { get; init; }
    public List<SolarTreeNodeDto> Children { get; init; } = new();
}
