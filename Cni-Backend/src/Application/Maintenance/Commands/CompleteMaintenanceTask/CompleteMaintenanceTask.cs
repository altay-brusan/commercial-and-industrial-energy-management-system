using Backend.Application.Common.Interfaces;
using Backend.Application.Common.Security;
using Backend.Domain.Constants;
using Backend.Domain.Entities;
using FluentValidation.Results;

namespace Backend.Application.Maintenance.Commands.CompleteMaintenanceTask;

[Authorize(Roles = "Administrator,Tenant.Admin")]
[RequirePermission(Backend.Domain.Constants.Permissions.MaintenanceManage)]
public record CompleteMaintenanceTaskCommand : IRequest
{
    public Guid Id { get; init; }
    public string Resolution { get; init; } = string.Empty;
    public string? RootCause { get; init; }
    public decimal? LaborHours { get; init; }
    public string? PartsUsed { get; init; }
    public string? Recommendations { get; init; }
}

public class CompleteMaintenanceTaskCommandHandler : IRequestHandler<CompleteMaintenanceTaskCommand>
{
    private readonly IApplicationDbContext _context;
    private readonly IUser _user;

    public CompleteMaintenanceTaskCommandHandler(IApplicationDbContext context, IUser user)
    {
        _context = context;
        _user = user;
    }

    public async Task Handle(CompleteMaintenanceTaskCommand request, CancellationToken cancellationToken)
    {
        var task = await _context.MaintenanceTasks
            .FirstOrDefaultAsync(t => t.Id == request.Id, cancellationToken);

        if (task == null)
            throw new NotFoundException(nameof(MaintenanceTask), request.Id.ToString());

        if (task.Status != TaskStatuses.InProgress && task.Status != TaskStatuses.Overdue)
            throw new Common.Exceptions.ValidationException(
                new[] { new ValidationFailure("Id", $"Task must be InProgress or Overdue to complete. Current: '{task.Status}'.") });

        var previousStatus = task.Status;
        task.Status = TaskStatuses.Completed;
        task.Resolution = request.Resolution;
        task.RootCause = request.RootCause;
        task.LaborHours = request.LaborHours;
        task.PartsUsed = request.PartsUsed;
        task.Recommendations = request.Recommendations;
        task.CompletedAt = DateTimeOffset.UtcNow;
        task.CompletedById = _user.Id;

        task.StatusChanges.Add(new TaskStatusChange
        {
            FromStatus = previousStatus,
            ToStatus = TaskStatuses.Completed,
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
