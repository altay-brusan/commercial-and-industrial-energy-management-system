using System.Net.Http.Json;
using System.Text.Json;
using Backend.Application.Common.Interfaces;
using Backend.Application.Common.Models.Alm;
using Backend.Infrastructure.Configurations;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Backend.Infrastructure.Alm;

public class AlmApiClient : IAlmApiClient
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<AlmApiClient> _logger;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public AlmApiClient(HttpClient httpClient, IOptions<AlmSettings> settings, ILogger<AlmApiClient> logger)
    {
        _httpClient = httpClient;
        _logger = logger;

        var baseUrl = settings.Value.BaseUrl;
        if (!string.IsNullOrEmpty(baseUrl))
        {
            _httpClient.BaseAddress = new Uri(baseUrl.TrimEnd('/') + "/");
        }
    }

    public async Task<AlmSubmitJobResponse> SubmitJobAsync(string module, AlmSubmitJobRequest request, CancellationToken ct = default)
    {
        _logger.LogDebug("Submitting ALM job to {Module}", module);

        var response = await _httpClient.PostAsJsonAsync(module, request, ct);

        if (!response.IsSuccessStatusCode)
        {
            var errorBody = await response.Content.ReadAsStringAsync(ct);
            _logger.LogError("ALM API error {StatusCode} from {Module}: {Body}",
                (int)response.StatusCode, module, errorBody);
            response.EnsureSuccessStatusCode();
        }

        return await response.Content.ReadFromJsonAsync<AlmSubmitJobResponse>(JsonOptions, ct)
            ?? throw new InvalidOperationException($"ALM returned null response for {module} job");
    }

    public async Task<AlmJobStatusResponse> PollJobStatusAsync(string module, string jobId, CancellationToken ct = default)
    {
        var url = $"{module}/jobs/{jobId}";
        _logger.LogDebug("Polling ALM job status: {Url}", url);

        var response = await _httpClient.GetAsync(url, ct);
        response.EnsureSuccessStatusCode();

        return await response.Content.ReadFromJsonAsync<AlmJobStatusResponse>(JsonOptions, ct)
            ?? throw new InvalidOperationException($"ALM returned null status for job {jobId}");
    }
}
