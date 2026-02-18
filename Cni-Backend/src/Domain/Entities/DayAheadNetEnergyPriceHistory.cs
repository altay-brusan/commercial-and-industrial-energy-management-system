namespace Backend.Domain.Entities;

/// <summary>
/// Stores historical Day-Ahead energy price data from Market Operators.
/// Day-Ahead prices are published one day before delivery.
/// Global entity - shared across all tenants for the same Market Operator and country.
/// </summary>
public class DayAheadNetEnergyPriceHistory : BaseAuditableEntity
{
    /// <summary>
    /// Reference to the Market Operator
    /// </summary>
    public Guid MarketOperatorId { get; set; }

    /// <summary>
    /// The country name for this price data (e.g., "Netherlands", "Turkey", "Germany")
    /// Converted from area code when storing.
    /// </summary>
    public string Country { get; set; } = string.Empty;

    /// <summary>
    /// Net price from the Market Operator (per MWh) - before taxes and fees.
    /// Gross price is calculated at query time using the facility's tariff.
    /// </summary>
    public decimal NetPrice { get; set; }

    /// <summary>
    /// UTC timestamp of the price data point.
    /// Use the Country field to determine the local timezone for display.
    /// </summary>
    public DateTimeOffset TimestampUtc { get; set; }

    // Navigation property
    public MarketOperator? MarketOperator { get; set; }
}
