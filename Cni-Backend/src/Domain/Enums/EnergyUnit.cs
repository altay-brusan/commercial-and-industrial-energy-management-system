namespace Backend.Domain.Enums;

/// <summary>
/// Represents units of energy measurement used across the system.
/// KWh is the default (0) to align with database defaults and most common usage.
/// </summary>
public enum EnergyUnit
{
    /// <summary>
    /// Kilowatt-hours (1,000 Wh) - Default unit
    /// </summary>
    KWh = 0,

    /// <summary>
    /// Watt-hours
    /// </summary>
    Wh = 1,

    /// <summary>
    /// Megawatt-hours (1,000,000 Wh)
    /// </summary>
    MWh = 2
}
