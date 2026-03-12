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
    /// Country name (e.g., "Türkiye", "Germany", "United States")
    /// </summary>
    public string Country { get; set; } = string.Empty;

    /// <summary>
    /// Province or state. For Turkey: plate code (e.g., "06" for Ankara).
    /// For other countries: free-text province/state name.
    /// </summary>
    public string Province { get; set; } = string.Empty;

    /// <summary>
    /// City, town, or district (ilçe/şehir)
    /// </summary>
    public string City { get; set; } = string.Empty;

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
    /// Indicates how this facility's telemetry data is sourced.
    /// Rtu = physical RTU ingestion, Huawei = FusionSolar API polling.
    /// </summary>
    public FacilityDataSource DataSource { get; set; } = FacilityDataSource.Rtu;

    /// <summary>
    /// Full topology JSON containing devices and links architecture.
    /// Used for storing/exporting the complete facility topology.
    /// </summary>
    public string? Topology { get; set; }

    /// <summary>
    /// Whether ALM (Algorithm Module) prediction jobs are enabled for this facility.
    /// </summary>
    public bool AlmEnabled { get; set; }

    // Navigation properties
    public Firm? Firm { get; set; }
    public ICollection<DeviceLink> DeviceLinks { get; set; } = new List<DeviceLink>();
    public ICollection<FacilityTariff> FacilityTariffs { get; set; } = new List<FacilityTariff>();
    public ICollection<UserFirmFacility> UserCompanyFacilities { get; set; } = new List<UserFirmFacility>();
    public ICollection<Rtu> Rtus { get; set; } = new List<Rtu>();
}
