namespace Backend.Domain.Entities;

/// <summary>
/// Represents a connection/link between device ports within a Facility.
/// Replaces TopologyEdge for device connections.
/// </summary>
public class DeviceLink : BaseAuditableEntity
{
    /// <summary>
    /// The Facility this link belongs to
    /// </summary>
    public Guid FacilityId { get; set; }

    /// <summary>
    /// Unique identifier for the link within the facility (e.g., "AC_BUS_001", "DC_LINK_002")
    /// </summary>
    public string LinkId { get; set; } = string.Empty;

    /// <summary>
    /// Type of link: AC, DC, RS485, CAN, Ethernet, etc.
    /// </summary>
    public string LinkType { get; set; } = string.Empty;

    /// <summary>
    /// JSON configuration specific to the link type.
    /// AC: {"max_voltage":230,"max_current":400,"ac_impedance":3,"frequency":50,"cable_length":5}
    /// DC: {"max_voltage":240,"max_current":100,"dc_resistance":3,"cable_length":10}
    /// RS485: {"baudrate":115200,"protocol":"modbus"}
    /// </summary>
    public string Configuration { get; set; } = "{}";

    /// <summary>
    /// JSON array of connected port references.
    /// Format: ["DEVICE_ID.PORT_ID", "DEVICE_ID.PORT_ID"]
    /// Example: ["GRID_001.Main", "INVERTER_001.Grid"]
    /// </summary>
    public string ConnectedPorts { get; set; } = "[]";

    // Navigation properties
    public Facility? Facility { get; set; }
}
