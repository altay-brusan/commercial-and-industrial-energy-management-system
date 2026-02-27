using Backend.Application.Common.Models.Monitoring;

namespace Backend.Application.BatteryStorage.Queries.GetBessUnitHeader;

public record BessUnitHeaderDto
{
    public Guid Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string? SerialNumber { get; init; }
    public string? ModelName { get; init; }
    public string? Manufacturer { get; init; }
    public Guid FacilityId { get; init; }
    public string? FacilityName { get; init; }

    // Ancestor references (populated for sub-unit levels: rack, pack, module, etc.)
    public Guid? UnitId { get; init; }
    public string? UnitName { get; init; }
    public int? HierarchyNumber { get; init; }

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
}

public record ConnectedDeviceDto
{
    public Guid Id { get; init; }
    public string Name { get; init; } = string.Empty;
}
