namespace Backend.Application.EnergyPrices.Queries.GetHistoricalEnergyPrices;

public class HistoricalEnergyPricesDto
{
    public string Country { get; set; } = string.Empty;
    public DateTimeOffset StartDate { get; set; }
    public DateTimeOffset EndDate { get; set; }
    public int DataPointCount { get; set; }
    public List<HistoricalPricePointDto> Prices { get; set; } = new();
}

public class HistoricalPricePointDto
{
    public DateTimeOffset Timestamp { get; set; }
    public decimal NetPrice { get; set; }
    public decimal GrossPrice { get; set; }
    public int MarketOperatorId { get; set; }
    public string MarketOperatorCode { get; set; } = string.Empty;
}
