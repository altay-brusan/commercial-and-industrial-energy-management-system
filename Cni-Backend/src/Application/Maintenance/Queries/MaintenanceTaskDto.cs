namespace Backend.Application.Maintenance.Queries;

public record MaintenanceTaskBriefDto
{
    public Guid Id { get; init; }
    public string Title { get; init; } = string.Empty;
    public string Status { get; init; } = string.Empty;
    public string Priority { get; init; } = string.Empty;
    public string? AssignedToId { get; init; }
    public string? AssignedToName { get; init; }
    public string? AssignedById { get; init; }
    public string? AssignedByName { get; init; }
    public DateTimeOffset DueDate { get; init; }
    public DateTimeOffset Created { get; init; }
    public string? DeviceName { get; init; }
    public string? FacilityName { get; init; }
    public Guid? DeviceId { get; init; }
    public Guid? DeviceAlarmId { get; init; }
    public Guid FacilityId { get; init; }
    public bool HasRelatedAlarm { get; init; }
}

public record MaintenanceTaskDetailDto : MaintenanceTaskBriefDto
{
    public string? Description { get; init; }
    public string? Resolution { get; init; }
    public string? RootCause { get; init; }
    public string? Recommendations { get; init; }
    public string? PartsUsed { get; init; }
    public decimal? LaborHours { get; init; }
    public string? CloseReason { get; init; }
    public string? CloseExplanation { get; init; }
    public DateTimeOffset? CompletedAt { get; init; }
    public DateTimeOffset? ClosedAt { get; init; }
    public string? CompletedByName { get; init; }
    public string? ClosedByName { get; init; }
    public RelatedAlarmDto? RelatedAlarm { get; init; }
    public List<TaskNoteDto> Notes { get; init; } = new();
    public List<TaskChecklistItemDto> Checklist { get; init; } = new();
    public List<TaskStatusChangeDto> Timeline { get; init; } = new();
    public List<TaskWorkSessionDto> WorkSessions { get; init; } = new();
}

public record RelatedAlarmDto
{
    public Guid Id { get; init; }
    public string Code { get; init; } = string.Empty;
    public string Name { get; init; } = string.Empty;
    public string Severity { get; init; } = string.Empty;
    public bool IsActive { get; init; }
    public DateTimeOffset OccurredAt { get; init; }
}

public record TaskNoteDto
{
    public Guid Id { get; init; }
    public string Content { get; init; } = string.Empty;
    public string? AuthorName { get; init; }
    public DateTimeOffset Created { get; init; }
}

public record TaskChecklistItemDto
{
    public Guid Id { get; init; }
    public string Label { get; init; } = string.Empty;
    public int SortOrder { get; init; }
    public bool IsCompleted { get; init; }
    public DateTimeOffset? CompletedAt { get; init; }
    public string? CompletedByName { get; init; }
}

public record TaskStatusChangeDto
{
    public Guid Id { get; init; }
    public string? FromStatus { get; init; }
    public string ToStatus { get; init; } = string.Empty;
    public string? ChangedByName { get; init; }
    public DateTimeOffset ChangedAt { get; init; }
}

public record TaskWorkSessionDto
{
    public Guid Id { get; init; }
    public string? UserName { get; init; }
    public DateTimeOffset StartedAt { get; init; }
    public DateTimeOffset? EndedAt { get; init; }
}
