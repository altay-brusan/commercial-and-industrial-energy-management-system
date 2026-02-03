namespace Backend.Application.Common.Interfaces;

/// <summary>
/// Service interface for fetching energy prices from Transmission System Operators (TSO).
/// </summary>
public interface ITsoService
{
    /// <summary>
    /// Fetches day-ahead prices from a TSO for a specific date and country.
    /// </summary>
    /// <param name="tsoCode">TSO code (e.g., "ENTSOE", "EPIAS")</param>
    /// <param name="baseUrl">Base URL of the TSO API</param>
    /// <param name="apiKey">API key or credentials</param>
    /// <param name="country">Country name (e.g., "Netherlands", "Germany", "Türkiye")</param>
    /// <param name="date">The date to fetch prices for</param>
    /// <param name="periodMinutes">Requested resolution in minutes (15, 30, or 60). Defaults to 60 (hourly).</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Result containing prices or error message</returns>
    Task<TsoPriceResult> GetDayAheadPricesAsync(
        string tsoCode,
        string baseUrl,
        string? apiKey,
        string? country,
        DateTimeOffset date,
        int? periodMinutes = null,
        CancellationToken cancellationToken = default);
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
    public List<TsoPricePoint> Prices { get; set; } = new();

    /// <summary>
    /// The resolution of the returned prices in minutes (15, 30, or 60).
    /// </summary>
    public int ResolutionMinutes { get; set; } = 60;

    /// <summary>
    /// The IANA timezone ID for the country (e.g., "Europe/Amsterdam").
    /// </summary>
    public string? Timezone { get; set; }
}

/// <summary>
/// Represents a single price point from a TSO.
/// </summary>
public class TsoPricePoint
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
    /// Price per MWh in the TSO's currency.
    /// </summary>
    public decimal PricePerMWh { get; set; }
}
