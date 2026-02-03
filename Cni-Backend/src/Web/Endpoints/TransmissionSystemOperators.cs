using Backend.Application.Common.Models;
using Backend.Application.TransmissionSystemOperators.Commands.CreateTransmissionSystemOperator;
using Backend.Application.TransmissionSystemOperators.Commands.UpdateTransmissionSystemOperator;
using Backend.Application.TransmissionSystemOperators.Commands.DeleteTransmissionSystemOperator;
using Backend.Application.TransmissionSystemOperators.Queries.GetTransmissionSystemOperator;
using Backend.Application.TransmissionSystemOperators.Queries.GetTransmissionSystemOperatorsWithPagination;
using Microsoft.AspNetCore.Http.HttpResults;

namespace Backend.Web.Endpoints;

public class TransmissionSystemOperators : EndpointGroupBase
{
    public override void Map(WebApplication app)
    {
        app.MapGroup(this)
            .RequireAuthorization()
            .MapGet(GetTransmissionSystemOperator, "{id}")
            .MapGet(GetTransmissionSystemOperatorsWithPagination)
            .MapPost(CreateTransmissionSystemOperator)
            .MapPut(UpdateTransmissionSystemOperator, "{id}")
            .MapDelete(DeleteTransmissionSystemOperator, "{id}");
    }

    public async Task<Ok<TransmissionSystemOperatorDto>> GetTransmissionSystemOperator(ISender sender, [AsParameters] GetTransmissionSystemOperatorQuery query)
    {
        var result = await sender.Send(query);
        return TypedResults.Ok(result);
    }

    public async Task<Ok<PaginatedList<TransmissionSystemOperatorBriefDto>>> GetTransmissionSystemOperatorsWithPagination(ISender sender, [AsParameters] GetTransmissionSystemOperatorsWithPaginationQuery query)
    {
        var result = await sender.Send(query);
        return TypedResults.Ok(result);
    }

    public async Task<Created<int>> CreateTransmissionSystemOperator(ISender sender, CreateTransmissionSystemOperatorCommand command)
    {
        var id = await sender.Send(command);
        return TypedResults.Created($"/{nameof(TransmissionSystemOperators)}/{id}", id);
    }

    public async Task<Results<NoContent, BadRequest>> UpdateTransmissionSystemOperator(ISender sender, int id, UpdateTransmissionSystemOperatorCommand command)
    {
        if (id != command.Id) return TypedResults.BadRequest();
        await sender.Send(command);
        return TypedResults.NoContent();
    }

    public async Task<NoContent> DeleteTransmissionSystemOperator(ISender sender, int id)
    {
        await sender.Send(new DeleteTransmissionSystemOperatorCommand(id));
        return TypedResults.NoContent();
    }
}
