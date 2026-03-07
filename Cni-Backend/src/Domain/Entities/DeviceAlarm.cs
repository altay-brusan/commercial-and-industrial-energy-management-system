namespace Backend.Domain.Entities;

public class DeviceAlarm : BaseAuditableEntity
{
    public Guid DeviceId { get; set; }
    public Guid DeviceAlarmTypeId { get; set; }

    /// <summary>
    /// Sub-device path within the physical device where the alarm occurred.
    /// Null for device-level alarms. Examples: "Rack/2", "Rack/2/Pack/1".
    /// </summary>
    public string? Path { get; set; }

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
