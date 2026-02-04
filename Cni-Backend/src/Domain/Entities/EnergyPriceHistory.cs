namespace Backend.Domain.Entities;

/// <summary>
/// Stores historical energy price data from Market Operators.
/// Global entity - shared across all tenants for the same Market Operator and country.
/// </summary>
public class EnergyPriceHistory : BaseAuditableEntity
{
    /// <summary>
    /// Reference to the Market Operator
    /// </summary>
    public int MarketOperatorId { get; set; }

    /// <summary>
    /// The country name for this price data (e.g., "Netherlands", "Turkey", "Germany")
    /// Converted from area code when storing.
    /// </summary>
    public string Country { get; set; } = string.Empty;

    /// <summary>
    /// Net price from the Market Operator (per MWh) - before taxes and fees
    /// </summary>
    public decimal NetPrice { get; set; }

    /// <summary>
    /// Gross price (per MWh) - including applicable taxes and fees at time of recording
    /// </summary>
    public decimal GrossPrice { get; set; }

    /// <summary>
    /// Local timestamp of the price data point (in the area's timezone)
    /// </summary>
    public DateTimeOffset PriceLocalTimestamp { get; set; }

    // Navigation property
    public MarketOperator? MarketOperator { get; set; }
}
