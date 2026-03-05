namespace Backend.Domain.Entities;

public class MaintenanceTask : BaseAuditableEntity
{
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string Status { get; set; } = string.Empty;
    public string Priority { get; set; } = string.Empty;

    public string AssignedToId { get; set; } = string.Empty;
    public string AssignedById { get; set; } = string.Empty;

    public DateTimeOffset DueDate { get; set; }

    // Facility (required — provides tenant chain)
    public Guid FacilityId { get; set; }

    // Related device (optional)
    public Guid? DeviceId { get; set; }

    // Related alarm (optional)
    public Guid? DeviceAlarmId { get; set; }

    // Completion details
    public string? Resolution { get; set; }
    public string? RootCause { get; set; }
    public decimal? LaborHours { get; set; }
    public string? PartsUsed { get; set; }
    public string? Recommendations { get; set; }
    public DateTimeOffset? CompletedAt { get; set; }
    public string? CompletedById { get; set; }

    // Close details
    public string? CloseReason { get; set; }
    public string? CloseExplanation { get; set; }
    public DateTimeOffset? ClosedAt { get; set; }
    public string? ClosedById { get; set; }

    // Navigation
    public ApplicationUser? AssignedTo { get; set; }
    public ApplicationUser? AssignedBy { get; set; }
    public ApplicationUser? CompletedBy { get; set; }
    public ApplicationUser? ClosedBy { get; set; }
    public Facility? Facility { get; set; }
    public Device? Device { get; set; }
    public DeviceAlarm? DeviceAlarm { get; set; }
    public ICollection<TaskNote> Notes { get; set; } = new List<TaskNote>();
    public ICollection<TaskChecklistItem> ChecklistItems { get; set; } = new List<TaskChecklistItem>();
    public ICollection<TaskStatusChange> StatusChanges { get; set; } = new List<TaskStatusChange>();
    public ICollection<TaskWorkSession> WorkSessions { get; set; } = new List<TaskWorkSession>();
}
