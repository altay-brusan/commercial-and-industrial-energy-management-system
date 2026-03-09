using Backend.Application.Common.Models.Monitoring;

namespace Backend.Application.BatteryStorage.Queries.GetBessUnitHeader;

public record BessUnitHeaderDto
{
    public Guid Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string? SerialNumber { get; init; }
    public string? Model { get; init; }
    public string? Brand { get; init; }
    public string? ProductType { get; init; }
    public Guid FacilityId { get; init; }
    public string? FacilityName { get; init; }

    // Ancestor references (populated for sub-unit levels: rack, pack, module, etc.)
    public Guid? UnitId { get; init; }
    public string? UnitName { get; init; }
    public Guid? RackId { get; init; }
    public string? RackName { get; init; }
    public Guid? PackId { get; init; }
    public string? PackName { get; init; }
    public int? HierarchyNumber { get; init; }

    /// <summary>
    /// Device display type: "unit", "rack", "pack", "module", "hvac", "esc", "pcs", etc.
    /// Used by the frontend to determine rendering mode.
    /// </summary>
    public string? DisplayType { get; init; }

    public StatusBadgeDto Status { get; init; } = new();
    public StatusBadgeDto? OperatingMode { get; init; }
    public double? Soc { get; init; }
    public double? Soh { get; init; }
    public double? Voltage { get; init; }
    public double? Current { get; init; }
    public double? Power { get; init; }
    public double? CapacityKwh { get; init; }

    public int ActiveAlarmCount { get; init; }
    public List<TabInfoDto> AvailableTabs { get; init; } = new();
    public Dictionary<string, List<ConnectedDeviceDto>> ConnectedDevices { get; init; } = new();
    public DateTimeOffset? LastSeenUtc { get; init; }

    /// <summary>
    /// Sections-based content for HVAC/ESC devices.
    /// When populated, the frontend renders dynamic sections instead of flat fields.
    /// </summary>
    public List<DynamicSectionDto>? Sections { get; init; }
}

public record ConnectedDeviceDto
{
    public Guid Id { get; init; }
    public string Name { get; init; } = string.Empty;
}
