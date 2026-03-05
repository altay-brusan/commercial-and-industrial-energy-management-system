namespace Backend.Domain.Entities;

public class TaskNote : BaseAuditableEntity
{
    public Guid MaintenanceTaskId { get; set; }
    public string Content { get; set; } = string.Empty;
    public string AuthorId { get; set; } = string.Empty;

    // Navigation
    public MaintenanceTask? MaintenanceTask { get; set; }
    public ApplicationUser? Author { get; set; }
}
