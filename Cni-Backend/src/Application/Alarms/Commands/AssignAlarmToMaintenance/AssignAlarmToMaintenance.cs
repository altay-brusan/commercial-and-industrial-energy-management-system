using Backend.Application.Alarms.Queries;
using Backend.Application.Common.Interfaces;
using Backend.Application.Common.Security;
using Backend.Domain.Constants;
using Backend.Domain.Entities;
using FluentValidation.Results;

namespace Backend.Application.Alarms.Commands.AssignAlarmToMaintenance;

/// <summary>
/// Response DTO for the AssignToMaintenance endpoint.
/// Returns both the created task and the updated alarm.
/// </summary>
public record AssignAlarmToMaintenanceResponseDto
{
    public AssignedMaintenanceTaskDto Task { get; init; } = null!;
    public AlarmDto Alarm { get; init; } = null!;
}

/// <summary>
/// Task DTO matching the frontend contract for the assign-to-maintenance response.
/// Uses user objects, relatedAlarm sub-object, completionDetails/closeDetails nullable objects.
/// </summary>
public record AssignedMaintenanceTaskDto
{
    public Guid Id { get; init; }
    public string Title { get; init; } = string.Empty;
    public string? Description { get; init; }
    public string Status { get; init; } = string.Empty;
    public string Priority { get; init; } = string.Empty;
    public AlarmUserDto? AssignedTo { get; init; }
    public AlarmUserDto? AssignedBy { get; init; }
    public DateTimeOffset AssignedDate { get; init; }
    public DateTimeOffset DueDate { get; init; }
    public AlarmRelatedAssetDto? RelatedAsset { get; init; }
    public TaskRelatedAlarmDto? RelatedAlarm { get; init; }
    public List<object> Checklist { get; init; } = new();
    public List<object> WorkSessions { get; init; } = new();
    public object? CompletionDetails { get; init; }
    public object? CloseDetails { get; init; }
    public DateTimeOffset? DeletedAt { get; init; }
    public DateTimeOffset CreatedAt { get; init; }
    public DateTimeOffset UpdatedAt { get; init; }
}

/// <summary>
/// Alarm sub-object within the task response — lighter shape than full AlarmDto.
/// </summary>
public record TaskRelatedAlarmDto
{
    public Guid Id { get; init; }
    public string Code { get; init; } = string.Empty;
    public string? Message { get; init; }
    public string Severity { get; init; } = string.Empty;
    public string? Source { get; init; }
    public DateTimeOffset TriggeredAt { get; init; }
    public string? Value { get; init; }
    public string? Threshold { get; init; }
    public DateTimeOffset? AcknowledgedAt { get; init; }
    public AlarmUserDto? AcknowledgedBy { get; init; }
}

[Authorize(Roles = "Administrator,Tenant.Admin")]
[RequirePermission(Backend.Domain.Constants.Permissions.AlarmManage)]
public record AssignAlarmToMaintenanceCommand : IRequest<AssignAlarmToMaintenanceResponseDto>
{
    public Guid AlarmId { get; init; }
    public string Title { get; init; } = string.Empty;
    public string? Description { get; init; }
    public string Priority { get; init; } = string.Empty;
    public string AssignedToId { get; init; } = string.Empty;
    public DateTimeOffset DueDate { get; init; }
}

public class AssignAlarmToMaintenanceCommandHandler
    : IRequestHandler<AssignAlarmToMaintenanceCommand, AssignAlarmToMaintenanceResponseDto>
{
    private readonly IApplicationDbContext _context;
    private readonly IUser _user;

    public AssignAlarmToMaintenanceCommandHandler(IApplicationDbContext context, IUser user)
    {
        _context = context;
        _user = user;
    }

    public async Task<AssignAlarmToMaintenanceResponseDto> Handle(
        AssignAlarmToMaintenanceCommand request, CancellationToken cancellationToken)
    {
        // Load alarm with device chain to derive facilityId
        var alarm = await _context.DeviceAlarms
            .Include(a => a.AlarmType)
            .Include(a => a.Device)
                .ThenInclude(d => d!.Rtu)
                    .ThenInclude(r => r!.Facility)
            .Include(a => a.AcknowledgedBy)
            .Include(a => a.ResolvedBy)
            .FirstOrDefaultAsync(a => a.Id == request.AlarmId, cancellationToken);

        if (alarm == null)
            throw new NotFoundException(nameof(DeviceAlarm), request.AlarmId.ToString());

        if (!alarm.IsActive)
            throw new Common.Exceptions.ValidationException(
                new[] { new ValidationFailure("AlarmId", "Alarm is not active.") });

        if (alarm.MaintenanceTaskId != null)
            throw new Common.Exceptions.ValidationException(
                new[] { new ValidationFailure("AlarmId", "Alarm already has a linked maintenance task.") });

        // Derive facilityId from alarm's device chain
        var facilityId = alarm.Device?.Rtu?.Facility?.Id
            ?? throw new Common.Exceptions.ValidationException(
                new[] { new ValidationFailure("AlarmId", "Cannot determine facility from alarm's device.") });

        var initialStatus = request.DueDate > DateTimeOffset.UtcNow
            ? TaskStatuses.Scheduled
            : TaskStatuses.Pending;

        var now = DateTimeOffset.UtcNow;

        var task = new MaintenanceTask
        {
            Title = request.Title,
            Description = request.Description,
            Status = initialStatus,
            Priority = request.Priority,
            AssignedToId = request.AssignedToId,
            AssignedById = _user.Id!,
            DueDate = request.DueDate,
            FacilityId = facilityId,
            DeviceId = alarm.DeviceId,
            DeviceAlarmId = alarm.Id
        };

        // Add initial status change
        task.StatusChanges.Add(new TaskStatusChange
        {
            FromStatus = null,
            ToStatus = initialStatus,
            ChangedById = _user.Id!,
            ChangedAt = now
        });

        _context.MaintenanceTasks.Add(task);

        // Link alarm to task
        alarm.MaintenanceTaskId = task.Id;

        await _context.SaveChangesAsync(cancellationToken);

        // Re-query task with includes for response
        var createdTask = await _context.MaintenanceTasks
            .Include(t => t.AssignedTo)
            .Include(t => t.AssignedBy)
            .Include(t => t.Device)
                .ThenInclude(d => d!.Rtu)
                    .ThenInclude(r => r!.Facility)
            .FirstAsync(t => t.Id == task.Id, cancellationToken);

        // Re-query alarm with full includes for response
        var updatedAlarm = await _context.DeviceAlarms
            .Include(a => a.AlarmType)
            .Include(a => a.Device)
                .ThenInclude(d => d!.Rtu)
                    .ThenInclude(r => r!.Facility)
            .Include(a => a.AcknowledgedBy)
            .Include(a => a.ResolvedBy)
            .FirstAsync(a => a.Id == alarm.Id, cancellationToken);

        return new AssignAlarmToMaintenanceResponseDto
        {
            Task = MapTask(createdTask, updatedAlarm),
            Alarm = AlarmDtoMapper.ToDto(updatedAlarm)
        };
    }

    private static AssignedMaintenanceTaskDto MapTask(MaintenanceTask task, DeviceAlarm alarm)
    {
        var alarmDto = AlarmDtoMapper.ToDto(alarm);

        return new AssignedMaintenanceTaskDto
        {
            Id = task.Id,
            Title = task.Title,
            Description = task.Description,
            Status = task.Status.ToLower(),
            Priority = task.Priority.ToLower(),
            AssignedTo = AlarmDtoMapper.MapUser(task.AssignedTo),
            AssignedBy = AlarmDtoMapper.MapUser(task.AssignedBy),
            AssignedDate = task.Created,
            DueDate = task.DueDate,
            RelatedAsset = task.Device != null ? new AlarmRelatedAssetDto
            {
                Id = task.Device.Id,
                Name = task.Device.Name,
                Type = alarm.AlarmType?.Category?.ToLower(),
                Location = task.Device.Rtu?.Facility?.Name,
                Status = task.Device.Status,
                SerialNumber = task.Device.SerialNumber,
                Model = task.Device.Model
            } : null,
            RelatedAlarm = new TaskRelatedAlarmDto
            {
                Id = alarm.Id,
                Code = alarmDto.Code,
                Message = alarmDto.Message,
                Severity = alarmDto.Severity,
                Source = alarmDto.Source,
                TriggeredAt = alarmDto.TriggeredAt,
                Value = alarmDto.Value,
                Threshold = alarmDto.Threshold,
                AcknowledgedAt = alarmDto.AcknowledgedAt,
                AcknowledgedBy = alarmDto.AcknowledgedBy
            },
            Checklist = new List<object>(),
            WorkSessions = new List<object>(),
            CompletionDetails = null,
            CloseDetails = null,
            DeletedAt = null,
            CreatedAt = task.Created,
            UpdatedAt = task.LastModified
        };
    }
}
