using Backend.Application.Common.Interfaces;
using Backend.Application.Common.Security;

namespace Backend.Application.Devices.Queries.GetDeviceTree;

[Authorize(Roles = "Administrator,Tenant.Admin")]
[RequirePermission(Backend.Domain.Constants.Permissions.DeviceView)]
public record GetDeviceTreeQuery : IRequest<DeviceTreeNodeDto>
{
    public Guid DeviceId { get; init; }
}

public class GetDeviceTreeQueryHandler : IRequestHandler<GetDeviceTreeQuery, DeviceTreeNodeDto>
{
    private readonly IApplicationDbContext _context;

    public GetDeviceTreeQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<DeviceTreeNodeDto> Handle(GetDeviceTreeQuery request, CancellationToken cancellationToken)
    {
        // Load the root device
        var rootDevice = await _context.Devices
            .Include(d => d.DeviceType)
            .FirstOrDefaultAsync(d => d.Id == request.DeviceId, cancellationToken);

        Guard.Against.NotFound(request.DeviceId, rootDevice);

        // Load all descendants in a single query (devices that share the same RTU)
        var allDevices = await _context.Devices
            .Include(d => d.DeviceType)
            .Where(d => d.RtuId == rootDevice.RtuId)
            .ToListAsync(cancellationToken);

        // Load alarm counts for all device IDs
        var deviceIds = allDevices.Select(d => d.Id).ToList();
        var alarmCounts = await _context.DeviceAlarms
            .Where(a => deviceIds.Contains(a.DeviceId) && a.IsActive)
            .GroupBy(a => a.DeviceId)
            .Select(g => new { DeviceId = g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.DeviceId, x => x.Count, cancellationToken);

        // Build tree from flat list
        var deviceMap = allDevices.ToDictionary(d => d.Id);
        var childrenMap = allDevices
            .Where(d => d.ParentDeviceId.HasValue)
            .GroupBy(d => d.ParentDeviceId!.Value)
            .ToDictionary(g => g.Key, g => g.OrderBy(d => d.Name).ToList());

        return BuildTreeNode(rootDevice, childrenMap, alarmCounts);
    }

    private static DeviceTreeNodeDto BuildTreeNode(
        Domain.Entities.Device device,
        Dictionary<Guid, List<Domain.Entities.Device>> childrenMap,
        Dictionary<Guid, int> alarmCounts)
    {
        var children = new List<DeviceTreeNodeDto>();
        if (childrenMap.TryGetValue(device.Id, out var childDevices))
        {
            foreach (var child in childDevices)
            {
                children.Add(BuildTreeNode(child, childrenMap, alarmCounts));
            }
        }

        // Calculate active alarm count including descendants
        var directAlarms = alarmCounts.GetValueOrDefault(device.Id, 0);
        var descendantAlarms = children.Sum(c => c.ActiveAlarmCount);

        // Parse supported tabs from Specifications
        var availableTabs = new List<string>();
        if (!string.IsNullOrWhiteSpace(device.Specifications))
        {
            try
            {
                var specDoc = System.Text.Json.JsonDocument.Parse(device.Specifications);
                if (specDoc.RootElement.TryGetProperty("supportedTabs", out var tabsElement) &&
                    tabsElement.ValueKind == System.Text.Json.JsonValueKind.Array)
                {
                    foreach (var tab in tabsElement.EnumerateArray())
                    {
                        var isEnabled = !tab.TryGetProperty("isEnabled", out var enabled) || enabled.GetBoolean();
                        if (isEnabled && tab.TryGetProperty("key", out var key))
                            availableTabs.Add(key.GetString()!);
                    }
                }
            }
            catch
            {
                // Ignore parse errors
            }
        }

        return new DeviceTreeNodeDto
        {
            Id = device.Id,
            Name = device.Name,
            DeviceTypeName = device.DeviceType?.Name ?? string.Empty,
            Status = device.Status,
            ActiveAlarmCount = directAlarms + descendantAlarms,
            AvailableTabs = availableTabs,
            Children = children
        };
    }
}

public record DeviceTreeNodeDto
{
    public Guid Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string DeviceTypeName { get; init; } = string.Empty;
    public string? Status { get; init; }
    public int ActiveAlarmCount { get; init; }
    public List<string> AvailableTabs { get; init; } = new();
    public List<DeviceTreeNodeDto> Children { get; init; } = new();
}
