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

    // Navigation
    public Device? Device { get; set; }
    public DeviceAlarmType? AlarmType { get; set; }
}
