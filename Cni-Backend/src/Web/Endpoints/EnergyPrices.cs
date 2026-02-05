using Backend.Application.EnergyPrices.Queries.GetEnergyPricesWithRange;
using Backend.Application.EnergyPrices.Queries.GetEnergyPriceStatistics;
using Backend.Application.EnergyPrices.Queries.GetHistoricalEnergyPrices;
using Backend.Application.EnergyPrices.Queries.GetLiveEnergyPrices;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Web.Endpoints;

/// <summary>
/// Energy Prices API endpoints organized into Live and History categories.
///
/// Live endpoints: Fetch real-time prices from Market Operator APIs
/// History endpoints: Query stored historical price data from the database
/// </summary>
public class EnergyPrices : EndpointGroupBase
{
    public override void Map(WebApplication app)
    {
        var group = app.MapGroup(this)
            .RequireAuthorization();

        // Live endpoints - real-time data from Market Operator
        group.MapGet(GetLiveEnergyPrices, "Live");

        // History endpoints - stored data from database
        group.MapGet(GetHistoricalEnergyPrices, "History");
        group.MapGet(GetHistoricalEnergyPricesForTenant, "History/Tenant");
        group.MapGet(GetEnergyPriceStatistics, "History/Statistics");
    }

    /// <summary>
    /// Fetches live day-ahead prices directly from a firm's configured Market Operator API.
    /// Returns current/tomorrow's prices with tax and distribution fees applied based on firm's tariff.
    /// </summary>
    /// <param name="sender">MediatR sender</param>
    /// <param name="firmId">Firm ID to fetch prices for.</param>
    /// <param name="date">Optional date to fetch prices for</param>
    /// <param name="period">Resolution in minutes (15, 30, or 60). Defaults to 60 (hourly).</param>
    public async Task<Ok<LiveEnergyPricesDto>> GetLiveEnergyPrices(
        ISender sender,
        [FromQuery] int firmId,
        [FromQuery] DateTimeOffset? date = null,
        [FromQuery] int? period = null)
    {
        var query = new GetLiveEnergyPricesQuery
        {
            FirmId = firmId,
            Date = date,
            Period = period
        };
        var result = await sender.Send(query);
        return TypedResults.Ok(result);
    }

    /// <summary>
    /// Gets historical energy prices by country and date range from the database.
    /// This endpoint queries stored price history directly by country name.
    /// Returns null if no data is found for the specified criteria.
    /// </summary>
    /// <param name="sender">MediatR sender</param>
    /// <param name="country">Country name (e.g., "Netherlands", "Germany", "Turkey")</param>
    /// <param name="startDate">Start date of the range (inclusive)</param>
    /// <param name="endDate">End date of the range (inclusive). If not provided, only startDate's data is returned.</param>
    public async Task<Results<Ok<HistoricalEnergyPricesDto>, NotFound>> GetHistoricalEnergyPrices(
        ISender sender,
        [FromQuery] string country,
        [FromQuery] DateTimeOffset startDate,
        [FromQuery] DateTimeOffset? endDate = null)
    {
        var query = new GetHistoricalEnergyPricesQuery
        {
            Country = country,
            StartDate = startDate,
            EndDate = endDate
        };
        var result = await sender.Send(query);

        if (result == null)
        {
            return TypedResults.NotFound();
        }

        return TypedResults.Ok(result);
    }

    /// <summary>
    /// Gets historical energy prices for a specific tenant's configuration.
    /// Uses the tenant's area code to determine the country and queries historical data.
    /// Returns a time series of prices with timestamps.
    /// </summary>
    public async Task<Ok<EnergyPriceRangeDto>> GetHistoricalEnergyPricesForTenant(ISender sender, [AsParameters] GetEnergyPricesWithRangeQuery query)
    {
        var result = await sender.Send(query);
        return TypedResults.Ok(result);
    }

    /// <summary>
    /// Gets statistical values (min, max, average) for historical energy prices within a date range.
    /// Uses the tenant's configured Market Operator and area code.
    /// </summary>
    public async Task<Ok<EnergyPriceStatisticsDto>> GetEnergyPriceStatistics(ISender sender, [AsParameters] GetEnergyPriceStatisticsQuery query)
    {
        var result = await sender.Send(query);
        return TypedResults.Ok(result);
    }
}
