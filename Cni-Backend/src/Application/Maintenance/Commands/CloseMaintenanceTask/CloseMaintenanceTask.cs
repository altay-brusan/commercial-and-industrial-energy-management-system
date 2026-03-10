using Backend.Application.Common.Interfaces;
using Backend.Application.Common.Security;
using Backend.Domain.Constants;
using Backend.Domain.Entities;
using FluentValidation.Results;

namespace Backend.Application.Maintenance.Commands.CloseMaintenanceTask;

[Authorize(Roles = "Administrator,Tenant.Admin")]
[RequirePermission(Backend.Domain.Constants.Permissions.MaintenanceManage)]
public record CloseMaintenanceTaskCommand : IRequest
{
    public Guid Id { get; init; }
    public string Reason { get; init; } = string.Empty;
    public string? Explanation { get; init; }
}

public class CloseMaintenanceTaskCommandHandler : IRequestHandler<CloseMaintenanceTaskCommand>
{
    private readonly IApplicationDbContext _context;
    private readonly IUser _user;

    public CloseMaintenanceTaskCommandHandler(IApplicationDbContext context, IUser user)
    {
        _context = context;
        _user = user;
    }

    public async Task Handle(CloseMaintenanceTaskCommand request, CancellationToken cancellationToken)
    {
        var task = await _context.MaintenanceTasks
            .FirstOrDefaultAsync(t => t.Id == request.Id, cancellationToken);

        if (task == null)
            throw new NotFoundException(nameof(MaintenanceTask), request.Id.ToString());

        if (task.Status == TaskStatuses.Closed || task.Status == TaskStatuses.Completed)
            throw new Common.Exceptions.ValidationException(
                new[] { new ValidationFailure("Id", $"Task is already '{task.Status}' and cannot be closed.") });

        var previousStatus = task.Status;
        task.Status = TaskStatuses.Closed;
        task.CloseReason = request.Reason;
        task.CloseExplanation = request.Explanation;
        task.ClosedAt = DateTimeOffset.UtcNow;
        task.ClosedById = _user.Id;

        _context.TaskStatusChanges.Add(new TaskStatusChange
        {
            MaintenanceTaskId = task.Id,
            FromStatus = previousStatus,
            ToStatus = TaskStatuses.Closed,
            ChangedById = _user.Id!,
            ChangedAt = DateTimeOffset.UtcNow
        });

        // Auto-resolve linked alarm
        if (task.DeviceAlarmId.HasValue)
        {
            var alarm = await _context.DeviceAlarms
                .FirstOrDefaultAsync(a => a.Id == task.DeviceAlarmId.Value, cancellationToken);

            if (alarm != null && alarm.IsActive)
            {
                alarm.IsActive = false;
                alarm.ResolvedAt = DateTimeOffset.UtcNow;
                alarm.ResolvedById = _user.Id;
            }
        }

        await _context.SaveChangesAsync(cancellationToken);
    }
}
