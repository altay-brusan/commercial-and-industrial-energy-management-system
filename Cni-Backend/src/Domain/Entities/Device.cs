namespace Backend.Domain.Entities;

/// <summary>
/// Represents a physical device instance within a Facility.
/// Contains port configurations for connections and links to other devices.
/// Tenant relationship is through Facility → Firm → Tenant.
/// </summary>
public class Device : BaseAuditableEntity
{
    /// <summary>
    /// The Facility this device belongs to
    /// </summary>
    public int FacilityId { get; set; }

    /// <summary>
    /// The type of device (e.g., Inverter, Battery, Solar Panel)
    /// </summary>
    public int DeviceTypeId { get; set; }

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

    // Navigation properties
    public Facility? Facility { get; set; }
    public DeviceType? DeviceType { get; set; }
}
