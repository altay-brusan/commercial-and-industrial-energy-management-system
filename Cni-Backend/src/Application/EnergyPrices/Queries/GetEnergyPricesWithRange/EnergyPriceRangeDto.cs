namespace Backend.Application.EnergyPrices.Queries.GetEnergyPricesWithRange;

public class EnergyPriceRangeDto
{
    public int MarketOperatorId { get; set; }
    public string MarketOperatorName { get; set; } = string.Empty;
    public string MarketOperatorCode { get; set; } = string.Empty;
    public string AreaCode { get; set; } = string.Empty;
    public DateTimeOffset StartDate { get; set; }
    public DateTimeOffset EndDate { get; set; }
    public List<EnergyPricePointDto> Prices { get; set; } = new();
}

public class EnergyPricePointDto
{
    public DateTimeOffset Timestamp { get; set; }
    public decimal NetPrice { get; set; }
    public decimal GrossPrice { get; set; }
}
