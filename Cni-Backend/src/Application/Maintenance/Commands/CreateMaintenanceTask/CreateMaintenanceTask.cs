using Backend.Application.Common.Interfaces;
using Backend.Application.Common.Security;
using Backend.Domain.Constants;
using Backend.Domain.Entities;

namespace Backend.Application.Maintenance.Commands.CreateMaintenanceTask;

[Authorize(Roles = "Administrator,Tenant.Admin")]
[RequirePermission(Backend.Domain.Constants.Permissions.MaintenanceCreate)]
public record CreateMaintenanceTaskCommand : IRequest<Guid>
{
    public string Title { get; init; } = string.Empty;
    public string? Description { get; init; }
    public string AssignedToId { get; init; } = string.Empty;
    public string Priority { get; init; } = string.Empty;
    public DateTimeOffset DueDate { get; init; }
    public Guid FacilityId { get; init; }
    public Guid? DeviceId { get; init; }
    public Guid? DeviceAlarmId { get; init; }
    public List<string>? ChecklistItems { get; init; }
}

public class CreateMaintenanceTaskCommandHandler : IRequestHandler<CreateMaintenanceTaskCommand, Guid>
{
    private readonly IApplicationDbContext _context;
    private readonly IUser _user;

    public CreateMaintenanceTaskCommandHandler(IApplicationDbContext context, IUser user)
    {
        _context = context;
        _user = user;
    }

    public async Task<Guid> Handle(CreateMaintenanceTaskCommand request, CancellationToken cancellationToken)
    {
        var initialStatus = request.DueDate > DateTimeOffset.UtcNow
            ? TaskStatuses.Scheduled
            : TaskStatuses.Pending;

        var task = new MaintenanceTask
        {
            Title = request.Title,
            Description = request.Description,
            Status = initialStatus,
            Priority = request.Priority,
            AssignedToId = request.AssignedToId,
            AssignedById = _user.Id!,
            DueDate = request.DueDate,
            FacilityId = request.FacilityId,
            DeviceId = request.DeviceId,
            DeviceAlarmId = request.DeviceAlarmId
        };

        // Add checklist items
        if (request.ChecklistItems != null)
        {
            for (var i = 0; i < request.ChecklistItems.Count; i++)
            {
                task.ChecklistItems.Add(new TaskChecklistItem
                {
                    Label = request.ChecklistItems[i],
                    SortOrder = i
                });
            }
        }

        // Add initial status change
        task.StatusChanges.Add(new TaskStatusChange
        {
            FromStatus = null,
            ToStatus = initialStatus,
            ChangedById = _user.Id!,
            ChangedAt = DateTimeOffset.UtcNow
        });

        _context.MaintenanceTasks.Add(task);

        // Link alarm if provided
        if (request.DeviceAlarmId.HasValue)
        {
            var alarm = await _context.DeviceAlarms
                .FirstOrDefaultAsync(a => a.Id == request.DeviceAlarmId.Value, cancellationToken);

            if (alarm != null)
            {
                alarm.MaintenanceTaskId = task.Id;
            }
        }

        await _context.SaveChangesAsync(cancellationToken);

        return task.Id;
    }
}
