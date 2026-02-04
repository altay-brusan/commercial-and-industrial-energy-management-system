namespace Backend.Application.EnergyPrices.Queries.GetEnergyPriceStatistics;

public class EnergyPriceStatisticsDto
{
    public int MarketOperatorId { get; set; }
    public string MarketOperatorName { get; set; } = string.Empty;
    public string MarketOperatorCode { get; set; } = string.Empty;
    public string Country { get; set; } = string.Empty;
    public DateTimeOffset StartDate { get; set; }
    public DateTimeOffset EndDate { get; set; }
    public int DataPointCount { get; set; }

    // Net price statistics
    public decimal MinNetPrice { get; set; }
    public decimal MaxNetPrice { get; set; }
    public decimal AvgNetPrice { get; set; }

    // Gross price statistics (with tax and distribution)
    public decimal MinGrossPrice { get; set; }
    public decimal MaxGrossPrice { get; set; }
    public decimal AvgGrossPrice { get; set; }

    // Timestamps for min/max
    public DateTimeOffset? MinNetPriceTimestamp { get; set; }
    public DateTimeOffset? MaxNetPriceTimestamp { get; set; }
    public DateTimeOffset? MinGrossPriceTimestamp { get; set; }
    public DateTimeOffset? MaxGrossPriceTimestamp { get; set; }
}
