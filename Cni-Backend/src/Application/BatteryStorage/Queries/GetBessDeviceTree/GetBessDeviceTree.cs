using Backend.Application.Common.Interfaces;
using Backend.Application.Common.Security;
using Backend.Application.Devices.Queries.GetDeviceTree;
using Backend.Domain.Constants;

namespace Backend.Application.BatteryStorage.Queries.GetBessDeviceTree;

[Authorize(Roles = "Administrator,Tenant.Admin")]
[RequirePermission(Backend.Domain.Constants.Permissions.BessView)]
public record GetBessDeviceTreeQuery : IRequest<BessDeviceTreeResponseDto>;

public class GetBessDeviceTreeQueryHandler : IRequestHandler<GetBessDeviceTreeQuery, BessDeviceTreeResponseDto>
{
    private readonly IApplicationDbContext _context;

    public GetBessDeviceTreeQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<BessDeviceTreeResponseDto> Handle(GetBessDeviceTreeQuery request, CancellationToken cancellationToken)
    {
        // Find all root Battery devices
        var rootDevices = await _context.Devices
            .Include(d => d.DeviceType)
            .Where(d => d.DeviceType != null && d.DeviceType.Name == DeviceTypeNames.EnergyStorage)
            .OrderBy(d => d.Name)
            .ToListAsync(cancellationToken);

        if (rootDevices.Count == 0)
            return new BessDeviceTreeResponseDto();

        // Load all devices under the same RTUs to get full hierarchy
        var rtuIds = rootDevices.Select(d => d.RtuId).Distinct().ToList();
        var allDevices = await _context.Devices
            .Include(d => d.DeviceType)
            .Where(d => rtuIds.Contains(d.RtuId))
            .ToListAsync(cancellationToken);

        // Load alarm counts for all devices in one query
        var deviceIds = allDevices.Select(d => d.Id).ToList();
        var alarmCounts = await _context.DeviceAlarms
            .Where(a => deviceIds.Contains(a.DeviceId) && a.IsActive)
            .GroupBy(a => a.DeviceId)
            .Select(g => new { DeviceId = g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.DeviceId, x => x.Count, cancellationToken);

        // Build parent→children lookup
        var childrenMap = allDevices
            .Where(d => d.ParentDeviceId.HasValue)
            .GroupBy(d => d.ParentDeviceId!.Value)
            .ToDictionary(g => g.Key, g => g.OrderBy(d => d.Name).ToList());

        // Build a tree for each root BESS unit
        var trees = rootDevices
            .Select(root => DeviceTreeBuilder.BuildTreeNode(root, childrenMap, alarmCounts))
            .ToList();

        return new BessDeviceTreeResponseDto { Tree = trees };
    }
}

public record BessDeviceTreeResponseDto
{
    public List<DeviceTreeNodeDto> Tree { get; init; } = new();
}
