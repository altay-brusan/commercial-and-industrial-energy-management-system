namespace Backend.Domain.Entities;

public class DeviceAlarm : BaseAuditableEntity
{
    public Guid DeviceId { get; set; }
    public Guid DeviceAlarmTypeId { get; set; }
    public DateTimeOffset OccurredAt { get; set; }
    public DateTimeOffset? RecoveredAt { get; set; }
    public bool IsActive { get; set; }
    public bool IsRead { get; set; }
    public bool IsProcessed { get; set; }
    public string? Details { get; set; }

    /// <summary>
    /// RTU serial number that reported this alarm (denormalized for traceability).
    /// </summary>
    public string? RtuSerialNumber { get; set; }

    /// <summary>
    /// Device name at the time the alarm was raised (denormalized for display).
    /// </summary>
    public string? DeviceName { get; set; }

    // Acknowledgement
    public string? AcknowledgedById { get; set; }
    public DateTimeOffset? AcknowledgedAt { get; set; }

    // Resolution (human-level; separate from telemetry-level RecoveredAt)
    public string? ResolvedById { get; set; }
    public DateTimeOffset? ResolvedAt { get; set; }

    // Linked maintenance task
    public Guid? MaintenanceTaskId { get; set; }

    // Navigation
    public Device? Device { get; set; }
    public DeviceAlarmType? AlarmType { get; set; }
    public ApplicationUser? AcknowledgedBy { get; set; }
    public ApplicationUser? ResolvedBy { get; set; }
    public MaintenanceTask? MaintenanceTask { get; set; }
}
