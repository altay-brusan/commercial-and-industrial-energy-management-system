namespace Backend.Application.EnergyPrices.Queries.GetLiveTsoPrices;

public class LiveTsoPricesDto
{
    public bool Success { get; set; }
    public string? ErrorMessage { get; set; }
    public int TsoId { get; set; }
    public string TsoName { get; set; } = string.Empty;
    public string TsoCode { get; set; } = string.Empty;
    public string? Country { get; set; }
    public string Currency { get; set; } = string.Empty;
    public DateTimeOffset Date { get; set; }
    public decimal DistributionFee { get; set; }
    public decimal TransmissionFee { get; set; }
    public decimal TaxRate { get; set; }
    public bool UsingDefaultTariff { get; set; }
    public List<LivePricePointDto> Prices { get; set; } = new();
    public LivePriceStatisticsDto? Statistics { get; set; }

    /// <summary>
    /// The resolution of the returned prices in minutes (15, 30, or 60).
    /// </summary>
    public int ResolutionMinutes { get; set; } = 60;

    /// <summary>
    /// The IANA timezone ID for the country (e.g., "Europe/Amsterdam").
    /// </summary>
    public string? Timezone { get; set; }
}

public class LivePricePointDto
{
    /// <summary>
    /// UTC timestamp of the price point.
    /// </summary>
    public DateTimeOffset Timestamp { get; set; }

    /// <summary>
    /// Local timestamp in the country's timezone.
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

public class LivePriceStatisticsDto
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
