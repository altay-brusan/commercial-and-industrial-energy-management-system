namespace Backend.Application.EnergyPrices.Queries.GetDayAheadPrices;

/// <summary>
/// DTO for Day-Ahead energy prices response.
/// </summary>
public class DayAheadEnergyPricesDto
{
    public int FacilityId { get; set; }
    public string FacilityName { get; set; } = string.Empty;
    public string Country { get; set; } = string.Empty;
    public int MarketOperatorId { get; set; }
    public string MarketOperatorCode { get; set; } = string.Empty;
    public DateTimeOffset StartDate { get; set; }
    public DateTimeOffset EndDate { get; set; }
    public int DataPointCount { get; set; }

    // Tariff information from facility's active tariff assignment
    public decimal DiscountRate { get; set; }
    public decimal DistributionFee { get; set; }
    public decimal TransmissionFee { get; set; }
    public decimal TaxRate { get; set; }
    public string Currency { get; set; } = "EUR";

    public List<DayAheadEnergyPricePointDto> Prices { get; set; } = new();
}

/// <summary>
/// Individual price point in the Day-Ahead time series.
/// </summary>
public class DayAheadEnergyPricePointDto
{
    public DateTimeOffset TimestampUtc { get; set; }

    /// <summary>
    /// Net price per MWh (before taxes and fees).
    /// </summary>
    public decimal NetPrice { get; set; }

    /// <summary>
    /// Gross price per MWh (including taxes and fees from Market Operator).
    /// </summary>
    public decimal GrossPrice { get; set; }

    /// <summary>
    /// Gross price after applying facility's discount rate.
    /// Only populated if facility has a discount.
    /// </summary>
    public decimal? DiscountedGrossPrice { get; set; }
}
