using System.Text.Json;
using System.Text.Json.Serialization;
using Backend.Application.Common.Interfaces;
using Backend.Domain.Entities;

namespace Backend.Infrastructure.Services;

public static class RtuPricePayloadBuilder
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    public static string Build(
        List<EnergyPricePoint> prices,
        MarketOperator mo,
        string currency,
        DateTimeOffset date)
    {
        // Determine conversion factor: MO provides in PriceUnit, RTU expects kWh
        var conversionFactor = GetConversionToKwh(mo.PriceUnit);
        var unitLabel = $"{currency}/kWh";

        var hourlyPrices = prices
            .OrderBy(p => p.LocalTimestamp)
            .Select(p => new RtuHourlyPrice
            {
                HourStart = p.LocalTimestamp.Hour,
                HourEnd = p.LocalTimestamp.Hour + 1,
                Price = Math.Round(p.PricePerMWh * conversionFactor, 4)
            })
            .ToList();

        var payload = new RtuDayAheadPayload
        {
            Type = "day-ahead-prices",
            Date = date.ToString("yyyy-MM-dd"),
            Currency = currency,
            Unit = unitLabel,
            Source = $"{mo.Code}_DAF",
            Prices = hourlyPrices
        };

        return JsonSerializer.Serialize(payload, JsonOptions);
    }

    private static decimal GetConversionToKwh(string priceUnit)
    {
        return priceUnit.ToUpperInvariant() switch
        {
            "MWH" => 0.001m,   // 1 MWh = 1000 kWh, so divide by 1000
            "KWH" => 1m,       // Already in kWh
            "GWH" => 0.000001m,
            _ => 0.001m        // Default: assume MWh
        };
    }
}

public class RtuDayAheadPayload
{
    public string Type { get; set; } = string.Empty;
    public string Date { get; set; } = string.Empty;
    public string Currency { get; set; } = string.Empty;
    public string Unit { get; set; } = string.Empty;
    public string Source { get; set; } = string.Empty;
    public List<RtuHourlyPrice> Prices { get; set; } = new();
}

public class RtuHourlyPrice
{
    public int HourStart { get; set; }
    public int HourEnd { get; set; }
    public decimal Price { get; set; }
}
