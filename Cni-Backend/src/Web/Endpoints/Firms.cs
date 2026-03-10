using Backend.Application.Common.Models;
using Backend.Application.Firms.Commands.CreateFirm;
using Backend.Application.Firms.Commands.DeleteFirm;
using Backend.Application.Firms.Commands.UpdateFirm;
using Backend.Application.Firms.Queries.GetFirm;
using Backend.Application.Firms.Queries.GetFirmsWithPagination;
using Backend.Application.Firms.Queries.GetUserFirmFacilities;
using Backend.Application.Firms.Queries.GetUserFirms;
using Microsoft.AspNetCore.Http.HttpResults;

namespace Backend.Web.Endpoints;

public class Firms : EndpointGroupBase
{
    public override void Map(WebApplication app)
    {
        app.MapGroup(this)
            .RequireAuthorization()
            .MapGet(GetUserFirms, "Me")
            .MapGet(GetUserFirmFacilities, "{firmId}/Facilities")
            .MapGet(GetFirm,"{id}")
            .MapGet(GetFirmsWithPagination)
            .MapPost(CreateFirm)
            .MapPut(UpdateFirm, "{id}")
            .MapDelete(DeleteFirm, "{id}");
    }
    public async Task<Ok<List<UserFirmDto>>> GetUserFirms(ISender sender)
    {
        var result = await sender.Send(new GetUserFirmsQuery());

        return TypedResults.Ok(result);
    }

    public async Task<Ok<List<UserFirmFacilityDto>>> GetUserFirmFacilities(ISender sender, Guid firmId)
    {
        var result = await sender.Send(new GetUserFirmFacilitiesQuery { FirmId = firmId });

        return TypedResults.Ok(result);
    }

    public async Task<Ok<FirmDto>> GetFirm(ISender sender, [AsParameters] GetFirmQuery query)
    {
        var result = await sender.Send(query);

        return TypedResults.Ok(result);
    }
    public async Task<Ok<PaginatedList<FirmBriefDto>>> GetFirmsWithPagination(ISender sender, [AsParameters] GetFirmsWithPaginationQuery query)
    {
        var result = await sender.Send(query);

        return TypedResults.Ok(result);
    }

    public async Task<Created<Guid>> CreateFirm(ISender sender, CreateFirmCommand command)
    {
        var id = await sender.Send(command);

        return TypedResults.Created($"/{nameof(Firms)}/{id}", id);
    }

    public async Task<Results<NoContent, BadRequest>> UpdateFirm(ISender sender, Guid id, UpdateFirmCommand command)
    {
        if (id != command.Id) return TypedResults.BadRequest();

        await sender.Send(command);

        return TypedResults.NoContent();
    }

    public async Task<NoContent> DeleteFirm(ISender sender, Guid id)
    {
        await sender.Send(new DeleteFirmCommand(id));

        return TypedResults.NoContent();
    }
}
