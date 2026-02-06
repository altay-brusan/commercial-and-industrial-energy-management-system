namespace Backend.Application.EnergyPrices.Queries.GetLiveDayAheadEnergyPrices;

public class LiveDayAheadEnergyPricesDto
{
    public bool Success { get; set; }
    public string? ErrorMessage { get; set; }
    public int? FirmId { get; set; }
    public int MarketOperatorId { get; set; }
    public string MarketOperatorName { get; set; } = string.Empty;
    public string MarketOperatorCode { get; set; } = string.Empty;
    public string AreaCode { get; set; } = string.Empty;
    public string Country { get; set; } = string.Empty;
    public string Currency { get; set; } = string.Empty;
    public DateTimeOffset StartDate { get; set; }
    public DateTimeOffset EndDate { get; set; }
    public decimal DistributionFee { get; set; }
    public decimal TransmissionFee { get; set; }
    public decimal TaxRate { get; set; }
    public bool UsingDefaultTariff { get; set; }
    public List<LiveDayAheadPricePointDto> Prices { get; set; } = new();
    public LiveDayAheadPriceStatisticsDto? Statistics { get; set; }

    /// <summary>
    /// Total number of price data points returned.
    /// </summary>
    public int DataPointCount { get; set; }

    /// <summary>
    /// The resolution of the returned prices in minutes (15, 30, or 60).
    /// Determined by the Market Operator (e.g., ENTSO-E returns PT15M or PT60M based on market).
    /// </summary>
    public int ResolutionMinutes { get; set; } = 60;

    /// <summary>
    /// The IANA timezone ID for the area (e.g., "Europe/Amsterdam").
    /// </summary>
    public string? Timezone { get; set; }
}

public class LiveDayAheadPricePointDto
{
    /// <summary>
    /// UTC timestamp of the price point.
    /// </summary>
    public DateTimeOffset Timestamp { get; set; }

    /// <summary>
    /// Local timestamp in the area's timezone.
    /// </summary>
    public DateTimeOffset LocalTimestamp { get; set; }

    /// <summary>
    /// Net price per MWh (before tax and distribution fees).
    /// </summary>
    public decimal NetPrice { get; set; }

    /// <summary>
    /// Gross price per MWh (including tax and distribution fees).
    /// </summary>
    public decimal GrossPrice { get; set; }
}

public class LiveDayAheadPriceStatisticsDto
{
    public decimal MinNetPrice { get; set; }
    public decimal MaxNetPrice { get; set; }
    public decimal AvgNetPrice { get; set; }
    public decimal MinGrossPrice { get; set; }
    public decimal MaxGrossPrice { get; set; }
    public decimal AvgGrossPrice { get; set; }
    public int? MinPriceHour { get; set; }
    public int? MaxPriceHour { get; set; }
}
