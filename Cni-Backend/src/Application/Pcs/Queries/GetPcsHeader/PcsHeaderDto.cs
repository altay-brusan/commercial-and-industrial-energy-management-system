using Backend.Application.Common.Models.Monitoring;

namespace Backend.Application.Pcs.Queries.GetPcsHeader;

public record PcsHeaderDto
{
    public Guid Id { get; init; }
    public string DeviceId { get; init; } = string.Empty;
    public string Name { get; init; } = string.Empty;
    public StatusBadgeDto Status { get; init; } = new();
    public PcsHeaderLinkedBessDto? LinkedBessUnit { get; init; }
    public List<TabInfoDto> AvailableTabs { get; init; } = new();
}

public record PcsHeaderLinkedBessDto
{
    public Guid Id { get; init; }
    public string Name { get; init; } = string.Empty;
}
