namespace Backend.Domain.Entities.DeviceData;

/// <summary>
/// Base class for all device telemetry data.
/// Uses BIGINT Id for high-volume time-series data.
/// </summary>
public abstract class DeviceDataBase
{
    /// <summary>
    /// Primary key - BIGINT to handle billions of rows
    /// </summary>
    public long Id { get; set; }

    /// <summary>
    /// Reference to the device that generated this data
    /// </summary>
    public Guid DeviceId { get; set; }

    /// <summary>
    /// Timestamp from the device itself (device's local time)
    /// </summary>
    public DateTimeOffset DeviceLocalTime { get; set; }

    /// <summary>
    /// Message type discriminator: "ContainerBatteryState", "PcsAcMeasState", etc.
    /// </summary>
    public string? Kind { get; set; }

    /// <summary>
    /// All telemetry data as JSON payload
    /// </summary>
    public string Payload { get; set; } = "{}";

    // Audit fields
    public DateTimeOffset Created { get; set; }
    public string? CreatedBy { get; set; }
    public DateTimeOffset LastModified { get; set; }
    public string? LastModifiedBy { get; set; }

    // Navigation
    public Device? Device { get; set; }
}
