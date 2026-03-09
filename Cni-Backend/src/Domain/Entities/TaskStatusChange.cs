namespace Backend.Domain.Entities;

public class TaskStatusChange : BaseAuditableEntity
{
    public Guid MaintenanceTaskId { get; set; }
    public string? FromStatus { get; set; }
    public string ToStatus { get; set; } = string.Empty;
    public string ChangedById { get; set; } = string.Empty;
    public DateTimeOffset ChangedAt { get; set; }

    // Navigation
    public MaintenanceTask? MaintenanceTask { get; set; }
    public ApplicationUser? ChangedBy { get; set; }
}
