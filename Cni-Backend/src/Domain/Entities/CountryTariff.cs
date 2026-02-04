namespace Backend.Domain.Entities;

/// <summary>
/// Represents country-specific tariff configuration for energy pricing.
/// Includes distribution fees, transmission fees, and tax rates per country per tenant.
/// </summary>
public class CountryTariff : BaseAuditableEntity, IMultiTenantEntity
{
    public int TenantId { get; set; }

    /// <summary>
    /// ISO 3166-1 alpha-2 country code (e.g., "NL", "DE", "TR")
    /// </summary>
    public string CountryCode { get; set; } = string.Empty;

    /// <summary>
    /// Full country name (e.g., "Netherlands", "Germany", "Türkiye")
    /// </summary>
    public string CountryName { get; set; } = string.Empty;

    /// <summary>
    /// Distribution fee per MWh (local grid distribution costs)
    /// Default: 20 €/MWh (EU average)
    /// </summary>
    public decimal DistributionFee { get; set; } = 20m;

    /// <summary>
    /// Transmission fee per MWh (high-voltage transmission costs)
    /// Default: 5 €/MWh (EU average)
    /// </summary>
    public decimal TransmissionFee { get; set; } = 5m;

    /// <summary>
    /// Tax rate applied to gross price (as decimal, e.g., 0.21 for 21%)
    /// Includes VAT and electricity-specific taxes
    /// Default: 0.21 (21% - EU average)
    /// </summary>
    public decimal TaxRate { get; set; } = 0.21m;

    /// <summary>
    /// Whether this tariff configuration is currently active
    /// </summary>
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// Currency code for prices in this country (e.g., "EUR", "TRY")
    /// </summary>
    public string Currency { get; set; } = "EUR";

    // Navigation
    public Tenant? Tenant { get; set; }
}
