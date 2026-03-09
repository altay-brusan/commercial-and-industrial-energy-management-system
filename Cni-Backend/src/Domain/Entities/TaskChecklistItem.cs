namespace Backend.Domain.Entities;

public class TaskChecklistItem : BaseAuditableEntity
{
    public Guid MaintenanceTaskId { get; set; }
    public string Label { get; set; } = string.Empty;
    public int SortOrder { get; set; }
    public bool IsCompleted { get; set; }
    public DateTimeOffset? CompletedAt { get; set; }
    public string? CompletedById { get; set; }

    // Navigation
    public MaintenanceTask? MaintenanceTask { get; set; }
    public ApplicationUser? CompletedBy { get; set; }
}
