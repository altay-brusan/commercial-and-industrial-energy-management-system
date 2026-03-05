namespace Backend.Domain.Entities;

/// <summary>
/// A catalog entry representing an abstract device type composed of logical nodes.
/// Maps to IEC 61850 logical device concept. Global entity — not tenant-scoped.
/// </summary>
public class LogicalDevice : BaseAuditableEntity
{
    /// <summary>
    /// Unique machine name (e.g., "BatteryContainer", "PcsUnit", "SolarInverter")
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Human-readable label for UI display
    /// </summary>
    public string? DisplayName { get; set; }

    /// <summary>
    /// Optional description of the logical device
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// High-level category: BESS, Solar, Wind, Grid, PCS
    /// </summary>
    public string Category { get; set; } = string.Empty;

    /// <summary>
    /// JSON array of port definitions from the engineering tool catalog.
    /// </summary>
    public string Ports { get; set; } = "[]";

    /// <summary>
    /// JSON describing child device structure (references LogicalDevice names).
    /// Example: { "children": [{ "logicalDeviceName": "BatteryRack", "count": 4, "namePattern": "Rack {index}" }] }
    /// </summary>
    public string? HierarchyTemplate { get; set; }

    /// <summary>
    /// Optional JSON for UI layout overrides per device type.
    /// </summary>
    public string? UiTemplate { get; set; }

    // Navigation
    public ICollection<LogicalDeviceNode> LogicalDeviceNodes { get; set; } = new List<LogicalDeviceNode>();
    public ICollection<Device> Devices { get; set; } = new List<Device>();
}
