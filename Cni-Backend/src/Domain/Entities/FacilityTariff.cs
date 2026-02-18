namespace Backend.Domain.Entities;

/// <summary>
/// Junction table linking Facilities to Tariffs.
/// Maintains history of tariff assignments with timestamps.
/// Supports special contract arrangements with discounts.
/// Only one tariff should be active per facility at a time.
/// </summary>
public class FacilityTariff : BaseAuditableEntity
{
    /// <summary>
    /// The Facility this tariff is assigned to
    /// </summary>
    public Guid FacilityId { get; set; }

    /// <summary>
    /// The Tariff assigned to the facility
    /// </summary>
    public Guid TariffId { get; set; }

    /// <summary>
    /// Whether this tariff assignment is currently active.
    /// Only one should be active per facility at a time.
    /// </summary>
    public bool IsActive { get; set; } = true;

    #region Contract Details

    /// <summary>
    /// Contract number for special arrangements (optional).
    /// </summary>
    public string? ContractNumber { get; set; }

    /// <summary>
    /// Discount rate as decimal (e.g., 0.10 for 10% discount).
    /// Applied to the calculated gross price.
    /// </summary>
    public decimal DiscountRate { get; set; } = 0m;

    /// <summary>
    /// Contract start date (optional).
    /// </summary>
    public DateTimeOffset? ContractStartDate { get; set; }

    /// <summary>
    /// Contract end date (optional).
    /// </summary>
    public DateTimeOffset? ContractEndDate { get; set; }

    /// <summary>
    /// Additional notes about the contract or special arrangement.
    /// </summary>
    public string? Notes { get; set; }

    #endregion

    // Navigation properties
    public Facility? Facility { get; set; }
    public Tariff? Tariff { get; set; }
}
