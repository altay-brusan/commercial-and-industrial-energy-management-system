using Backend.Application.Common.Interfaces;
using Backend.Application.Common.Security;
using Backend.Domain.Entities;

namespace Backend.Application.MaintenanceTasks.Commands.UpdateChecklistItem;

[Authorize(Roles = "Administrator,Tenant.Admin")]
[RequirePermission(Backend.Domain.Constants.Permissions.MaintenanceManage)]
public record UpdateChecklistItemCommand : IRequest
{
    public Guid TaskId { get; init; }
    public Guid ChecklistItemId { get; init; }
    public bool IsCompleted { get; init; }
}

public class UpdateChecklistItemCommandHandler : IRequestHandler<UpdateChecklistItemCommand>
{
    private readonly IApplicationDbContext _context;
    private readonly IUser _user;

    public UpdateChecklistItemCommandHandler(IApplicationDbContext context, IUser user)
    {
        _context = context;
        _user = user;
    }

    public async Task Handle(UpdateChecklistItemCommand request, CancellationToken cancellationToken)
    {
        var item = await _context.TaskChecklistItems
            .FirstOrDefaultAsync(c => c.Id == request.ChecklistItemId && c.MaintenanceTaskId == request.TaskId, cancellationToken);

        if (item == null)
            throw new NotFoundException(nameof(TaskChecklistItem), request.ChecklistItemId.ToString());

        item.IsCompleted = request.IsCompleted;

        if (request.IsCompleted)
        {
            item.CompletedAt = DateTimeOffset.UtcNow;
            item.CompletedById = _user.Id;
        }
        else
        {
            item.CompletedAt = null;
            item.CompletedById = null;
        }

        await _context.SaveChangesAsync(cancellationToken);
    }
}
