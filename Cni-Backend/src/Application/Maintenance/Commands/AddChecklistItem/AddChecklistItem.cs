using Backend.Application.Common.Interfaces;
using Backend.Application.Common.Security;
using Backend.Domain.Entities;

namespace Backend.Application.Maintenance.Commands.AddChecklistItem;

[Authorize(Roles = "Administrator,Tenant.Admin")]
[RequirePermission(Backend.Domain.Constants.Permissions.MaintenanceManage)]
public record AddChecklistItemCommand : IRequest<Guid>
{
    public Guid TaskId { get; init; }
    public string Label { get; init; } = string.Empty;
}

public class AddChecklistItemCommandHandler : IRequestHandler<AddChecklistItemCommand, Guid>
{
    private readonly IApplicationDbContext _context;

    public AddChecklistItemCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Guid> Handle(AddChecklistItemCommand request, CancellationToken cancellationToken)
    {
        var taskExists = await _context.MaintenanceTasks
            .AnyAsync(t => t.Id == request.TaskId, cancellationToken);

        if (!taskExists)
            throw new NotFoundException(nameof(MaintenanceTask), request.TaskId.ToString());

        var maxOrder = await _context.TaskChecklistItems
            .Where(c => c.MaintenanceTaskId == request.TaskId)
            .Select(c => (int?)c.SortOrder)
            .MaxAsync(cancellationToken) ?? -1;

        var item = new TaskChecklistItem
        {
            MaintenanceTaskId = request.TaskId,
            Label = request.Label,
            SortOrder = maxOrder + 1
        };

        _context.TaskChecklistItems.Add(item);
        await _context.SaveChangesAsync(cancellationToken);

        return item.Id;
    }
}
