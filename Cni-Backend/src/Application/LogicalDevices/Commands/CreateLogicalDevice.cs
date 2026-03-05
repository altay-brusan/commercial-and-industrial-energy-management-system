using Backend.Application.Common.Interfaces;
using Backend.Application.Common.Security;
using Backend.Domain.Entities;

namespace Backend.Application.LogicalDevices.Commands;

public record NodeAssignment
{
    public Guid LogicalNodeId { get; init; }
    public string? NodeAlias { get; init; }
    public int InstanceCount { get; init; } = 1;
    public int SortOrder { get; init; }
}

[Authorize(Roles = "Administrator")]
[RequirePermission(Backend.Domain.Constants.Permissions.DeviceCreate)]
public record CreateLogicalDeviceCommand : IRequest<Guid>
{
    public string Name { get; init; } = string.Empty;
    public string? DisplayName { get; init; }
    public string? Description { get; init; }
    public string Category { get; init; } = string.Empty;
    public string Ports { get; init; } = "[]";
    public string? HierarchyTemplate { get; init; }
    public string? UiTemplate { get; init; }
    public List<NodeAssignment> NodeAssignments { get; init; } = new();
}

public class CreateLogicalDeviceCommandHandler : IRequestHandler<CreateLogicalDeviceCommand, Guid>
{
    private readonly IApplicationDbContext _context;

    public CreateLogicalDeviceCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Guid> Handle(CreateLogicalDeviceCommand request, CancellationToken cancellationToken)
    {
        var entity = new LogicalDevice
        {
            Name = request.Name,
            DisplayName = request.DisplayName,
            Description = request.Description,
            Category = request.Category,
            Ports = request.Ports,
            HierarchyTemplate = request.HierarchyTemplate,
            UiTemplate = request.UiTemplate
        };

        foreach (var assignment in request.NodeAssignments)
        {
            entity.LogicalDeviceNodes.Add(new LogicalDeviceNode
            {
                LogicalNodeId = assignment.LogicalNodeId,
                NodeAlias = assignment.NodeAlias,
                InstanceCount = assignment.InstanceCount,
                SortOrder = assignment.SortOrder
            });
        }

        _context.LogicalDevices.Add(entity);
        await _context.SaveChangesAsync(cancellationToken);
        return entity.Id;
    }
}
