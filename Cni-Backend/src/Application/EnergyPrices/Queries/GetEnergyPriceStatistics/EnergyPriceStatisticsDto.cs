namespace Backend.Application.EnergyPrices.Queries.GetEnergyPriceStatistics;

public class EnergyPriceStatisticsDto
{
    public int TsoId { get; set; }
    public string TsoName { get; set; } = string.Empty;
    public string TsoCode { get; set; } = string.Empty;
    public string Currency { get; set; } = string.Empty;
    public DateTimeOffset StartDate { get; set; }
    public DateTimeOffset EndDate { get; set; }
    public int DataPointCount { get; set; }

    // Raw price statistics
    public decimal MinRawPrice { get; set; }
    public decimal MaxRawPrice { get; set; }
    public decimal AvgRawPrice { get; set; }

    // Total price statistics (with tax and distribution)
    public decimal MinTotalPrice { get; set; }
    public decimal MaxTotalPrice { get; set; }
    public decimal AvgTotalPrice { get; set; }

    // Timestamps for min/max
    public DateTimeOffset? MinRawPriceTimestamp { get; set; }
    public DateTimeOffset? MaxRawPriceTimestamp { get; set; }
    public DateTimeOffset? MinTotalPriceTimestamp { get; set; }
    public DateTimeOffset? MaxTotalPriceTimestamp { get; set; }
}
