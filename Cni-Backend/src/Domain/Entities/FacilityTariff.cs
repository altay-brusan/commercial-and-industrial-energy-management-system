namespace Backend.Domain.Entities;

/// <summary>
/// Junction table linking Facilities to Tariffs.
/// Maintains history of tariff assignments with timestamps.
/// Only one tariff should be active per facility at a time.
/// </summary>
public class FacilityTariff : BaseAuditableEntity
{
    /// <summary>
    /// The Facility this tariff is assigned to
    /// </summary>
    public int FacilityId { get; set; }

    /// <summary>
    /// The Tariff assigned to the facility
    /// </summary>
    public int TariffId { get; set; }

    /// <summary>
    /// Whether this tariff assignment is currently active.
    /// Only one should be active per facility at a time.
    /// </summary>
    public bool IsActive { get; set; } = true;

    // Navigation properties
    public Facility? Facility { get; set; }
    public Tariff? Tariff { get; set; }
}
