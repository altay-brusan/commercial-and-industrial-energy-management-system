using Backend.Application.Common.Interfaces;
using Backend.Application.Common.Security;

namespace Backend.Application.LogicalDevices.Queries;

[Authorize(Roles = "Administrator,Tenant.Admin")]
[RequirePermission(Backend.Domain.Constants.Permissions.DeviceView)]
public record GetLogicalDevicesQuery : IRequest<List<LogicalDeviceDto>>
{
    public string? Category { get; init; }
}

public class GetLogicalDevicesQueryHandler : IRequestHandler<GetLogicalDevicesQuery, List<LogicalDeviceDto>>
{
    private readonly IApplicationDbContext _context;

    public GetLogicalDevicesQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<LogicalDeviceDto>> Handle(GetLogicalDevicesQuery request, CancellationToken cancellationToken)
    {
        var query = _context.LogicalDevices
            .Include(d => d.LogicalDeviceNodes)
                .ThenInclude(ldn => ldn.LogicalNode)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(request.Category))
            query = query.Where(d => d.Category == request.Category);

        return await query
            .OrderBy(d => d.Category)
            .ThenBy(d => d.Name)
            .Select(d => new LogicalDeviceDto
            {
                Id = d.Id,
                Name = d.Name,
                DisplayName = d.DisplayName,
                Description = d.Description,
                Category = d.Category,
                Ports = d.Ports,
                HierarchyTemplate = d.HierarchyTemplate,
                UiTemplate = d.UiTemplate,
                Nodes = d.LogicalDeviceNodes
                    .OrderBy(n => n.SortOrder)
                    .Select(n => new LogicalDeviceNodeDto
                    {
                        LogicalNodeId = n.LogicalNodeId,
                        LogicalNodeName = n.LogicalNode!.Name,
                        NodeAlias = n.NodeAlias,
                        InstanceCount = n.InstanceCount,
                        SortOrder = n.SortOrder
                    })
                    .ToList()
            })
            .ToListAsync(cancellationToken);
    }
}
