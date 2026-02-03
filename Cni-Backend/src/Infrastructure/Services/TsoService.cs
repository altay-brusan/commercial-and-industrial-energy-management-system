using System.Globalization;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using Backend.Application.Common.Interfaces;
using Microsoft.Extensions.Logging;

namespace Backend.Infrastructure.Services;

/// <summary>
/// Service for fetching energy prices from TSOs (ENTSO-E and EPIAS).
/// </summary>
public class TsoService : ITsoService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<TsoService> _logger;

    public TsoService(HttpClient httpClient, ILogger<TsoService> logger)
    {
        _httpClient = httpClient;
        _httpClient.Timeout = TimeSpan.FromSeconds(30);
        _logger = logger;
    }

    public async Task<TsoPriceResult> GetDayAheadPricesAsync(
        string tsoCode,
        string baseUrl,
        string? apiKey,
        string? country,
        DateTimeOffset date,
        int? periodMinutes = null,
        CancellationToken cancellationToken = default)
    {
        var upperTsoCode = tsoCode.ToUpperInvariant();

        // Map country name to TSO-specific area code
        string? areaCode = null;
        if (!string.IsNullOrWhiteSpace(country))
        {
            if (!TsoCountryMapping.IsCountrySupported(upperTsoCode, country))
            {
                return new TsoPriceResult
                {
                    Success = false,
                    ErrorMessage = TsoCountryMapping.GetUnsupportedCountryMessage(upperTsoCode, country),
                    TsoCode = tsoCode
                };
            }
            areaCode = TsoCountryMapping.GetAreaCode(upperTsoCode, country);
        }

        // Default to hourly if not specified
        var targetResolution = periodMinutes ?? 60;

        return upperTsoCode switch
        {
            "ENTSOE" => await GetEntsoepricesAsync(baseUrl, apiKey, areaCode, country, date, targetResolution, cancellationToken),
            "EPIAS" => await GetEpiasPricesAsync(baseUrl, apiKey, date, targetResolution, cancellationToken),
            _ => new TsoPriceResult
            {
                Success = false,
                ErrorMessage = $"Unknown TSO code: {tsoCode}. Supported codes: ENTSOE, EPIAS",
                TsoCode = tsoCode
            }
        };
    }

    #region ENTSO-E Implementation

    private async Task<TsoPriceResult> GetEntsoepricesAsync(
        string baseUrl,
        string? apiKey,
        string? areaCode,
        string? country,
        DateTimeOffset date,
        int targetResolution,
        CancellationToken cancellationToken)
    {
        var timezone = TsoCountryMapping.GetTimezone(country);
        var result = new TsoPriceResult
        {
            TsoCode = "ENTSOE",
            Currency = "EUR",
            Date = date,
            Timezone = timezone,
            ResolutionMinutes = targetResolution
        };

        if (string.IsNullOrEmpty(apiKey))
        {
            result.Success = false;
            result.ErrorMessage = "ENTSO-E API key is required";
            return result;
        }

        if (string.IsNullOrEmpty(areaCode))
        {
            result.Success = false;
            result.ErrorMessage = "ENTSO-E area code is required";
            return result;
        }

        try
        {
            // ENTSO-E uses UTC times in format yyyyMMddHHmm
            // Important: Use the date component directly, don't convert to UTC first
            // as that can shift the date backwards (e.g., 2026-02-03 00:00 +03:00 becomes 2026-02-02 in UTC)
            var targetDate = date.Date;
            var nextDate = targetDate.AddDays(1);

            string periodStart = targetDate.ToString("yyyyMMdd") + "0000";
            string periodEnd = nextDate.ToString("yyyyMMdd") + "0000";

            // Build URL for Day-ahead Prices (documentType A44)
            var url = $"{baseUrl}?securityToken={apiKey}" +
                      $"&documentType=A44" +
                      $"&in_Domain={areaCode}" +
                      $"&out_Domain={areaCode}" +
                      $"&periodStart={periodStart}" +
                      $"&periodEnd={periodEnd}";

            _logger.LogInformation("Fetching ENTSO-E prices for {Date} (periodStart={PeriodStart}, periodEnd={PeriodEnd}) from {BaseUrl}",
                targetDate.ToString("yyyy-MM-dd"), periodStart, periodEnd, baseUrl);

            var response = await _httpClient.GetAsync(url, cancellationToken);
            var content = await response.Content.ReadAsStringAsync(cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                // Parse error from XML
                if (content.Contains("Reason"))
                {
                    var errorDoc = XDocument.Parse(content);
                    var reason = errorDoc.Descendants().FirstOrDefault(e => e.Name.LocalName == "text")?.Value;
                    result.ErrorMessage = $"ENTSO-E API Error: {reason ?? content}";
                }
                else
                {
                    result.ErrorMessage = $"ENTSO-E API request failed. Status: {response.StatusCode}";
                }
                return result;
            }

            // Parse XML response
            var (prices, sourceResolution) = ParseEntsoeXml(content, date, country);

            // Aggregate to target resolution if needed
            if (sourceResolution < targetResolution)
            {
                prices = AggregateToLargerPeriod(prices, sourceResolution, targetResolution);
                result.ResolutionMinutes = targetResolution;
            }
            else
            {
                result.ResolutionMinutes = sourceResolution;
            }

            result.Prices = prices;
            result.Success = result.Prices.Count > 0;

            if (!result.Success)
            {
                result.ErrorMessage = "No price data found in ENTSO-E response";
            }

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching ENTSO-E prices");
            result.Success = false;
            result.ErrorMessage = $"Error fetching ENTSO-E prices: {ex.Message}";
            return result;
        }
    }

    /// <summary>
    /// Parses ISO 8601 duration string to minutes.
    /// Supports: PT15M, PT30M, PT60M, PT1H
    /// </summary>
    private static int ParseIso8601DurationToMinutes(string duration)
    {
        if (string.IsNullOrWhiteSpace(duration))
            return 60; // Default to hourly

        // Match PT[number]M pattern (e.g., PT15M, PT30M, PT60M)
        var minuteMatch = Regex.Match(duration, @"PT(\d+)M", RegexOptions.IgnoreCase);
        if (minuteMatch.Success && int.TryParse(minuteMatch.Groups[1].Value, out int minutes))
        {
            return minutes;
        }

        // Match PT[number]H pattern (e.g., PT1H)
        var hourMatch = Regex.Match(duration, @"PT(\d+)H", RegexOptions.IgnoreCase);
        if (hourMatch.Success && int.TryParse(hourMatch.Groups[1].Value, out int hours))
        {
            return hours * 60;
        }

        return 60; // Default to hourly
    }

    private (List<TsoPricePoint> Prices, int SourceResolution) ParseEntsoeXml(string xmlContent, DateTimeOffset date, string? country)
    {
        var prices = new List<TsoPricePoint>();
        int sourceResolution = 60; // Default to hourly

        try
        {
            var doc = XDocument.Parse(xmlContent);

            // Get all TimeSeries elements (there may be multiple)
            var allTimeSeries = doc.Descendants().Where(e => e.Name.LocalName == "TimeSeries").ToList();

            if (allTimeSeries.Count == 0)
                return (prices, sourceResolution);

            foreach (var timeSeries in allTimeSeries)
            {
                // Each TimeSeries can have multiple Periods
                var periods = timeSeries.Descendants().Where(e => e.Name.LocalName == "Period").ToList();

                foreach (var period in periods)
                {
                    // Get the period start time from timeInterval
                    var startStr = period.Descendants()
                        .FirstOrDefault(e => e.Name.LocalName == "start")?.Value;

                    DateTimeOffset periodStart;
                    if (!string.IsNullOrEmpty(startStr) && DateTimeOffset.TryParse(startStr, out var parsedStart))
                    {
                        periodStart = parsedStart;
                    }
                    else
                    {
                        // Fallback to requested date
                        periodStart = date.Date;
                    }

                    // Parse resolution from XML (e.g., PT15M, PT60M)
                    var resolutionStr = period.Descendants()
                        .FirstOrDefault(e => e.Name.LocalName == "resolution")?.Value;
                    sourceResolution = ParseIso8601DurationToMinutes(resolutionStr ?? "PT60M");

                    var points = period.Descendants().Where(e => e.Name.LocalName == "Point");

                    foreach (var point in points)
                    {
                        var positionStr = point.Descendants().FirstOrDefault(e => e.Name.LocalName == "position")?.Value;
                        var priceStr = point.Descendants().FirstOrDefault(e => e.Name.LocalName == "price.amount")?.Value;

                        if (int.TryParse(positionStr, out int position) &&
                            decimal.TryParse(priceStr, NumberStyles.Any, CultureInfo.InvariantCulture, out decimal price))
                        {
                            // FIXED: Calculate timestamp based on actual resolution
                            // Position is 1-based, so position 1 = minute 0
                            var minutesOffset = (position - 1) * sourceResolution;
                            var timestamp = periodStart.AddMinutes(minutesOffset);

                            // Convert to local timezone
                            var localTimestamp = TsoCountryMapping.ConvertToLocalTime(timestamp, country);

                            prices.Add(new TsoPricePoint
                            {
                                Timestamp = timestamp,
                                LocalTimestamp = localTimestamp,
                                PricePerMWh = price
                            });
                        }
                    }
                }
            }

            // Filter to only include prices for the requested date (in local time) and sort by timestamp
            var targetDate = date.Date;
            prices = prices
                .Where(p => p.LocalTimestamp.Date == targetDate)
                .OrderBy(p => p.Timestamp)
                .ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to parse ENTSO-E XML response");
        }

        return (prices, sourceResolution);
    }

    /// <summary>
    /// Aggregates prices from a smaller period to a larger period.
    /// Takes the first value of each larger period (e.g., 00:00, 00:15, 00:30, 00:45 → 00:00).
    /// </summary>
    private static List<TsoPricePoint> AggregateToLargerPeriod(
        List<TsoPricePoint> prices,
        int sourceResolution,
        int targetResolution)
    {
        if (prices.Count == 0 || sourceResolution >= targetResolution)
            return prices;

        var aggregated = new List<TsoPricePoint>();

        // Group by the start of each target period
        var grouped = prices
            .GroupBy(p =>
            {
                // Calculate which target period this price belongs to
                var totalMinutes = p.LocalTimestamp.Hour * 60 + p.LocalTimestamp.Minute;
                var periodIndex = totalMinutes / targetResolution;
                return periodIndex;
            })
            .OrderBy(g => g.Key);

        foreach (var group in grouped)
        {
            // Take the first price in each period
            var firstPrice = group.OrderBy(p => p.Timestamp).First();

            // Adjust the local timestamp to the start of the period
            var periodStartMinutes = group.Key * targetResolution;
            var periodHour = periodStartMinutes / 60;
            var periodMinute = periodStartMinutes % 60;

            // Calculate the UTC timestamp for the period start
            var localPeriodStart = new DateTimeOffset(
                firstPrice.LocalTimestamp.Year,
                firstPrice.LocalTimestamp.Month,
                firstPrice.LocalTimestamp.Day,
                periodHour,
                periodMinute,
                0,
                firstPrice.LocalTimestamp.Offset);

            aggregated.Add(new TsoPricePoint
            {
                Timestamp = localPeriodStart.ToUniversalTime(),
                LocalTimestamp = localPeriodStart,
                PricePerMWh = firstPrice.PricePerMWh
            });
        }

        return aggregated;
    }

    #endregion

    #region EPIAS Implementation

    private async Task<TsoPriceResult> GetEpiasPricesAsync(
        string baseUrl,
        string? apiKey,
        DateTimeOffset date,
        int targetResolution,
        CancellationToken cancellationToken)
    {
        var result = new TsoPriceResult
        {
            TsoCode = "EPIAS",
            Currency = "TRY",
            Date = date,
            Timezone = "Europe/Istanbul",
            ResolutionMinutes = 60 // EPIAS always returns hourly data
        };

        try
        {
            // EPIAS requires CAS authentication first
            // apiKey format: "username:password"
            if (string.IsNullOrEmpty(apiKey) || !apiKey.Contains(':'))
            {
                result.Success = false;
                result.ErrorMessage = "EPIAS credentials required in format 'username:password'";
                return result;
            }

            var credentials = apiKey.Split(':');
            var username = credentials[0];
            var password = credentials[1];

            // Step 1: Get TGT token from CAS
            var tgtToken = await GetEpiasTgtTokenAsync(username, password, cancellationToken);
            if (string.IsNullOrEmpty(tgtToken))
            {
                result.Success = false;
                result.ErrorMessage = "Failed to authenticate with EPIAS CAS";
                return result;
            }

            // Step 2: Fetch price data
            var dateStr = date.ToString("yyyy-MM-dd");
            var dataUrl = $"{baseUrl}/electricity-service/v1/markets/dam/data/mcp";

            var requestBody = new
            {
                startDate = $"{dateStr}T00:00:00+03:00",
                endDate = $"{dateStr}T23:59:59+03:00"
            };

            var jsonContent = JsonSerializer.Serialize(requestBody);
            var httpContent = new StringContent(jsonContent, Encoding.UTF8, "application/json");

            var request = new HttpRequestMessage(HttpMethod.Post, dataUrl);
            request.Content = httpContent;
            request.Headers.Add("TGT", tgtToken);
            request.Headers.Add("Accept", "application/json");

            _logger.LogInformation("Fetching EPIAS prices for {Date}", dateStr);

            var response = await _httpClient.SendAsync(request, cancellationToken);
            var content = await response.Content.ReadAsStringAsync(cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                result.Success = false;
                result.ErrorMessage = $"EPIAS API request failed. Status: {response.StatusCode}";
                return result;
            }

            // Parse JSON response
            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            var epiasResponse = JsonSerializer.Deserialize<EpiasResponse>(content, options);

            if (epiasResponse?.Items == null || epiasResponse.Items.Count == 0)
            {
                result.Success = false;
                result.ErrorMessage = "No price data found in EPIAS response";
                return result;
            }

            var turkeyOffset = TimeSpan.FromHours(3);
            result.Prices = epiasResponse.Items
                .Select(item =>
                {
                    var localTimestamp = new DateTimeOffset(item.Date, turkeyOffset);
                    return new TsoPricePoint
                    {
                        Timestamp = localTimestamp.ToUniversalTime(),
                        LocalTimestamp = localTimestamp,
                        PricePerMWh = item.Price
                    };
                })
                .OrderBy(p => p.LocalTimestamp)
                .ToList();

            result.Success = true;
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching EPIAS prices");
            result.Success = false;
            result.ErrorMessage = $"Error fetching EPIAS prices: {ex.Message}";
            return result;
        }
    }

    private async Task<string?> GetEpiasTgtTokenAsync(string username, string password, CancellationToken cancellationToken)
    {
        const string casUrl = "https://giris.epias.com.tr/cas/v1/tickets";

        var content = new FormUrlEncodedContent(new[]
        {
            new KeyValuePair<string, string>("username", username),
            new KeyValuePair<string, string>("password", password)
        });

        var response = await _httpClient.PostAsync(casUrl, content, cancellationToken);

        if (response.IsSuccessStatusCode)
        {
            var location = response.Headers.Location?.ToString();
            if (!string.IsNullOrEmpty(location))
            {
                return location.Substring(location.LastIndexOf('/') + 1);
            }
        }

        _logger.LogWarning("EPIAS CAS authentication failed. Status: {StatusCode}", response.StatusCode);
        return null;
    }

    #endregion

    #region EPIAS DTOs

    private class EpiasResponse
    {
        [JsonPropertyName("items")]
        public List<EpiasPtfItem> Items { get; set; } = new();
    }

    private class EpiasPtfItem
    {
        [JsonPropertyName("date")]
        public DateTime Date { get; set; }

        [JsonPropertyName("price")]
        public decimal Price { get; set; }
    }

    #endregion
}
