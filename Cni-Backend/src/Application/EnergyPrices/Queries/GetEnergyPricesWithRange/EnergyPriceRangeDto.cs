namespace Backend.Application.EnergyPrices.Queries.GetEnergyPricesWithRange;

public class EnergyPriceRangeDto
{
    public int TsoId { get; set; }
    public string TsoName { get; set; } = string.Empty;
    public string TsoCode { get; set; } = string.Empty;
    public string Currency { get; set; } = string.Empty;
    public DateTimeOffset StartDate { get; set; }
    public DateTimeOffset EndDate { get; set; }
    public List<EnergyPricePointDto> Prices { get; set; } = new();
}

public class EnergyPricePointDto
{
    public DateTimeOffset Timestamp { get; set; }
    public decimal RawPrice { get; set; }
    public decimal TotalPrice { get; set; }
}
