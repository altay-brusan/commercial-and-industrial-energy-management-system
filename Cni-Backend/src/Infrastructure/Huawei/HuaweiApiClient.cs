using System.Net.Http.Json;
using System.Text.Json;
using Backend.Application.Common.Interfaces;
using Backend.Application.Common.Models.Huawei;
using Microsoft.Extensions.Logging;

namespace Backend.Infrastructure.Huawei;

/// <summary>
/// Session token containing XSRF-TOKEN and all session cookies from login.
/// Huawei requires both the XSRF-TOKEN header AND all session cookies on every request.
/// </summary>
public class HuaweiSession
{
    public required string XsrfToken { get; init; }
    public required string CookieHeader { get; init; }
}

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
        var session = await LoginForSessionAsync(baseUrl, userName, systemCode, ct);
        // Serialize session as "xsrfToken|||cookieHeader" so we can pass it as a single string
        // through the existing interface without breaking the contract
        return $"{session.XsrfToken}|||{session.CookieHeader}";
    }

    private async Task<HuaweiSession> LoginForSessionAsync(string baseUrl, string userName, string systemCode, CancellationToken ct)
    {
        var url = $"{baseUrl}/login";
        var payload = new { userName, systemCode };

        var response = await _httpClient.PostAsJsonAsync(url, payload, ct);
        response.EnsureSuccessStatusCode();

        // Collect all Set-Cookie headers
        string? xsrfToken = null;
        var cookieParts = new List<string>();

        if (response.Headers.TryGetValues("Set-Cookie", out var setCookies))
        {
            foreach (var setCookie in setCookies)
            {
                // Extract cookie name=value (before the first ';')
                var semiIdx = setCookie.IndexOf(';');
                var nameValue = semiIdx > 0 ? setCookie[..semiIdx] : setCookie;
                cookieParts.Add(nameValue);

                if (nameValue.StartsWith("XSRF-TOKEN=", StringComparison.OrdinalIgnoreCase))
                {
                    xsrfToken = nameValue["XSRF-TOKEN=".Length..];
                }
            }
        }

        // Fallback: check response body
        if (string.IsNullOrEmpty(xsrfToken))
        {
            var body = await response.Content.ReadFromJsonAsync<HuaweiApiResponse<JsonElement?>>(JsonOptions, ct);

            if (body is { Success: false })
            {
                throw new InvalidOperationException(
                    $"Huawei login failed: FailCode={body.FailCode}, Message={body.Message}");
            }

            if (body?.Params is { ValueKind: JsonValueKind.Object } paramsEl)
            {
                if (paramsEl.TryGetProperty("csrfToken", out var csrfProp))
                {
                    xsrfToken = csrfProp.GetString();
                    cookieParts.Add($"XSRF-TOKEN={xsrfToken}");
                }
            }
        }

        if (string.IsNullOrEmpty(xsrfToken))
        {
            throw new InvalidOperationException(
                "Login succeeded but no XSRF-TOKEN found in response cookies or body");
        }

        var cookieHeader = string.Join("; ", cookieParts);
        _logger.LogDebug("Huawei login successful for user {UserName}, {CookieCount} cookies captured",
            userName, cookieParts.Count);

        return new HuaweiSession { XsrfToken = xsrfToken, CookieHeader = cookieHeader };
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

    public async Task<List<HuaweiAlarmData>> GetAlarmListAsync(
        string baseUrl, string token, string stationCodes,
        long beginTimeMs, long endTimeMs, CancellationToken ct = default)
    {
        var url = $"{baseUrl}/getAlarmList";
        var payload = new { stationCodes, beginTime = beginTimeMs, endTime = endTimeMs, language = "en_US" };
        var result = await PostAsync<List<HuaweiAlarmData>>(url, token, payload, ct);
        return result ?? [];
    }

    private async Task<T?> PostAsync<T>(string url, string sessionToken, object payload, CancellationToken ct)
    {
        // Parse the combined session token: "xsrfToken|||cookieHeader"
        var (xsrfToken, cookieHeader) = ParseSessionToken(sessionToken);

        using var request = new HttpRequestMessage(HttpMethod.Post, url);
        request.Headers.Add("XSRF-TOKEN", xsrfToken);
        request.Headers.Add("Cookie", cookieHeader);
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

    private static (string xsrfToken, string cookieHeader) ParseSessionToken(string sessionToken)
    {
        var separatorIdx = sessionToken.IndexOf("|||", StringComparison.Ordinal);
        if (separatorIdx > 0)
        {
            return (sessionToken[..separatorIdx], sessionToken[(separatorIdx + 3)..]);
        }

        // Fallback: treat entire string as XSRF-TOKEN (backwards compatibility)
        return (sessionToken, $"XSRF-TOKEN={sessionToken}");
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
