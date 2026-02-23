namespace Backend.Application.Common.Models.Monitoring;

public record FaultRecordDto
{
    public Guid Id { get; init; }
    public string Code { get; init; } = string.Empty;
    public string Name { get; init; } = string.Empty;
    public StatusBadgeDto Severity { get; init; } = new();
    public StatusBadgeDto Status { get; init; } = new();
    public string? Source { get; init; }
    public DateTimeOffset OccurredAt { get; init; }
    public DateTimeOffset? RecoveredAt { get; init; }
    public Guid? RelatedAlarmId { get; init; }

    public static StatusBadgeDto SeverityBadge(string? severity) => severity?.ToLowerInvariant() switch
    {
        "critical" => new StatusBadgeDto { Value = "critical", Label = "Critical", Color = "red" },
        "major" => new StatusBadgeDto { Value = "major", Label = "Major", Color = "orange" },
        "minor" => new StatusBadgeDto { Value = "minor", Label = "Minor", Color = "yellow" },
        "warning" => new StatusBadgeDto { Value = "warning", Label = "Warning", Color = "yellow" },
        "info" => new StatusBadgeDto { Value = "info", Label = "Info", Color = "blue" },
        _ => new StatusBadgeDto { Value = severity ?? "unknown", Label = severity ?? "Unknown", Color = "gray" }
    };

    public static StatusBadgeDto FaultStatusBadge(bool isActive) => isActive
        ? new StatusBadgeDto { Value = "active", Label = "Active", Color = "red" }
        : new StatusBadgeDto { Value = "recovered", Label = "Recovered", Color = "grey" };
}

public record FaultsPaginatedResponseDto
{
    public List<FaultRecordDto> Faults { get; init; } = new();
    public PaginationDto Pagination { get; init; } = new();
}

public record PaginationDto
{
    public int Page { get; init; }
    public int PageSize { get; init; }
    public int TotalItems { get; init; }
    public int TotalPages { get; init; }
}
