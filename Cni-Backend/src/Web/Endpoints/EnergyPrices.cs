using Backend.Application.EnergyPrices.Commands.CreateEnergyPriceHistory;
using Backend.Application.EnergyPrices.Queries.GetEnergyPrice;
using Backend.Application.EnergyPrices.Queries.GetEnergyPricesWithRange;
using Backend.Application.EnergyPrices.Queries.GetEnergyPriceStatistics;
using Backend.Application.EnergyPrices.Queries.GetLiveTsoPrices;
using Microsoft.AspNetCore.Http.HttpResults;

namespace Backend.Web.Endpoints;

public class EnergyPrices : EndpointGroupBase
{
    public override void Map(WebApplication app)
    {
        app.MapGroup(this)
            .RequireAuthorization()
            .MapGet(GetLiveTsoPrices, "live/{tsoId}")
            .MapGet(GetEnergyPrice, "price")
            .MapGet(GetEnergyPricesWithRange, "range")
            .MapGet(GetEnergyPriceStatistics, "statistics")
            .MapPost(CreateEnergyPriceHistory);
    }

    /// <summary>
    /// Fetches live day-ahead prices directly from a TSO API.
    /// Returns current/tomorrow's prices with tax and distribution fees applied.
    /// </summary>
    public async Task<Ok<LiveTsoPricesDto>> GetLiveTsoPrices(ISender sender, int tsoId, DateTimeOffset? date = null)
    {
        var query = new GetLiveTsoPricesQuery { TsoId = tsoId, Date = date };
        var result = await sender.Send(query);
        return TypedResults.Ok(result);
    }

    /// <summary>
    /// Gets energy price for a specific provider and timestamp.
    /// If exact timestamp doesn't exist, returns the nearest available price.
    /// </summary>
    public async Task<Ok<EnergyPriceDto>> GetEnergyPrice(ISender sender, [AsParameters] GetEnergyPriceQuery query)
    {
        var result = await sender.Send(query);
        return TypedResults.Ok(result);
    }

    /// <summary>
    /// Gets energy prices for a date range.
    /// Returns a time series of prices with timestamps.
    /// </summary>
    public async Task<Ok<EnergyPriceRangeDto>> GetEnergyPricesWithRange(ISender sender, [AsParameters] GetEnergyPricesWithRangeQuery query)
    {
        var result = await sender.Send(query);
        return TypedResults.Ok(result);
    }

    /// <summary>
    /// Gets statistical values (min, max, average) for energy prices within a date range.
    /// </summary>
    public async Task<Ok<EnergyPriceStatisticsDto>> GetEnergyPriceStatistics(ISender sender, [AsParameters] GetEnergyPriceStatisticsQuery query)
    {
        var result = await sender.Send(query);
        return TypedResults.Ok(result);
    }

    /// <summary>
    /// Creates a new energy price history record.
    /// </summary>
    public async Task<Created<int>> CreateEnergyPriceHistory(ISender sender, CreateEnergyPriceHistoryCommand command)
    {
        var id = await sender.Send(command);
        return TypedResults.Created($"/{nameof(EnergyPrices)}/{id}", id);
    }
}
