using Backend.Application.AppRoles.Commands.AddRoleClaim;
using Backend.Application.AppRoles.Commands.RemoveRoleClaim;
using Backend.Application.AppRoles.Queries.GetAppRolesWithPagination;
using Backend.Application.AppRoles.Queries.GetRoleClaimsWithPagination;
using Backend.Application.Common.Models;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Web.Endpoints;

public class AppRoles : EndpointGroupBase
{
    public override void Map(WebApplication app)
    {
        var group = app.MapGroup(this)
        .RequireAuthorization();  // Grup bazında authorization ayarlıyoruz.

        // /api/AppRoles yolunu yalnızca bir kez tanımlayalım
        group.MapGet("/roles", GetAppRolesWithPagination);

        // Role claim ekleme
        group.MapPost("/roles/{roleName}/claims", AddRoleClaim);

        // Role ait claim’leri getirme
        group.MapGet("/roles/{roleName}/claims", GetRoleClaims);

        // Role ait claim’i silme
        group.MapDelete("/roles/{roleName}/claims", RemoveRoleClaim);
    }
    
    public async Task<Ok<PaginatedList<AppRoleBriefDto>>> GetAppRolesWithPagination(ISender sender, [AsParameters] GetAppRolesWithPaginationQuery query)
    {
        var result = await sender.Send(query);

        return TypedResults.Ok(result);
    }
    public async Task<Results<Ok<bool>, NotFound>> AddRoleClaim(
        ISender sender, AddRoleClaimCommand command)
    {
        var result = await sender.Send(command);
        return result ? TypedResults.Ok(result) : TypedResults.NotFound();
    }

    public async Task<Results<Ok<List<RoleClaimDto>>, NotFound>> GetRoleClaims(
        ISender sender, [FromQuery] string roleName)
    {
        var result = await sender.Send(new GetRoleClaimsQuery { RoleName = roleName });
        return result.Any() ? TypedResults.Ok(result) : TypedResults.NotFound();
    }

    public async Task<Results<Ok<bool>, NotFound>> RemoveRoleClaim(
    ISender sender, [FromQuery] string roleName, [FromQuery] string claimType, [FromQuery] string claimValue)
    {
        var result = await sender.Send(new RemoveRoleClaimCommand
        {
            RoleName = roleName,
            ClaimType = claimType,
            ClaimValue = claimValue
        });

        return result ? TypedResults.Ok(result) : TypedResults.NotFound();
    }
}
