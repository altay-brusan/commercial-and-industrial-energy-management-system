using Backend.Application.Common.Models;
using Backend.Application.MarketOperators.Commands.CreateMarketOperator;
using Backend.Application.MarketOperators.Commands.UpdateMarketOperator;
using Backend.Application.MarketOperators.Commands.DeleteMarketOperator;
using Backend.Application.MarketOperators.Queries.GetMarketOperator;
using Backend.Application.MarketOperators.Queries.GetMarketOperatorsWithPagination;
using Microsoft.AspNetCore.Http.HttpResults;

namespace Backend.Web.Endpoints;

public class MarketOperators : EndpointGroupBase
{
    public override void Map(WebApplication app)
    {
        app.MapGroup(this)
            .RequireAuthorization()
            .MapGet(GetMarketOperator, "{id}")
            .MapGet(GetMarketOperatorsWithPagination)
            .MapPost(CreateMarketOperator)
            .MapPut(UpdateMarketOperator, "{id}")
            .MapDelete(DeleteMarketOperator, "{id}");
    }

    /// <summary>
    /// Gets a Market Operator by ID.
    /// </summary>
    public async Task<Ok<MarketOperatorDto>> GetMarketOperator(ISender sender, [AsParameters] GetMarketOperatorQuery query)
    {
        var result = await sender.Send(query);
        return TypedResults.Ok(result);
    }

    /// <summary>
    /// Gets a paginated list of Market Operators.
    /// </summary>
    public async Task<Ok<PaginatedList<MarketOperatorBriefDto>>> GetMarketOperatorsWithPagination(ISender sender, [AsParameters] GetMarketOperatorsWithPaginationQuery query)
    {
        var result = await sender.Send(query);
        return TypedResults.Ok(result);
    }

    /// <summary>
    /// Creates a new Market Operator. Admin only.
    /// </summary>
    public async Task<Created<int>> CreateMarketOperator(ISender sender, CreateMarketOperatorCommand command)
    {
        var id = await sender.Send(command);
        return TypedResults.Created($"/{nameof(MarketOperators)}/{id}", id);
    }

    /// <summary>
    /// Updates an existing Market Operator. Admin only.
    /// </summary>
    public async Task<Results<NoContent, BadRequest>> UpdateMarketOperator(ISender sender, int id, UpdateMarketOperatorCommand command)
    {
        if (id != command.Id) return TypedResults.BadRequest();
        await sender.Send(command);
        return TypedResults.NoContent();
    }

    /// <summary>
    /// Deletes a Market Operator. Admin only.
    /// </summary>
    public async Task<NoContent> DeleteMarketOperator(ISender sender, int id)
    {
        await sender.Send(new DeleteMarketOperatorCommand(id));
        return TypedResults.NoContent();
    }
}
