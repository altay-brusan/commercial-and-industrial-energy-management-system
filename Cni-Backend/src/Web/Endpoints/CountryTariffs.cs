using Backend.Application.Common.Models;
using Backend.Application.CountryTariffs.Commands.CreateCountryTariff;
using Backend.Application.CountryTariffs.Commands.UpdateCountryTariff;
using Backend.Application.CountryTariffs.Commands.DeleteCountryTariff;
using Backend.Application.CountryTariffs.Queries.GetCountryTariff;
using Backend.Application.CountryTariffs.Queries.GetCountryTariffs;
using Microsoft.AspNetCore.Http.HttpResults;

namespace Backend.Web.Endpoints;

public class CountryTariffs : EndpointGroupBase
{
    public override void Map(WebApplication app)
    {
        app.MapGroup(this)
            .RequireAuthorization()
            .MapGet(GetCountryTariff, "{id}")
            .MapGet(GetCountryTariffs)
            .MapPost(CreateCountryTariff)
            .MapPut(UpdateCountryTariff, "{id}")
            .MapDelete(DeleteCountryTariff, "{id}");
    }

    /// <summary>
    /// Gets a specific country tariff by ID.
    /// </summary>
    public async Task<Ok<CountryTariffDto>> GetCountryTariff(ISender sender, [AsParameters] GetCountryTariffQuery query)
    {
        var result = await sender.Send(query);
        return TypedResults.Ok(result);
    }

    /// <summary>
    /// Gets a paginated list of country tariffs.
    /// Supports filtering by IsActive and CountryCode.
    /// </summary>
    public async Task<Ok<PaginatedList<CountryTariffDto>>> GetCountryTariffs(ISender sender, [AsParameters] GetCountryTariffsQuery query)
    {
        var result = await sender.Send(query);
        return TypedResults.Ok(result);
    }

    /// <summary>
    /// Creates a new country tariff configuration.
    /// </summary>
    public async Task<Created<int>> CreateCountryTariff(ISender sender, CreateCountryTariffCommand command)
    {
        var id = await sender.Send(command);
        return TypedResults.Created($"/{nameof(CountryTariffs)}/{id}", id);
    }

    /// <summary>
    /// Updates an existing country tariff configuration.
    /// </summary>
    public async Task<Results<NoContent, BadRequest>> UpdateCountryTariff(ISender sender, int id, UpdateCountryTariffCommand command)
    {
        if (id != command.Id) return TypedResults.BadRequest();
        await sender.Send(command);
        return TypedResults.NoContent();
    }

    /// <summary>
    /// Deletes a country tariff configuration.
    /// </summary>
    public async Task<NoContent> DeleteCountryTariff(ISender sender, int id)
    {
        await sender.Send(new DeleteCountryTariffCommand(id));
        return TypedResults.NoContent();
    }
}
