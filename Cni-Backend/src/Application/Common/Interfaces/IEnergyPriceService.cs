namespace Backend.Application.Common.Interfaces;

/// <summary>
/// Service interface for fetching energy prices from Market Operators (ENTSO-E, EPIAS, etc.).
/// </summary>
public interface IEnergyPriceService
{
    /// <summary>
    /// Fetches day-ahead prices from a Market Operator for a specific date and country.
    /// Resolution is determined by the Market Operator (e.g., ENTSO-E returns PT15M or PT60M based on market).
    /// </summary>
    /// <param name="moCode">Market Operator code (e.g., "ENTSOE", "EPIAS")</param>
    /// <param name="baseUrl">Base URL of the Market Operator API</param>
    /// <param name="apiKey">API key or credentials</param>
    /// <param name="country">Country name (e.g., "Netherlands", "Germany", "Türkiye")</param>
    /// <param name="date">The date to fetch prices for</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Result containing prices or error message</returns>
    Task<EnergyPriceResult> GetDayAheadPricesAsync(
        string moCode,
        string baseUrl,
        string? apiKey,
        string? country,
        DateTimeOffset date,
        CancellationToken cancellationToken = default);
}

/// <summary>
/// Result of an energy price fetch operation.
/// </summary>
public class EnergyPriceResult
{
    public bool Success { get; set; }
    public string? ErrorMessage { get; set; }
    public string MarketOperatorCode { get; set; } = string.Empty;
    public string Currency { get; set; } = string.Empty;
    public DateTimeOffset Date { get; set; }
    public List<EnergyPricePoint> Prices { get; set; } = new();

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
/// Represents a single energy price point.
/// </summary>
public class EnergyPricePoint
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
    /// Price per MWh in the Market Operator's currency.
    /// </summary>
    public decimal PricePerMWh { get; set; }
}
