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
        "normal" => new StatusBadgeDto { Value = "normal", Label = "Normal", Color = "green" },
        "warning" => new StatusBadgeDto { Value = "warning", Label = "Warning", Color = "orange" },
        "fault" => new StatusBadgeDto { Value = "fault", Label = "Fault", Color = "red" },
        "maintenance" => new StatusBadgeDto { Value = "maintenance", Label = "Maintenance", Color = "orange" },
        "charging" => new StatusBadgeDto { Value = "charging", Label = "Charging", Color = "blue" },
        "discharging" => new StatusBadgeDto { Value = "discharging", Label = "Discharging", Color = "cyan" },
        "idle" => new StatusBadgeDto { Value = "idle", Label = "Idle", Color = "gray" },
        _ => new StatusBadgeDto { Value = status ?? "unknown", Label = status ?? "Unknown", Color = "gray" }
    };

    /// <summary>
    /// Derives operating mode badge from BMS state string (e.g., "Discharge", "Charge", "Idle").
    /// </summary>
    public static StatusBadgeDto FromOperatingMode(string? state) => state?.ToLowerInvariant() switch
    {
        "discharge" or "discharging" => new StatusBadgeDto { Value = "discharging", Label = "Discharging", Color = "purple" },
        "charge" or "charging" => new StatusBadgeDto { Value = "charging", Label = "Charging", Color = "green" },
        "idle" => new StatusBadgeDto { Value = "idle", Label = "Idle", Color = "gray" },
        "standby" => new StatusBadgeDto { Value = "standby", Label = "Standby", Color = "yellow" },
        "fault" or "error" => new StatusBadgeDto { Value = "fault", Label = "Fault", Color = "red" },
        _ => new StatusBadgeDto { Value = state?.ToLowerInvariant() ?? "unknown", Label = state ?? "Unknown", Color = "gray" }
    };
}
