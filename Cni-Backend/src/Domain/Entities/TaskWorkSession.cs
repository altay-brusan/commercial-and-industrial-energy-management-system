namespace Backend.Domain.Entities;

public class TaskWorkSession : BaseAuditableEntity
{
    public Guid MaintenanceTaskId { get; set; }
    public string UserId { get; set; } = string.Empty;
    public DateTimeOffset StartedAt { get; set; }
    public DateTimeOffset? EndedAt { get; set; }

    // Navigation
    public MaintenanceTask? MaintenanceTask { get; set; }
    public ApplicationUser? User { get; set; }
}
