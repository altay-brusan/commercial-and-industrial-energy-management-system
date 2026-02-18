namespace Backend.Application.Common.Models.Monitoring;

public record StatusBadgeDto
{
    public string Value { get; init; } = string.Empty;
    public string Label { get; init; } = string.Empty;
    public string Color { get; init; } = "gray";

    public static StatusBadgeDto FromStatus(string? status) => status?.ToLowerInvariant() switch
    {
        "online" => new StatusBadgeDto { Value = "online", Label = "Online", Color = "green" },
        "offline" => new StatusBadgeDto { Value = "offline", Label = "Offline", Color = "red" },
        "standby" => new StatusBadgeDto { Value = "standby", Label = "Standby", Color = "yellow" },
        "fault" => new StatusBadgeDto { Value = "fault", Label = "Fault", Color = "red" },
        "maintenance" => new StatusBadgeDto { Value = "maintenance", Label = "Maintenance", Color = "orange" },
        "charging" => new StatusBadgeDto { Value = "charging", Label = "Charging", Color = "blue" },
        "discharging" => new StatusBadgeDto { Value = "discharging", Label = "Discharging", Color = "cyan" },
        "idle" => new StatusBadgeDto { Value = "idle", Label = "Idle", Color = "gray" },
        _ => new StatusBadgeDto { Value = status ?? "unknown", Label = status ?? "Unknown", Color = "gray" }
    };
}
