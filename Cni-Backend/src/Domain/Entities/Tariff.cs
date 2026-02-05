namespace Backend.Domain.Entities;

/// <summary>
/// Represents tariff configuration for energy pricing.
/// Includes distribution fees, transmission fees, and tax rates.
/// This is an append-only table - records should not be deleted, only soft-deleted via IsActive.
/// </summary>
public class Tariff : BaseAuditableEntity
{
    /// <summary>
    /// Full country name (e.g., "Netherlands", "Germany", "Türkiye")
    /// </summary>
    public string CountryName { get; set; } = string.Empty;

    /// <summary>
    /// Name of the distribution company (e.g., "Liander", "Enexis")
    /// </summary>
    public string? DistributorName { get; set; }

    /// <summary>
    /// Distribution fee per MWh (local grid distribution costs)
    /// Default: 20 EUR/MWh (EU average)
    /// </summary>
    public decimal DistributionFee { get; set; } = 20m;

    /// <summary>
    /// Transmission fee per MWh (high-voltage transmission costs)
    /// Default: 5 EUR/MWh (EU average)
    /// </summary>
    public decimal TransmissionFee { get; set; } = 5m;

    /// <summary>
    /// Tax rate applied to gross price (as decimal, e.g., 0.21 for 21%)
    /// Includes VAT and electricity-specific taxes
    /// Default: 0.21 (21% - EU average)
    /// </summary>
    public decimal TaxRate { get; set; } = 0.21m;

    /// <summary>
    /// Whether this tariff configuration is currently active (soft delete)
    /// </summary>
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// Currency code for prices (e.g., "EUR", "TRY")
    /// </summary>
    public string Currency { get; set; } = "EUR";

    // Navigation
    public ICollection<FacilityTariff> FacilityTariffs { get; set; } = new List<FacilityTariff>();
}
