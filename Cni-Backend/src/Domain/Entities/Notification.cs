namespace Backend.Domain.Entities;

public class Notification : BaseAuditableEntity
{
    public string UserId { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public bool IsRead { get; set; }
    public DateTimeOffset Timestamp { get; set; }
    public Guid? RelatedAlarmId { get; set; }
    public Guid? RelatedTaskId { get; set; }

    // Navigation
    public ApplicationUser? User { get; set; }
    public DeviceAlarm? RelatedAlarm { get; set; }
    public MaintenanceTask? RelatedTask { get; set; }
}
