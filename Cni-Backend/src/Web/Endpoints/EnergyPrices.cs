using Backend.Application.EnergyPrices.Queries.GetEnergyPricesWithRange;
using Backend.Application.EnergyPrices.Queries.GetEnergyPriceStatistics;
using Backend.Application.EnergyPrices.Queries.GetLiveTsoPrices;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Web.Endpoints;

public class EnergyPrices : EndpointGroupBase
{
    public override void Map(WebApplication app)
    {
        app.MapGroup(this)
            .RequireAuthorization()
            .MapGet(GetLiveEnergyPrices)
            .MapGet(GetEnergyPricesWithRange, "range")
            .MapGet(GetEnergyPriceStatistics, "statistics");
    }

    /// <summary>
    /// Fetches live day-ahead prices directly from the tenant's configured Market Operator API.
    /// Returns current/tomorrow's prices with tax and distribution fees applied.
    /// If no Market Operator is configured for the tenant, falls back to default based on Firm's country.
    /// </summary>
    /// <param name="sender">MediatR sender</param>
    /// <param name="date">Optional date to fetch prices for</param>
    /// <param name="period">Resolution in minutes (15, 30, or 60). Defaults to 60 (hourly).</param>
    public async Task<Ok<LiveEnergyPricesDto>> GetLiveEnergyPrices(
        ISender sender,
        [FromQuery] DateTimeOffset? date = null,
        [FromQuery] int? period = null)
    {
        var query = new GetLiveEnergyPricesQuery
        {
            Date = date,
            Period = period
        };
        var result = await sender.Send(query);
        return TypedResults.Ok(result);
    }

    /// <summary>
    /// Gets energy prices for a date range from the tenant's configured Market Operator.
    /// Returns a time series of prices with timestamps.
    /// </summary>
    public async Task<Ok<EnergyPriceRangeDto>> GetEnergyPricesWithRange(ISender sender, [AsParameters] GetEnergyPricesWithRangeQuery query)
    {
        var result = await sender.Send(query);
        return TypedResults.Ok(result);
    }

    /// <summary>
    /// Gets statistical values (min, max, average) for energy prices within a date range.
    /// Uses the tenant's configured Market Operator.
    /// </summary>
    public async Task<Ok<EnergyPriceStatisticsDto>> GetEnergyPriceStatistics(ISender sender, [AsParameters] GetEnergyPriceStatisticsQuery query)
    {
        var result = await sender.Send(query);
        return TypedResults.Ok(result);
    }
}
