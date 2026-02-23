namespace Backend.Domain.Entities;

/// <summary>
/// Represents a type/category of device.
/// Examples: Solar, Generator, Wind Turbine, Battery (BESS), Inverter, PCS,
/// Smart Meter, Grid, DC Bus, Transformer, Load
/// </summary>
public class DeviceType : BaseAuditableEntity
{
    /// <summary>
    /// Name of the device type (e.g., "Inverter", "Battery", "Solar")
    /// </summary>
    public required string Name { get; set; }

    public string? Description { get; set; }
}
