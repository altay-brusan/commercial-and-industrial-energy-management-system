using Backend.Application.Common.Interfaces;
using Backend.Application.Common.Security;
using Backend.Domain.Entities;

namespace Backend.Application.LogicalDevices.Commands;

[Authorize(Roles = "Administrator")]
[RequirePermission(Backend.Domain.Constants.Permissions.DeviceCreate)]
public record UpdateLogicalDeviceCommand : IRequest
{
    public Guid Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string? DisplayName { get; init; }
    public string? Description { get; init; }
    public string Category { get; init; } = string.Empty;
    public string Ports { get; init; } = "[]";
    public string? HierarchyTemplate { get; init; }
    public string? UiTemplate { get; init; }
    public List<NodeAssignment> NodeAssignments { get; init; } = new();
}

public class UpdateLogicalDeviceCommandHandler : IRequestHandler<UpdateLogicalDeviceCommand>
{
    private readonly IApplicationDbContext _context;

    public UpdateLogicalDeviceCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task Handle(UpdateLogicalDeviceCommand request, CancellationToken cancellationToken)
    {
        var entity = await _context.LogicalDevices
            .Include(d => d.LogicalDeviceNodes)
            .FirstOrDefaultAsync(d => d.Id == request.Id, cancellationToken);

        Guard.Against.NotFound(request.Id, entity);

        entity.Name = request.Name;
        entity.DisplayName = request.DisplayName;
        entity.Description = request.Description;
        entity.Category = request.Category;
        entity.Ports = request.Ports;
        entity.HierarchyTemplate = request.HierarchyTemplate;
        entity.UiTemplate = request.UiTemplate;

        // Replace node assignments
        entity.LogicalDeviceNodes.Clear();
        foreach (var assignment in request.NodeAssignments)
        {
            entity.LogicalDeviceNodes.Add(new LogicalDeviceNode
            {
                LogicalDeviceId = entity.Id,
                LogicalNodeId = assignment.LogicalNodeId,
                NodeAlias = assignment.NodeAlias,
                InstanceCount = assignment.InstanceCount,
                SortOrder = assignment.SortOrder
            });
        }

        await _context.SaveChangesAsync(cancellationToken);
    }
}
