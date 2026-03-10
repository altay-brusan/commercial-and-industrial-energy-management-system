using Backend.Application.Common.Interfaces;
using Backend.Application.Common.Security;
using Backend.Domain.Constants;
using Backend.Domain.Entities;
using FluentValidation.Results;

namespace Backend.Application.Maintenance.Commands.UpdateMaintenanceTaskStatus;

[Authorize(Roles = "Administrator,Tenant.Admin")]
[RequirePermission(Backend.Domain.Constants.Permissions.MaintenanceManage)]
public record UpdateMaintenanceTaskStatusCommand : IRequest
{
    public Guid Id { get; init; }
    public string NewStatus { get; init; } = string.Empty;
}

public class UpdateMaintenanceTaskStatusCommandHandler : IRequestHandler<UpdateMaintenanceTaskStatusCommand>
{
    private readonly IApplicationDbContext _context;
    private readonly IUser _user;

    private static readonly Dictionary<string, string[]> ValidTransitions = new()
    {
        { TaskStatuses.Scheduled, new[] { TaskStatuses.Pending, TaskStatuses.Closed } },
        { TaskStatuses.Pending, new[] { TaskStatuses.InProgress, TaskStatuses.Closed } },
        { TaskStatuses.InProgress, new[] { TaskStatuses.Closed } }, // Completed via CompleteTask
        { TaskStatuses.Overdue, new[] { TaskStatuses.InProgress, TaskStatuses.Closed } },
    };

    public UpdateMaintenanceTaskStatusCommandHandler(IApplicationDbContext context, IUser user)
    {
        _context = context;
        _user = user;
    }

    public async Task Handle(UpdateMaintenanceTaskStatusCommand request, CancellationToken cancellationToken)
    {
        var task = await _context.MaintenanceTasks
            .FirstOrDefaultAsync(t => t.Id == request.Id, cancellationToken);

        if (task == null)
            throw new NotFoundException(nameof(MaintenanceTask), request.Id.ToString());

        if (!ValidTransitions.TryGetValue(task.Status, out var allowed) || !allowed.Contains(request.NewStatus))
            throw new Common.Exceptions.ValidationException(
                new[] { new ValidationFailure("NewStatus", $"Cannot transition from '{task.Status}' to '{request.NewStatus}'.") });

        var previousStatus = task.Status;
        task.Status = request.NewStatus;

        _context.TaskStatusChanges.Add(new TaskStatusChange
        {
            MaintenanceTaskId = task.Id,
            FromStatus = previousStatus,
            ToStatus = request.NewStatus,
            ChangedById = _user.Id!,
            ChangedAt = DateTimeOffset.UtcNow
        });

        await _context.SaveChangesAsync(cancellationToken);
    }
}
