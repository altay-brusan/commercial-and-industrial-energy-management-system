namespace Backend.Application.Common.Interfaces;

/// <summary>
/// Service interface for fetching energy prices from Transmission System Operators (TSO).
/// </summary>
public interface ITsoService
{
    /// <summary>
    /// Fetches day-ahead prices from a TSO for a specific date.
    /// </summary>
    /// <param name="tsoCode">TSO code (e.g., "ENTSOE", "EPIAS")</param>
    /// <param name="baseUrl">Base URL of the TSO API</param>
    /// <param name="apiKey">API key or credentials</param>
    /// <param name="areaCode">Area/bidding zone code (optional, used by ENTSO-E)</param>
    /// <param name="date">The date to fetch prices for</param>
    /// <returns>List of hourly prices</returns>
    Task<TsoPriceResult> GetDayAheadPricesAsync(string tsoCode, string baseUrl, string? apiKey, string? areaCode, DateTimeOffset date, CancellationToken cancellationToken = default);
}

/// <summary>
/// Result of a TSO price fetch operation.
/// </summary>
public class TsoPriceResult
{
    public bool Success { get; set; }
    public string? ErrorMessage { get; set; }
    public string TsoCode { get; set; } = string.Empty;
    public string Currency { get; set; } = string.Empty;
    public DateTimeOffset Date { get; set; }
    public List<TsoHourlyPrice> Prices { get; set; } = new();
}

/// <summary>
/// Represents a single hourly price from a TSO.
/// </summary>
public class TsoHourlyPrice
{
    public DateTimeOffset Timestamp { get; set; }
    public int Hour { get; set; }
    public decimal PricePerMWh { get; set; }
}
