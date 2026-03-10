using System.Text.Json;
using Backend.Application.Common.Services;

namespace Backend.Application.Dashboard.Queries;

/// <summary>
/// Shared helpers for all Dashboard query handlers.
/// </summary>
public static class DashboardHelpers
{
    /// <summary>
    /// Maps a period string to a UTC date range (start, end).
    /// Supported values: "today" (default), "weekly", "monthly".
    /// </summary>
    public static (DateTimeOffset Start, DateTimeOffset End) GetDateRange(string? period)
    {
        var now = DateTimeOffset.UtcNow;
        var start = period?.ToLowerInvariant() switch
        {
            "weekly" => now.AddDays(-7),
            "monthly" => now.AddDays(-30),
            _ => new DateTimeOffset(now.Year, now.Month, now.Day, 0, 0, 0, TimeSpan.Zero) // today
        };
        return (start, now);
    }

    /// <summary>
    /// Extracts a decimal value from a JSON payload element at the given dot-path.
    /// Returns null if the element is null or the path cannot be resolved to a number.
    /// </summary>
    public static decimal? GetDecimal(JsonElement? el, string path)
    {
        if (el == null) return null;
        var val = PayloadDeserializer.ExtractValue(el.Value, path);
        return val is double d ? (decimal)d : null;
    }

    /// <summary>
    /// Truncates a DateTimeOffset to the start of its hour (UTC).
    /// Used to bucket time-series data by hour for chart data points.
    /// </summary>
    public static DateTimeOffset HourBucket(DateTimeOffset time)
    {
        var utc = time.ToUniversalTime();
        return new DateTimeOffset(utc.Year, utc.Month, utc.Day, utc.Hour, 0, 0, TimeSpan.Zero);
    }

    /// <summary>
    /// Computes the effective cost per kWh from a Tariff entity.
    /// Formula: (DistributionFee + TransmissionFee) / 1000 * (1 + TaxRate)
    /// </summary>
    public static decimal ComputeRatePerKWh(Domain.Entities.Tariff tariff)
    {
        return (tariff.DistributionFee + tariff.TransmissionFee) / 1000m * (1m + tariff.TaxRate);
    }

    /// <summary>
    /// Default electricity rate (EUR/kWh) used when no facility tariff is assigned.
    /// </summary>
    public const decimal DefaultRatePerKWh = 0.25m;

    /// <summary>
    /// Default currency used when no facility tariff is assigned.
    /// </summary>
    public const string DefaultCurrency = "EUR";

    /// <summary>
    /// CO2 emission factor in kg CO2 per kWh (EU grid average).
    /// </summary>
    public const decimal Co2FactorKgPerKWh = 0.4m;

    /// <summary>
    /// Determines the charge level label from a State of Charge percentage.
    /// </summary>
    public static string GetChargeLevel(decimal? socPercent)
    {
        if (socPercent == null) return "Unknown";
        return socPercent switch
        {
            < 20m => "Low",
            < 50m => "Medium",
            < 80m => "High",
            _ => "Full"
        };
    }
}
