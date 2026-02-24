namespace Backend.Domain.Entities;

/// <summary>
/// Catalog entry defining a specific fault/alarm type for a device model.
/// Global entity — no tenant query filter (like Tariff, MarketOperator).
/// </summary>
public class DeviceAlarmType : BaseAuditableEntity
{
    public Guid DeviceTypeId { get; set; }
    public string Model { get; set; } = string.Empty;
    public string? Manufacturer { get; set; }
    public string Code { get; set; } = string.Empty;

    /// <summary>
    /// JSON property path used to detect this alarm in the payload.
    /// For BooleanFlag: the boolean property name (e.g., "faultCellOverVoltageWarning").
    /// For MessageMatch: the JSON path to the message field.
    /// For ErrorCode: the JSON path to the error code field.
    /// </summary>
    public string FaultKey { get; set; } = string.Empty;

    /// <summary>
    /// Which telemetry message kind carries this alarm (e.g., "ContainerBatteryFaultState").
    /// </summary>
    public string Kind { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string Category { get; set; } = string.Empty;
    public string Severity { get; set; } = "Warning";
    public string? Source { get; set; }

    /// <summary>
    /// How to interpret the payload value at FaultKey:
    /// "BooleanFlag" — true = fault active, false = recovered (default, BESS containers)
    /// "MessageMatch" — presence of a string value = fault active
    /// "ErrorCode" — non-zero numeric value = fault active
    /// </summary>
    public string DetectionStrategy { get; set; } = "BooleanFlag";

    /// <summary>
    /// Optional pattern for MessageMatch strategy (regex or exact string).
    /// Ignored for BooleanFlag and ErrorCode strategies.
    /// </summary>
    public string? MatchPattern { get; set; }

    public bool IsEnabled { get; set; } = true;

    // Navigation
    public DeviceType? DeviceType { get; set; }
}
