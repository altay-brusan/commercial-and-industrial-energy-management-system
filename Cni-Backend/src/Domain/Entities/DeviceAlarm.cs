namespace Backend.Domain.Entities;

public class DeviceAlarm : BaseAuditableEntity
{
    public Guid DeviceId { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Severity { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public string? Source { get; set; }
    public string? FaultKey { get; set; }
    public DateTimeOffset OccurredAt { get; set; }
    public DateTimeOffset? RecoveredAt { get; set; }
    public bool IsActive { get; set; }
    public string? Details { get; set; }

    // Navigation
    public Device? Device { get; set; }
}
