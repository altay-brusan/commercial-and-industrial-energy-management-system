using Backend.Application.Common.Models.Monitoring;

namespace Backend.Application.BatteryStorage.Queries.GetBessUnitDetail;

public record BessUnitDetailDto
{
    public Guid Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string? SerialNumber { get; init; }
    public string? ModelName { get; init; }
    public string? Manufacturer { get; init; }
    public Guid FacilityId { get; init; }
    public string? FacilityName { get; init; }
    public StatusBadgeDto Status { get; init; } = new();
    public List<TabInfoDto> AvailableTabs { get; init; } = new();
    public DateTimeOffset? LastSeenUtc { get; init; }
}
