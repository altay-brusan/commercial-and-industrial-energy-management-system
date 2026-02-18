using Backend.Domain.Enums;

namespace Backend.Domain.Entities;

/// <summary>
/// Represents a Facility - a specific installation (solar cluster, battery container) within a Firm.
/// Contains devices and their topology. Tenant relationship is through Firm.
/// </summary>
public class Facility : BaseAuditableEntity
{
    /// <summary>
    /// The Firm (Site/Factory) this facility belongs to
    /// </summary>
    public Guid FirmId { get; set; }

    /// <summary>
    /// Display name of the facility (e.g., "Solar Cluster A", "Battery Container 1")
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Physical address of the facility
    /// </summary>
    public string Address { get; set; } = string.Empty;

    /// <summary>
    /// Energy capacity value (unit specified by EnergyUnit)
    /// </summary>
    public int EnergyCapacity { get; set; }

    /// <summary>
    /// Unit of measurement for EnergyCapacity (default: KWh)
    /// </summary>
    public EnergyUnit EnergyUnit { get; set; } = EnergyUnit.KWh;

    /// <summary>
    /// Geographic longitude coordinate
    /// </summary>
    public double Longitude { get; set; }

    /// <summary>
    /// Geographic latitude coordinate
    /// </summary>
    public double Latitude { get; set; }

    /// <summary>
    /// Full topology JSON containing devices and links architecture.
    /// Used for storing/exporting the complete facility topology.
    /// </summary>
    public string? Topology { get; set; }

    // Navigation properties
    public Firm? Firm { get; set; }
    public ICollection<Device> Devices { get; set; } = new List<Device>();
    public ICollection<DeviceLink> DeviceLinks { get; set; } = new List<DeviceLink>();
    public ICollection<FacilityTariff> FacilityTariffs { get; set; } = new List<FacilityTariff>();
    public ICollection<UserFirmFacility> UserCompanyFacilities { get; set; } = new List<UserFirmFacility>();
}
