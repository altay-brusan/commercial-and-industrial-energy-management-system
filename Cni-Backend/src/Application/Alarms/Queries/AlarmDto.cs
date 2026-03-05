namespace Backend.Application.Alarms.Queries;

public record AlarmDto
{
    public Guid Id { get; init; }
    public Guid DeviceId { get; init; }
    public Guid DeviceAlarmTypeId { get; init; }
    public string Code { get; init; } = string.Empty;
    public string Name { get; init; } = string.Empty;
    public string? Message { get; init; }
    public string Severity { get; init; } = string.Empty;
    public string Status { get; init; } = string.Empty;
    public string? Source { get; init; }
    public string? Category { get; init; }
    public string? DeviceName { get; init; }
    public string? DeviceModel { get; init; }
    public string? FacilityName { get; init; }
    public Guid? FacilityId { get; init; }
    public DateTimeOffset OccurredAt { get; init; }
    public DateTimeOffset? RecoveredAt { get; init; }
    public DateTimeOffset? AcknowledgedAt { get; init; }
    public DateTimeOffset? ResolvedAt { get; init; }
    public string? AcknowledgedByName { get; init; }
    public string? ResolvedByName { get; init; }
    public Guid? MaintenanceTaskId { get; init; }
    public bool HasMaintenanceTask { get; init; }
}

public record AlarmDetailDto : AlarmDto
{
    public string? Details { get; init; }
    public bool IsActive { get; init; }
    public bool IsRead { get; init; }
    public string? AcknowledgedById { get; init; }
    public string? ResolvedById { get; init; }
    public DateTimeOffset Created { get; init; }
}

public record AlarmSummaryDto
{
    public int TotalActive { get; init; }
    public int Critical { get; init; }
    public int High { get; init; }
    public int Acknowledged { get; init; }
}
