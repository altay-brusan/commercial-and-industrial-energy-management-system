using Backend.Application.Common.Models.Monitoring;

namespace Backend.Application.Pcs.Queries.GetPcsUnits;

public record PcsUnitsResultDto
{
    public List<PcsUnitListItemDto> Units { get; init; } = new();
    public PcsAggregatesDto Aggregates { get; init; } = new();
}

public record PcsUnitListItemDto
{
    public Guid Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string? SerialNumber { get; init; }
    public string? ModelName { get; init; }
    public string? Manufacturer { get; init; }
    public string? FacilityName { get; init; }
    public Guid FacilityId { get; init; }
    public StatusBadgeDto Status { get; init; } = new();
    public string? GridMode { get; init; }
    public double? ActivePowerKw { get; init; }
    public double? GridFreqHz { get; init; }
    public double? IgbtTempC { get; init; }
    public double? EfficiencyPct { get; init; }
    public double? PowerFactor { get; init; }
    public Guid? LinkedBessUnitId { get; init; }
    public string? LinkedBessUnitName { get; init; }
    public DateTimeOffset? LastSeenUtc { get; init; }
}

public record PcsAggregatesDto
{
    public int TotalUnits { get; init; }
    public int OnlineCount { get; init; }
    public int OfflineCount { get; init; }
    public int FaultCount { get; init; }
    public double? TotalActivePowerKw { get; init; }
    public double? AvgEfficiencyPct { get; init; }
    public double? AvgPowerFactor { get; init; }
}
