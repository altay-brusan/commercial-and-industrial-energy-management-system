using Backend.Application.Common.Interfaces;
using Backend.Application.Common.Models.Huawei;
using Backend.Infrastructure.BackgroundJobs;
using Backend.Infrastructure.Huawei;
using Backend.Web.Infrastructure;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Backend.Web.Endpoints;

public class Huawei : EndpointGroupBase
{
    public override void Map(WebApplication app)
    {
        var group = app.MapGroup("/api/v1/Huawei")
            .WithGroupName("Huawei")
            .WithTags("Huawei")
            .RequireAuthorization();

        group.MapPost("/TestConnection", TestConnection)
            .WithSummary("Test Huawei FusionSolar API connection with provided credentials");

        group.MapPost("/TestConnection/Stations", TestGetStations)
            .WithSummary("Login and list all stations from a Huawei account");

        group.MapPost("/TestConnection/Devices", TestGetDevices)
            .WithSummary("Login and list all devices for a station");

        group.MapPost("/TestConnection/RealTimeKpi", TestGetRealTimeKpi)
            .WithSummary("Login and get real-time KPI data for specific devices");
    }

    public static async Task<Ok<HuaweiTestConnectionResult>> TestConnection(
        [FromServices] IHttpClientFactory httpClientFactory,
        [FromBody] HuaweiTestConnectionRequest request)
    {
        var client = CreateApiClient(httpClientFactory);
        try
        {
            var token = await client.LoginAsync(request.BaseUrl, request.UserName, request.SystemCode);
            var stations = await client.GetStationsAsync(request.BaseUrl, token);

            return TypedResults.Ok(new HuaweiTestConnectionResult
            {
                Success = true,
                Message = $"Connected successfully. Found {stations.Total} station(s).",
                Token = token,
                StationCount = stations.Total
            });
        }
        catch (Exception ex)
        {
            return TypedResults.Ok(new HuaweiTestConnectionResult
            {
                Success = false,
                Message = $"Connection failed: {ex.Message}"
            });
        }
    }

    public static async Task<Ok<HuaweiStationListResult>> TestGetStations(
        [FromServices] IHttpClientFactory httpClientFactory,
        [FromBody] HuaweiTestConnectionRequest request)
    {
        var client = CreateApiClient(httpClientFactory);
        var token = await client.LoginAsync(request.BaseUrl, request.UserName, request.SystemCode);
        var stations = await client.GetStationsAsync(request.BaseUrl, token);

        return TypedResults.Ok(new HuaweiStationListResult
        {
            Total = stations.Total,
            Stations = stations.List
        });
    }

    public static async Task<Ok<List<HuaweiDeviceInfo>>> TestGetDevices(
        [FromServices] IHttpClientFactory httpClientFactory,
        [FromBody] HuaweiTestGetDevicesRequest request)
    {
        var client = CreateApiClient(httpClientFactory);
        var token = await client.LoginAsync(request.BaseUrl, request.UserName, request.SystemCode);
        var devices = await client.GetDeviceListAsync(request.BaseUrl, token, request.StationCode);

        return TypedResults.Ok(devices);
    }

    public static async Task<Ok<List<HuaweiDevRealKpiData>>> TestGetRealTimeKpi(
        [FromServices] IHttpClientFactory httpClientFactory,
        [FromBody] HuaweiTestGetRealTimeKpiRequest request)
    {
        var client = CreateApiClient(httpClientFactory);
        var token = await client.LoginAsync(request.BaseUrl, request.UserName, request.SystemCode);
        var kpiData = await client.GetDevRealKpiAsync(request.BaseUrl, token, request.DevIds, request.DevTypeId);

        return TypedResults.Ok(kpiData);
    }

    private static HuaweiApiClient CreateApiClient(IHttpClientFactory httpClientFactory)
    {
        var httpClient = httpClientFactory.CreateClient("HuaweiTest");
        return new HuaweiApiClient(httpClient, Microsoft.Extensions.Logging.Abstractions.NullLogger<HuaweiApiClient>.Instance);
    }
}

// Request/Response DTOs

public record HuaweiTestConnectionRequest
{
    public string BaseUrl { get; init; } = "https://intl.fusionsolar.huawei.com/thirdData";
    public string UserName { get; init; } = string.Empty;
    public string SystemCode { get; init; } = string.Empty;
}

public record HuaweiTestGetDevicesRequest : HuaweiTestConnectionRequest
{
    public string StationCode { get; init; } = string.Empty;
}

public record HuaweiTestGetRealTimeKpiRequest : HuaweiTestConnectionRequest
{
    public List<long> DevIds { get; init; } = [];
    public int DevTypeId { get; init; }
}

public record HuaweiTestConnectionResult
{
    public bool Success { get; init; }
    public string Message { get; init; } = string.Empty;
    public string? Token { get; init; }
    public int StationCount { get; init; }
}

public record HuaweiStationListResult
{
    public int Total { get; init; }
    public List<HuaweiStationInfo> Stations { get; init; } = [];
}
