namespace Backend.Application.Notifications.Queries;

public record NotificationDto
{
    public Guid Id { get; init; }
    public string Type { get; init; } = string.Empty;
    public string Title { get; init; } = string.Empty;
    public string Message { get; init; } = string.Empty;
    public DateTimeOffset Timestamp { get; init; }
    public bool Read { get; init; }
    public Guid? RelatedAlarmId { get; init; }
    public Guid? RelatedTaskId { get; init; }
}

public record NotificationListResponseDto
{
    public List<NotificationDto> Notifications { get; init; } = new();
    public int UnreadCount { get; init; }
}

public record MarkAllReadResponseDto
{
    public string Message { get; init; } = string.Empty;
    public int UpdatedCount { get; init; }
}
