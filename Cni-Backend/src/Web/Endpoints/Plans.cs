using Backend.Application.Common.Models;
using Backend.Application.Plans.Commands.CreatePlan;
using Backend.Application.Plans.Commands.DeletePlan;
using Backend.Application.Plans.Commands.UpdatePlan;
using Backend.Application.Plans.Queries.GetPlan;
using Backend.Application.Plans.Queries.GetPlansWithPagination;
using Microsoft.AspNetCore.Http.HttpResults;

namespace Backend.Web.Endpoints;

public class Plans : EndpointGroupBase
{
    public override void Map(WebApplication app)
    {
        app.MapGroup(this)
            .RequireAuthorization()
            .MapGet(GetPlan,"{id}")
            .MapGet(GetPlansWithPagination)
            .MapPost(CreatePlan)
            .MapPut(UpdatePlan, "{id}")
            .MapDelete(DeletePlan, "{id}");
    }
    public async Task<Ok<PlanDto>> GetPlan(ISender sender, [AsParameters] GetPlanQuery query)
    {
        var result = await sender.Send(query);

        return TypedResults.Ok(result);
    }
    public async Task<Ok<PaginatedList<PlanBriefDto>>> GetPlansWithPagination(ISender sender, [AsParameters] GetPlansWithPaginationQuery query)
    {
        var result = await sender.Send(query);

        return TypedResults.Ok(result);
    }

    public async Task<Created<int>> CreatePlan(ISender sender, CreatePlanCommand command)
    {
        var id = await sender.Send(command);

        return TypedResults.Created($"/{nameof(Plans)}/{id}", id);
    }

    public async Task<Results<NoContent, BadRequest>> UpdatePlan(ISender sender, int id, UpdatePlanCommand command)
    {
        if (id != command.Id) return TypedResults.BadRequest();

        await sender.Send(command);

        return TypedResults.NoContent();
    }

    public async Task<NoContent> DeletePlan(ISender sender, int id)
    {
        await sender.Send(new DeletePlanCommand(id));

        return TypedResults.NoContent();
    }
}
