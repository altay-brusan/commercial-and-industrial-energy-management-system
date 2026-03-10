using Backend.Application.Common.Interfaces;
using Backend.Application.Common.Security;

namespace Backend.Application.LogicalDevices.Queries;

[Authorize(Roles = "Administrator,Tenant.Admin")]
[RequirePermission(Backend.Domain.Constants.Permissions.DeviceView)]
public record GetLogicalDeviceQuery : IRequest<LogicalDeviceDto>
{
    public Guid Id { get; init; }
}

public class GetLogicalDeviceQueryHandler : IRequestHandler<GetLogicalDeviceQuery, LogicalDeviceDto>
{
    private readonly IApplicationDbContext _context;

    public GetLogicalDeviceQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<LogicalDeviceDto> Handle(GetLogicalDeviceQuery request, CancellationToken cancellationToken)
    {
        var entity = await _context.LogicalDevices
            .Include(d => d.LogicalDeviceNodes)
                .ThenInclude(ldn => ldn.LogicalNode)
            .FirstOrDefaultAsync(d => d.Id == request.Id, cancellationToken);

        Guard.Against.NotFound(request.Id, entity);

        return new LogicalDeviceDto
        {
            Id = entity.Id,
            Name = entity.Name,
            DisplayName = entity.DisplayName,
            Description = entity.Description,
            Category = entity.Category,
            Ports = entity.Ports,
            HierarchyTemplate = entity.HierarchyTemplate,
            UiTemplate = entity.UiTemplate,
            Nodes = entity.LogicalDeviceNodes
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
        };
    }
}
