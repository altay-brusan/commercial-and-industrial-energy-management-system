using Backend.Application.Common.Models;
using Backend.Application.Facilities.Queries.GetFacilitiesWithPagination;
using Microsoft.AspNetCore.Http.HttpResults;
using Backend.Application.Facilities.Commands.CreateFacility;
using Backend.Application.Facilities.Commands.UpdateFacility;
using Backend.Application.Facilities.Commands.DeleteFacility;
using Backend.Application.Facilities.Queries.GetFacility;

namespace Backend.Web.Endpoints;

public class Facilities : EndpointGroupBase
{
    public override void Map(WebApplication app)
    {
        app.MapGroup(this)
            .RequireAuthorization()
            .MapGet(GetFacility,"{id}")
            .MapGet(GetFacilitiesWithPagination)
            .MapPost(CreateFacility)
            .MapPut(UpdateFacility, "{id}")
            .MapDelete(DeleteFacility, "{id}");
    }
    public async Task<Ok<FacilityDto>> GetFacility(ISender sender, [AsParameters] GetFacilityQuery query)
    {
        var result = await sender.Send(query);

        return TypedResults.Ok(result);
    }
    public async Task<Ok<PaginatedList<FacilityBriefDto>>> GetFacilitiesWithPagination(ISender sender, [AsParameters] GetFacilitiesWithPaginationQuery query)
    {
        var result = await sender.Send(query);

        return TypedResults.Ok(result);
    }

    public async Task<Created<Guid>> CreateFacility(ISender sender, CreateFacilityCommand command)
    {
        var id = await sender.Send(command);

        return TypedResults.Created($"/{nameof(Facilities)}/{id}", id);
    }

    public async Task<Results<NoContent, BadRequest>> UpdateFacility(ISender sender, Guid id, UpdateFacilityCommand command)
    {
        if (id != command.Id) return TypedResults.BadRequest();

        await sender.Send(command);

        return TypedResults.NoContent();
    }

    public async Task<NoContent> DeleteFacility(ISender sender, Guid id)
    {
        await sender.Send(new DeleteFacilityCommand(id));

        return TypedResults.NoContent();
    }
}
