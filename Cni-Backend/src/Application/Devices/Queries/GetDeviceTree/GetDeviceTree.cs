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
        var rootDevice = await _context.Devices
            .Include(d => d.DeviceType)
            .FirstOrDefaultAsync(d => d.Id == request.DeviceId, cancellationToken);

        Guard.Against.NotFound(request.DeviceId, rootDevice);

        var allDevices = await _context.Devices
            .Include(d => d.DeviceType)
            .Where(d => d.RtuId == rootDevice.RtuId)
            .ToListAsync(cancellationToken);

        var deviceIds = allDevices.Select(d => d.Id).ToList();
        var alarmCounts = await _context.DeviceAlarms
            .Where(a => deviceIds.Contains(a.DeviceId) && a.IsActive)
            .GroupBy(a => a.DeviceId)
            .Select(g => new { DeviceId = g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.DeviceId, x => x.Count, cancellationToken);

        var childrenMap = allDevices
            .Where(d => d.ParentDeviceId.HasValue)
            .GroupBy(d => d.ParentDeviceId!.Value)
            .ToDictionary(g => g.Key, g => g.OrderBy(d => d.Name).ToList());

        return DeviceTreeBuilder.BuildTreeNode(rootDevice, childrenMap, alarmCounts);
    }
}

public static class DeviceTreeBuilder
{
    public static DeviceTreeNodeDto BuildTreeNode(
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

        var directAlarms = alarmCounts.GetValueOrDefault(device.Id, 0);
        var descendantAlarms = children.Sum(c => c.AlarmCount);

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
            Type = MapDeviceType(device.DeviceType?.Name),
            DeviceTypeName = device.DeviceType?.Name ?? string.Empty,
            Status = device.Status,
            AlarmCount = directAlarms + descendantAlarms,
            AvailableTabs = availableTabs,
            Children = children
        };
    }

    public static string MapDeviceType(string? deviceTypeName) => deviceTypeName switch
    {
        "Battery" => "unit",
        "Battery.Rack" => "rack",
        "Battery.Pack" => "pack",
        "Battery.Module" => "module",
        "Battery.Controller" => "esc",
        "Battery.TMS" => "hvac",
        "PCS" => "pcs",
        "Solar" => "solar",
        "SmartMeter" => "meter",
        _ => deviceTypeName?.Split('.').Last().ToLowerInvariant() ?? "unknown"
    };
}

public record DeviceTreeNodeDto
{
    public Guid Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string Type { get; init; } = string.Empty;
    public string DeviceTypeName { get; init; } = string.Empty;
    public string? Status { get; init; }
    public int AlarmCount { get; init; }
    public List<string> AvailableTabs { get; init; } = new();
    public List<DeviceTreeNodeDto> Children { get; init; } = new();
}
