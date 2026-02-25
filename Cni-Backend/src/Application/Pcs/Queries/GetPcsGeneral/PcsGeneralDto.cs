using Backend.Application.Common.Models.Monitoring;

namespace Backend.Application.Pcs.Queries.GetPcsGeneral;

public record PcsGeneralDto
{
    public Guid Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string? SerialNumber { get; init; }
    public string? ModelName { get; init; }
    public string? Manufacturer { get; init; }
    public Guid FacilityId { get; init; }
    public string? FacilityName { get; init; }
    public StatusBadgeDto Status { get; init; } = new();
    public PcsGeneralLinkedBessDto? LinkedBessUnit { get; init; }
    public List<TabInfoDto> AvailableTabs { get; init; } = new();
    public int ActiveFaultCount { get; init; }
    public DateTimeOffset? LastSeenUtc { get; init; }
    public List<DynamicSectionDto> Sections { get; init; } = new();
}

public record PcsGeneralLinkedBessDto
{
    public Guid Id { get; init; }
    public string Name { get; init; } = string.Empty;
}
