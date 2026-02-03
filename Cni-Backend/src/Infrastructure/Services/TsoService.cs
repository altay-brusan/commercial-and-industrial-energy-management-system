using System.Globalization;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
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
        string? areaCode,
        DateTimeOffset date,
        CancellationToken cancellationToken = default)
    {
        return tsoCode.ToUpperInvariant() switch
        {
            "ENTSOE" => await GetEntsoepricesAsync(baseUrl, apiKey, areaCode, date, cancellationToken),
            "EPIAS" => await GetEpiasPricesAsync(baseUrl, apiKey, date, cancellationToken),
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
        DateTimeOffset date,
        CancellationToken cancellationToken)
    {
        var result = new TsoPriceResult
        {
            TsoCode = "ENTSOE",
            Currency = "EUR",
            Date = date
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
            var startUtc = date.UtcDateTime.Date;
            var endUtc = startUtc.AddDays(1);

            string periodStart = startUtc.ToString("yyyyMMdd") + "0000";
            string periodEnd = endUtc.ToString("yyyyMMdd") + "0000";

            // Build URL for Day-ahead Prices (documentType A44)
            var url = $"{baseUrl}?securityToken={apiKey}" +
                      $"&documentType=A44" +
                      $"&in_Domain={areaCode}" +
                      $"&out_Domain={areaCode}" +
                      $"&periodStart={periodStart}" +
                      $"&periodEnd={periodEnd}";

            _logger.LogInformation("Fetching ENTSO-E prices for {Date} from {Url}", date.ToString("yyyy-MM-dd"), baseUrl);

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
            result.Prices = ParseEntsoeXml(content, date);
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

    private List<TsoHourlyPrice> ParseEntsoeXml(string xmlContent, DateTimeOffset date)
    {
        var prices = new List<TsoHourlyPrice>();

        try
        {
            var doc = XDocument.Parse(xmlContent);
            XNamespace ns = "urn:iec62325.351:tc57wg16:451-3:publicationdocument:7:3";

            var timeSeries = doc.Descendants(ns + "TimeSeries").FirstOrDefault();
            if (timeSeries == null)
            {
                // Try without namespace
                timeSeries = doc.Descendants().FirstOrDefault(e => e.Name.LocalName == "TimeSeries");
            }

            if (timeSeries == null)
                return prices;

            var period = timeSeries.Descendants().FirstOrDefault(e => e.Name.LocalName == "Period");
            if (period == null)
                return prices;

            var points = period.Descendants().Where(e => e.Name.LocalName == "Point");

            foreach (var point in points)
            {
                var positionStr = point.Descendants().FirstOrDefault(e => e.Name.LocalName == "position")?.Value;
                var priceStr = point.Descendants().FirstOrDefault(e => e.Name.LocalName == "price.amount")?.Value;

                if (int.TryParse(positionStr, out int position) &&
                    decimal.TryParse(priceStr, NumberStyles.Any, CultureInfo.InvariantCulture, out decimal price))
                {
                    var hour = position - 1; // Position is 1-based, convert to 0-23
                    prices.Add(new TsoHourlyPrice
                    {
                        Hour = hour,
                        Timestamp = date.Date.AddHours(hour),
                        PricePerMWh = price
                    });
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to parse ENTSO-E XML response");
        }

        return prices;
    }

    #endregion

    #region EPIAS Implementation

    private async Task<TsoPriceResult> GetEpiasPricesAsync(
        string baseUrl,
        string? apiKey,
        DateTimeOffset date,
        CancellationToken cancellationToken)
    {
        var result = new TsoPriceResult
        {
            TsoCode = "EPIAS",
            Currency = "TRY",
            Date = date
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

            result.Prices = epiasResponse.Items
                .Select(item => new TsoHourlyPrice
                {
                    Hour = item.Date.Hour,
                    Timestamp = new DateTimeOffset(item.Date, TimeSpan.FromHours(3)), // Turkey is UTC+3
                    PricePerMWh = item.Price
                })
                .OrderBy(p => p.Hour)
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
