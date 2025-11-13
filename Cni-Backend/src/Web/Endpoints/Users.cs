using Backend.Application.Common.Models;
using Backend.Application.Users.Queries.GetUser;
using Backend.Application.Users.Commands.UpdateUser;
using Backend.Application.Users.Queries.GetUserInfo;
using Backend.Application.Users.Queries.GetUsersWithPagination;
using Backend.Domain.Entities;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Backend.Application.Users.Commands.DeleteUser;
using Backend.Application.Users.Commands.AddUserClaim;
using Backend.Application.Users.Commands.RemoveUserClaim;
using Backend.Application.Users.Commands.RemoveUserRole;
using Backend.Application.Users.Queries.GetUserPermissions;

namespace Backend.Web.Endpoints;

public class Users : EndpointGroupBase
{
    public override void Map(WebApplication app)
    {
        // Grubu tanımla
        var group = app.MapGroup(this);

        // Identity API'yi ekle
        group.MapIdentityApiOklidis<ApplicationUser>();

        // Kullanıcı bilgisi endpoint'ini ekle
        group.MapGet("/userinfo", GetUserInfo).RequireAuthorization(); 
        // Kullanıcı güncelleme endpoint'i
        group.MapPut("/update", UpdateUser).RequireAuthorization();  
        group.MapPut("/updateprofile", UpdateUserProfile).RequireAuthorization();
        // Kullanıcıları getiren yeni endpoint
        group.MapGet("/", GetUsers);
        group.MapGet("/{id}",GetUser);
        group.MapDelete("/{id}", DeleteUser);
        // Kullanıcıya rol ekleme endpoint'i
        group.MapPost("/{id}/roles", AddUserRole);
        group.MapDelete("/{id}/roles", RemoveUserRole);

        // Kullanıcı claim yönetimi
        group.MapGet("/{id}/claims", GetUserClaims);
        group.MapPost("/{id}/claims", AddUserClaim);
        group.MapDelete("/{id}/claims", RemoveUserClaim);
    }

    public async Task<Ok<UserInfoDto>> GetUserInfo(ISender sender, [AsParameters] UserInfoQuery query)
    {
        var result = await sender.Send(query);
        return TypedResults.Ok(result);
    }
    public async Task<IResult> UpdateUser(ISender sender, [FromBody] UpdateUserCommand command)
    {
        await sender.Send(command);
        return TypedResults.Ok("User updated successfully.");
    } 
    public async Task<IResult> UpdateUserProfile(ISender sender, [FromBody] UpdateUserProfileCommand command)
    {
        await sender.Send(command);
        return TypedResults.Ok("User updated successfully.");
    }
    public async Task<Ok<PaginatedList<UserBriefDto>>> GetUsers(ISender sender, [AsParameters] GetUsersWithPaginationQuery query)
    {
        var result = await sender.Send(query);
        return TypedResults.Ok(result);
    }
    public async Task<Ok<UserDto>> GetUser(ISender sender, [AsParameters] GetUserQuery query)
    {
        var result = await sender.Send(query);

        return TypedResults.Ok(result);
    }
    public async Task<NoContent> DeleteUser(ISender sender, string id)
    {
        await sender.Send(new DeleteUserCommand(id));

        return TypedResults.NoContent();
    }
    public async Task<IResult> AddUserRole(ISender sender, string id, [FromBody] AddUserRoleCommand command)
    {
        command.UserId = id;
        await sender.Send(command);
        return TypedResults.Ok("User role added successfully.");
    }
    public async Task<IResult> RemoveUserRole(ISender sender, string id, [FromBody] RemoveUserRoleCommand command)
    {
        command.UserId = id;
        await sender.Send(command);
        return TypedResults.Ok("User role removed successfully.");
    }

    public async Task<Ok<List<string>>> GetUserClaims(ISender sender, string id)
    {
        var result = await sender.Send(new GetUserPermissionsQuery { UserId = id });
        return TypedResults.Ok(result);
    }

    public async Task<IResult> AddUserClaim(ISender sender, string id, [FromBody] AddUserClaimCommand command)
    {
        command.UserId = id;
        await sender.Send(command);
        return TypedResults.Ok("User claim added successfully.");
    }

    public async Task<IResult> RemoveUserClaim(ISender sender, string id, [FromBody] RemoveUserClaimCommand command)
    {
        command.UserId = id;
        await sender.Send(command);
        return TypedResults.Ok("User claim removed successfully.");
    }
}
