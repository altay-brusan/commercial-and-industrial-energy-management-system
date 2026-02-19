namespace Backend.Application.Common.Models.Monitoring;

public record FaultRecordDto
{
    public Guid Id { get; init; }
    public string Code { get; init; } = string.Empty;
    public string Name { get; init; } = string.Empty;
    public string Severity { get; init; } = string.Empty;
    public string Category { get; init; } = string.Empty;
    public string? Source { get; init; }
    public DateTimeOffset OccurredAt { get; init; }
    public DateTimeOffset? RecoveredAt { get; init; }
    public bool IsActive { get; init; }
    public string? Details { get; init; }
}

public record FaultsPaginatedResponseDto
{
    public List<FaultRecordDto> Items { get; init; } = new();
    public int PageNumber { get; init; }
    public int TotalPages { get; init; }
    public int TotalCount { get; init; }
}
