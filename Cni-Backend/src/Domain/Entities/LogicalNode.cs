namespace Backend.Domain.Entities;

/// <summary>
/// A catalog entry representing a group of related parameters (IEC 61850-inspired logical node).
/// Global entity — not tenant-scoped.
/// </summary>
public class LogicalNode : BaseAuditableEntity
{
    /// <summary>
    /// Unique machine name (e.g., "BatteryStatusData", "PcsAcMeasurementData")
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Human-readable label for UI display
    /// </summary>
    public string? DisplayName { get; set; }

    /// <summary>
    /// Optional description of the node's purpose
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// JSON array of parameter definitions.
    /// Each entry: { key, label, unit, dataType, sourceType }
    /// sourceType: Registration | Installation | Realtime | Calculated
    /// </summary>
    public string Parameters { get; set; } = "[]";

    // Navigation
    public ICollection<LogicalDeviceNode> LogicalDeviceNodes { get; set; } = new List<LogicalDeviceNode>();
}
