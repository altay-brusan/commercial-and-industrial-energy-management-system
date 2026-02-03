namespace Backend.Application.EnergyPrices.Queries.GetLiveTsoPrices;

public class LiveTsoPricesDto
{
    public bool Success { get; set; }
    public string? ErrorMessage { get; set; }
    public int TsoId { get; set; }
    public string TsoName { get; set; } = string.Empty;
    public string TsoCode { get; set; } = string.Empty;
    public string Currency { get; set; } = string.Empty;
    public DateTimeOffset Date { get; set; }
    public decimal TaxRate { get; set; }
    public decimal DistributionFee { get; set; }
    public List<LivePricePointDto> Prices { get; set; } = new();
    public LivePriceStatisticsDto? Statistics { get; set; }
}

public class LivePricePointDto
{
    public DateTimeOffset Timestamp { get; set; }
    public int Hour { get; set; }
    public decimal RawPrice { get; set; }
    public decimal TotalPrice { get; set; }
}

public class LivePriceStatisticsDto
{
    public decimal MinRawPrice { get; set; }
    public decimal MaxRawPrice { get; set; }
    public decimal AvgRawPrice { get; set; }
    public decimal MinTotalPrice { get; set; }
    public decimal MaxTotalPrice { get; set; }
    public decimal AvgTotalPrice { get; set; }
    public int? MinPriceHour { get; set; }
    public int? MaxPriceHour { get; set; }
}
