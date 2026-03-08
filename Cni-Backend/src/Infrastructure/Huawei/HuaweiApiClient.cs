using System.Net.Http.Json;
using System.Text.Json;
using Backend.Application.Common.Interfaces;
using Backend.Application.Common.Models.Huawei;
using Microsoft.Extensions.Logging;

namespace Backend.Infrastructure.Huawei;

public class HuaweiApiClient : IHuaweiApiClient
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<HuaweiApiClient> _logger;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public HuaweiApiClient(HttpClient httpClient, ILogger<HuaweiApiClient> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    public async Task<string> LoginAsync(string baseUrl, string userName, string systemCode, CancellationToken ct = default)
    {
        var url = $"{baseUrl}/login";
        var payload = new { userName, systemCode };

        var response = await _httpClient.PostAsJsonAsync(url, payload, ct);
        response.EnsureSuccessStatusCode();

        // XSRF-TOKEN comes in the Set-Cookie header
        var token = ExtractXsrfToken(response);
        if (string.IsNullOrEmpty(token))
        {
            // Fallback: try reading from response body
            var body = await response.Content.ReadFromJsonAsync<HuaweiApiResponse<object>>(JsonOptions, ct);
            throw new InvalidOperationException(
                $"Login failed: no XSRF-TOKEN in response. FailCode={body?.FailCode}, Message={body?.Message}");
        }

        _logger.LogDebug("Huawei login successful for user {UserName}", userName);
        return token;
    }

    public async Task<HuaweiStationListResponse> GetStationsAsync(string baseUrl, string token, int pageNo = 1, CancellationToken ct = default)
    {
        var url = $"{baseUrl}/getStationList";
        var payload = new { pageNo, pageSize = 100 };

        var result = await PostAsync<HuaweiStationListResponse>(url, token, payload, ct);
        return result ?? new HuaweiStationListResponse();
    }

    public async Task<List<HuaweiDeviceInfo>> GetDeviceListAsync(string baseUrl, string token, string stationCode, CancellationToken ct = default)
    {
        var url = $"{baseUrl}/getDevList";
        var payload = new { stationCodes = stationCode };

        var result = await PostAsync<List<HuaweiDeviceInfo>>(url, token, payload, ct);
        return result ?? [];
    }

    public async Task<List<HuaweiDevRealKpiData>> GetDevRealKpiAsync(string baseUrl, string token, IReadOnlyList<long> devIds, int devTypeId, CancellationToken ct = default)
    {
        var url = $"{baseUrl}/getDevRealKpi";
        var devIdStr = string.Join(",", devIds);
        var payload = new { devIds = devIdStr, devTypeId };

        var result = await PostAsync<List<HuaweiDevRealKpiData>>(url, token, payload, ct);
        return result ?? [];
    }

    public async Task<List<HuaweiStationRealKpiData>> GetStationRealKpiAsync(string baseUrl, string token, IReadOnlyList<string> stationCodes, CancellationToken ct = default)
    {
        var url = $"{baseUrl}/getStationRealKpi";
        var payload = new { stationCodes = string.Join(",", stationCodes) };

        var result = await PostAsync<List<HuaweiStationRealKpiData>>(url, token, payload, ct);
        return result ?? [];
    }

    private async Task<T?> PostAsync<T>(string url, string token, object payload, CancellationToken ct)
    {
        using var request = new HttpRequestMessage(HttpMethod.Post, url);
        request.Headers.Add("XSRF-TOKEN", token);
        request.Content = JsonContent.Create(payload);

        var response = await _httpClient.SendAsync(request, ct);
        response.EnsureSuccessStatusCode();

        var apiResponse = await response.Content.ReadFromJsonAsync<HuaweiApiResponse<T>>(JsonOptions, ct);

        if (apiResponse == null)
        {
            _logger.LogWarning("Null response from {Url}", url);
            return default;
        }

        if (!apiResponse.Success)
        {
            // 305 = token expired, 407 = rate limit
            if (apiResponse.FailCode == 305)
            {
                throw new HuaweiTokenExpiredException($"Token expired (305) calling {url}");
            }

            if (apiResponse.FailCode == 407)
            {
                throw new HuaweiRateLimitException($"Rate limited (407) calling {url}");
            }

            _logger.LogWarning("Huawei API error: FailCode={FailCode}, Message={Message}, Url={Url}",
                apiResponse.FailCode, apiResponse.Message, url);

            throw new InvalidOperationException(
                $"Huawei API error: FailCode={apiResponse.FailCode}, Message={apiResponse.Message}");
        }

        return apiResponse.Data;
    }

    private static string? ExtractXsrfToken(HttpResponseMessage response)
    {
        if (!response.Headers.TryGetValues("Set-Cookie", out var cookies))
            return null;

        foreach (var cookie in cookies)
        {
            if (cookie.StartsWith("XSRF-TOKEN=", StringComparison.OrdinalIgnoreCase))
            {
                var endIndex = cookie.IndexOf(';');
                return endIndex > 0
                    ? cookie[12..endIndex]
                    : cookie[12..];
            }
        }

        return null;
    }
}

public class HuaweiTokenExpiredException : Exception
{
    public HuaweiTokenExpiredException(string message) : base(message) { }
}

public class HuaweiRateLimitException : Exception
{
    public HuaweiRateLimitException(string message) : base(message) { }
}
