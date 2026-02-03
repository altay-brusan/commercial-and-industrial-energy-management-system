namespace Backend.Domain.Entities;

/// <summary>
/// Stores historical energy price data from TSO providers.
/// Includes raw price, applied rates, and calculated total price.
/// </summary>
public class EnergyPriceHistory : BaseAuditableEntity, IMultiTenantEntity
{
    public int TenantId { get; set; }

    /// <summary>
    /// Reference to the Transmission System Operator (TSO)
    /// </summary>
    public int TsoId { get; set; }

    /// <summary>
    /// Raw price from the TSO (per MWh)
    /// </summary>
    public decimal RawPrice { get; set; }

    /// <summary>
    /// Tax rate that was applied at the time of recording (as decimal)
    /// </summary>
    public decimal AppliedTaxRate { get; set; }

    /// <summary>
    /// Distribution fee that was applied at the time of recording (per MWh)
    /// </summary>
    public decimal AppliedDistributionFee { get; set; }

    /// <summary>
    /// Transmission fee that was applied at the time of recording (per MWh)
    /// </summary>
    public decimal AppliedTransmissionFee { get; set; }

    /// <summary>
    /// Total calculated price: (RawPrice + DistributionFee + TransmissionFee) * (1 + TaxRate)
    /// </summary>
    public decimal TotalPrice { get; set; }

    /// <summary>
    /// Timestamp of the price data point (UTC)
    /// </summary>
    public DateTimeOffset PriceTimestamp { get; set; }

    /// <summary>
    /// Currency code at the time of recording
    /// </summary>
    public string Currency { get; set; } = "EUR";

    public Tenant? Tenant { get; set; }

    public TransmissionSystemOperator? Tso { get; set; }
}
