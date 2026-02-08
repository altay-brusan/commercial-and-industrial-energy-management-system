using Backend.Application.TenantRoles;
using Backend.Application.TenantRoles.Commands.AssignPermissionsToRole;
using Backend.Application.TenantRoles.Commands.AssignRoleToUser;
using Backend.Application.TenantRoles.Commands.CreateTenantRole;
using Backend.Application.TenantRoles.Commands.DeleteTenantRole;
using Backend.Application.TenantRoles.Commands.RemoveRoleFromUser;
using Backend.Application.TenantRoles.Commands.UpdateTenantRole;
using Backend.Application.TenantRoles.Queries.GetTenantRole;
using Backend.Application.TenantRoles.Queries.GetTenantRoles;
using Microsoft.AspNetCore.Http.HttpResults;

namespace Backend.Web.Endpoints;

public class TenantRoles : EndpointGroupBase
{
    public override void Map(WebApplication app)
    {
        app.MapGroup(this)
            .RequireAuthorization()
            .MapGet(GetTenantRoles)
            .MapGet(GetTenantRole, "{id}")
            .MapPost(CreateTenantRole)
            .MapPut(UpdateTenantRole, "{id}")
            .MapDelete(DeleteTenantRole, "{id}")
            .MapPut(AssignPermissionsToRole, "{id}/permissions")
            .MapPost(AssignRoleToUser, "{id}/users")
            .MapDelete(RemoveRoleFromUser, "{id}/users/{userId}");
    }

    public async Task<Ok<List<TenantRoleBriefDto>>> GetTenantRoles(ISender sender)
    {
        var result = await sender.Send(new GetTenantRolesQuery());
        return TypedResults.Ok(result);
    }

    public async Task<Ok<TenantRoleDto>> GetTenantRole(ISender sender, int id)
    {
        var result = await sender.Send(new GetTenantRoleQuery(id));
        return TypedResults.Ok(result);
    }

    public async Task<Created<int>> CreateTenantRole(ISender sender, CreateTenantRoleCommand command)
    {
        var id = await sender.Send(command);
        return TypedResults.Created($"/{nameof(TenantRoles)}/{id}", id);
    }

    public async Task<Results<NoContent, BadRequest>> UpdateTenantRole(ISender sender, int id, UpdateTenantRoleCommand command)
    {
        if (id != command.Id) return TypedResults.BadRequest();
        await sender.Send(command);
        return TypedResults.NoContent();
    }

    public async Task<NoContent> DeleteTenantRole(ISender sender, int id)
    {
        await sender.Send(new DeleteTenantRoleCommand(id));
        return TypedResults.NoContent();
    }

    public async Task<NoContent> AssignPermissionsToRole(ISender sender, int id, AssignPermissionsToRoleCommand command)
    {
        await sender.Send(command with { TenantRoleId = id });
        return TypedResults.NoContent();
    }

    public async Task<NoContent> AssignRoleToUser(ISender sender, int id, AssignRoleToUserCommand command)
    {
        await sender.Send(command with { TenantRoleId = id });
        return TypedResults.NoContent();
    }

    public async Task<NoContent> RemoveRoleFromUser(ISender sender, int id, string userId)
    {
        await sender.Send(new RemoveRoleFromUserCommand { TenantRoleId = id, UserId = userId });
        return TypedResults.NoContent();
    }
}
