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
    public string? SerialNumber { get; init; }
    public string? ModelName { get; init; }
    public string? Manufacturer { get; init; }
    public string? FacilityName { get; init; }
    public Guid FacilityId { get; init; }
    public StatusBadgeDto Status { get; init; } = new();
    public double? SocPercent { get; init; }
    public double? SohPercent { get; init; }
    public double? PowerKw { get; init; }
    public double? TemperatureC { get; init; }
    public string? ChargeStatus { get; init; }
    public DateTimeOffset? LastSeenUtc { get; init; }
}

public record BessAggregatesDto
{
    public int TotalUnits { get; init; }
    public int OnlineCount { get; init; }
    public int OfflineCount { get; init; }
    public int FaultCount { get; init; }
    public double? AvgSocPercent { get; init; }
    public double? AvgSohPercent { get; init; }
    public double? TotalPowerKw { get; init; }
    public double? TotalEnergyStoredKwh { get; init; }
}
