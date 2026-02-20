namespace Backend.Domain.Entities;

/// <summary>
/// Represents a physical device instance connected to an RTU.
/// Contains port configurations for connections and links to other devices.
/// Tenant relationship is through Rtu → Facility → Firm → Tenant.
/// </summary>
public class Device : BaseAuditableEntity
{
    /// <summary>
    /// The RTU (Remote Terminal Unit) this device is connected to
    /// </summary>
    public Guid RtuId { get; set; }

    /// <summary>
    /// The type of device (e.g., Inverter, Battery, Solar Panel)
    /// </summary>
    public Guid DeviceTypeId { get; set; }

    /// <summary>
    /// Self-reference for composite devices (e.g., cabinet → child subsystems)
    /// </summary>
    public Guid? ParentDeviceId { get; set; }

    /// <summary>
    /// Display name of the device (e.g., "INVERTER_001", "BESS_002")
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Device model name (e.g., "Bosch8080", "SMA Sunny Tripower")
    /// </summary>
    public string Model { get; set; } = string.Empty;

    /// <summary>
    /// Serial number for the device (may be null for legacy devices)
    /// </summary>
    public string? SerialNumber { get; set; }

    /// <summary>
    /// Manufacturer name (e.g., "Bosch", "SMA", "Tesla")
    /// </summary>
    public string? Manufacturer { get; set; }

    /// <summary>
    /// JSON array of port configurations.
    /// Each port has: id, type (AC/DC), and type-specific config.
    /// Example: [{"id":"Grid","type":"AC","config":{"voltage":230,"frequency":50}}]
    /// </summary>
    public string Ports { get; set; } = "[]";

    /// <summary>
    /// Original ID from topology JSON (for re-export/roundtrip)
    /// </summary>
    public string? TopologyId { get; set; }

    /// <summary>
    /// Instance-specific config as JSON (capacity, chemistry, commissioning date, etc.)
    /// </summary>
    public string? Metadata { get; set; }

    /// <summary>
    /// Device specifications as JSON (tabs, curves, fault flags, hierarchy definitions, etc.)
    /// </summary>
    public string? Specifications { get; set; }

    /// <summary>
    /// Current device status: online/offline/standby/fault/maintenance
    /// </summary>
    public string? Status { get; set; }

    /// <summary>
    /// Last telemetry timestamp from the device
    /// </summary>
    public DateTimeOffset? LastSeenUtc { get; set; }

    // Navigation properties
    public Rtu? Rtu { get; set; }
    public DeviceType? DeviceType { get; set; }
    public Device? ParentDevice { get; set; }
    public ICollection<Device> ChildDevices { get; set; } = new List<Device>();
    public ICollection<DeviceAlarm> Alarms { get; set; } = new List<DeviceAlarm>();
}
