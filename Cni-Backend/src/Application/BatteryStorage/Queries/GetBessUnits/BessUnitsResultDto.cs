using Backend.Application.Common.Models.Monitoring;

namespace Backend.Application.BatteryStorage.Queries.GetBessUnits;

public record BessUnitsResultDto
{
    public List<BessUnitListItemDto> Units { get; init; } = new();
    public BessAggregatesDto Aggregates { get; init; } = new();
}

public record BessUnitListItemDto
{
    public Guid Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public StatusBadgeDto Status { get; init; } = new();
    public StatusBadgeDto OperatingMode { get; init; } = new();
    public double? Soc { get; init; }
    public double? Soh { get; init; }
    public double? Power { get; init; }
    public double? CapacityKwh { get; init; }
    public int ActiveAlarmCount { get; init; }
    public string? Brand { get; init; }
    public string? Model { get; init; }
    public string? SerialNumber { get; init; }
    public string? FacilityName { get; init; }
    public Guid FacilityId { get; init; }
    public double? TemperatureC { get; init; }
    public DateTimeOffset? LastSeenUtc { get; init; }
}

public record BessAggregatesDto
{
    public int TotalUnits { get; init; }
    public double? TotalCapacityKwh { get; init; }
    public double? AverageSoc { get; init; }
    public int TotalActiveAlarms { get; init; }
    public int OnlineCount { get; init; }
    public int OfflineCount { get; init; }
    public int FaultCount { get; init; }
    public int StandbyCount { get; init; }
}
