using Backend.Application.FirmMarketOperators.Commands.CreateFirmMarketOperator;
using Backend.Application.FirmMarketOperators.Commands.UpdateFirmMarketOperator;
using Backend.Application.FirmMarketOperators.Commands.DeleteFirmMarketOperator;
using Backend.Application.FirmMarketOperators.Queries.GetFirmMarketOperator;
using Microsoft.AspNetCore.Http.HttpResults;

namespace Backend.Web.Endpoints;

public class FirmMarketOperators : EndpointGroupBase
{
    public override void Map(WebApplication app)
    {
        app.MapGroup(this)
            .RequireAuthorization()
            .MapGet(GetFirmMarketOperator, "Firms/{firmId}/MarketOperator")
            .MapPost(CreateFirmMarketOperator, "Firms/{firmId}/MarketOperator")
            .MapPut(UpdateFirmMarketOperator, "Firms/{firmId}/MarketOperator/{id}")
            .MapDelete(DeleteFirmMarketOperator, "Firms/{firmId}/MarketOperator/{id}");
    }

    /// <summary>
    /// Gets the Market Operator configuration for a firm.
    /// </summary>
    public async Task<Ok<FirmMarketOperatorDto>> GetFirmMarketOperator(ISender sender, Guid firmId)
    {
        var query = new GetFirmMarketOperatorQuery { FirmId = firmId };
        var result = await sender.Send(query);
        return TypedResults.Ok(result);
    }

    /// <summary>
    /// Assigns a Market Operator to a firm.
    /// </summary>
    public async Task<Created<Guid>> CreateFirmMarketOperator(ISender sender, Guid firmId, CreateFirmMarketOperatorCommand command)
    {
        command.FirmId = firmId;
        var id = await sender.Send(command);
        return TypedResults.Created($"/Firms/{firmId}/MarketOperator/{id}", id);
    }

    /// <summary>
    /// Updates the Market Operator configuration for a firm.
    /// </summary>
    public async Task<Results<NoContent, BadRequest>> UpdateFirmMarketOperator(ISender sender, Guid firmId, Guid id, UpdateFirmMarketOperatorCommand command)
    {
        if (id != command.Id) return TypedResults.BadRequest();
        await sender.Send(command);
        return TypedResults.NoContent();
    }

    /// <summary>
    /// Removes the Market Operator assignment from a firm.
    /// </summary>
    public async Task<NoContent> DeleteFirmMarketOperator(ISender sender, Guid firmId, Guid id)
    {
        await sender.Send(new DeleteFirmMarketOperatorCommand(id));
        return TypedResults.NoContent();
    }
}
