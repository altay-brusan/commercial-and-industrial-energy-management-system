namespace Backend.Domain.Entities;

/// <summary>
/// Join table linking a LogicalDevice to its LogicalNodes with instance metadata.
/// Composite PK: (LogicalDeviceId, LogicalNodeId).
/// </summary>
public class LogicalDeviceNode
{
    public Guid LogicalDeviceId { get; set; }
    public Guid LogicalNodeId { get; set; }

    /// <summary>
    /// Custom alias for the node within this device (e.g., "mppt" instead of default name).
    /// Null means use the LogicalNode.Name.
    /// </summary>
    public string? NodeAlias { get; set; }

    /// <summary>
    /// Number of instances of this node (e.g., 2 MPPTs, 16 battery modules).
    /// </summary>
    public int InstanceCount { get; set; } = 1;

    /// <summary>
    /// Display ordering in UI.
    /// </summary>
    public int SortOrder { get; set; }

    // Navigation
    public LogicalDevice? LogicalDevice { get; set; }
    public LogicalNode? LogicalNode { get; set; }
}
