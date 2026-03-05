namespace Backend.Application.Alarms.Queries;

/// <summary>
/// User sub-object for acknowledgedBy / resolvedBy fields.
/// </summary>
public record AlarmUserDto
{
    public string Id { get; init; } = string.Empty;
    public string? Name { get; init; }
    public string? Email { get; init; }
    public string? Initials { get; init; }
    public string? Role { get; init; }
    public string? Department { get; init; }
    public string? Phone { get; init; }
}

/// <summary>
/// Related device/asset sub-object.
/// </summary>
public record AlarmRelatedAssetDto
{
    public Guid Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string? Type { get; init; }
    public string? Location { get; init; }
    public string? Status { get; init; }
    public string? SerialNumber { get; init; }
    public string? Model { get; init; }
}

/// <summary>
/// Alarm DTO matching the frontend contract.
/// </summary>
public record AlarmDto
{
    public Guid Id { get; init; }
    public string Code { get; init; } = string.Empty;
    public string? Message { get; init; }
    public string Severity { get; init; } = string.Empty;
    public string Status { get; init; } = string.Empty;
    public string? Source { get; init; }
    public string? SourceType { get; init; }
    public DateTimeOffset TriggeredAt { get; init; }
    public string? Value { get; init; }
    public string? Threshold { get; init; }
    public DateTimeOffset? AcknowledgedAt { get; init; }
    public AlarmUserDto? AcknowledgedBy { get; init; }
    public DateTimeOffset? ResolvedAt { get; init; }
    public AlarmUserDto? ResolvedBy { get; init; }
    public AlarmRelatedAssetDto? RelatedAsset { get; init; }
    public Guid? RelatedMaintenanceTaskId { get; init; }
    public string? Location { get; init; }
}

/// <summary>
/// Wrapper for paginated alarm list matching frontend contract.
/// </summary>
public record AlarmListResponseDto
{
    public List<AlarmDto> Alarms { get; init; } = new();
    public AlarmPaginationDto Pagination { get; init; } = new();
}

public record AlarmPaginationDto
{
    public int Page { get; init; }
    public int PageSize { get; init; }
    public int TotalItems { get; init; }
    public int TotalPages { get; init; }
}

/// <summary>
/// Single alarm detail — same shape as list item (frontend uses same DTO).
/// </summary>
public record AlarmDetailDto : AlarmDto
{
    public string? Details { get; init; }
    public bool IsActive { get; init; }
    public bool IsRead { get; init; }
    public string? Name { get; init; }
    public string? Category { get; init; }
    public Guid? DeviceId { get; init; }
    public Guid? FacilityId { get; init; }
    public DateTimeOffset? RecoveredAt { get; init; }
    public DateTimeOffset Created { get; init; }
}

public record AlarmSummaryDto
{
    public int TotalActive { get; init; }
    public int Critical { get; init; }
    public int High { get; init; }
    public int Acknowledged { get; init; }
}
