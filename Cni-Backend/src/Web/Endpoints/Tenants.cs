using Backend.Application.Common.Models;
using Backend.Application.Tenants.Commands.CreateTenant;
using Backend.Application.Tenants.Commands.DeleteTenant;
using Backend.Application.Tenants.Commands.UpdateTenant;
using Backend.Application.Tenants.Queries.GetTenant;
using Backend.Application.Tenants.Queries.GetTenantsWithPagination;
using Microsoft.AspNetCore.Http.HttpResults;

namespace Backend.Web.Endpoints;

public class Tenants : EndpointGroupBase
{
    public override void Map(WebApplication app)
    {
        app.MapGroup(this)
            .RequireAuthorization()
            .MapGet(GetTenant,"{id}")
            .MapGet(GetTenantsWithPagination)
            .MapPost(CreateTenant)
            .MapPut(UpdateTenant, "{id}")
            .MapDelete(DeleteTenant, "{id}");
    }
    public async Task<Ok<TenantDto>> GetTenant(ISender sender, [AsParameters] GetTenantQuery query)
    {
        var result = await sender.Send(query);

        return TypedResults.Ok(result);
    }
    public async Task<Ok<PaginatedList<TenantBriefDto>>> GetTenantsWithPagination(ISender sender, [AsParameters] GetTenantsWithPaginationQuery query)
    {
        var result = await sender.Send(query);

        return TypedResults.Ok(result);
    }

    public async Task<Created<int>> CreateTenant(ISender sender, CreateTenantCommand command)
    {
        var id = await sender.Send(command);

        return TypedResults.Created($"/{nameof(Tenants)}/{id}", id);
    }

    public async Task<Results<NoContent, BadRequest>> UpdateTenant(ISender sender, int id, UpdateTenantCommand command)
    {
        if (id != command.Id) return TypedResults.BadRequest();

        await sender.Send(command);

        return TypedResults.NoContent();
    }

    public async Task<NoContent> DeleteTenant(ISender sender, int id)
    {
        await sender.Send(new DeleteTenantCommand(id));

        return TypedResults.NoContent();
    }
}
