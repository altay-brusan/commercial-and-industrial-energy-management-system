using Backend.Application.Common.Interfaces;
using Backend.Application.Common.Models;
using Backend.Application.Common.Security;
using Backend.Application.Users.Queries.GetUserInfo;
using Backend.Domain.Entities;
using Microsoft.AspNetCore.Identity;

[Authorize(Roles = "Administrator,Tenant.Admin")]
[RequirePermission(Backend.Domain.Constants.Permissions.UserManagementEdit)]
public record AddUserRoleCommand : IRequest<Unit>
{
    public string UserId { get; set; } = default!;
    public string RoleName { get; set; } = default!;
}

public class AddUserRoleCommandHandler : IRequestHandler<AddUserRoleCommand, Unit>
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly RoleManager<IdentityRole> _roleManager;
    private readonly bool _isSuperAdmin;
    public AddUserRoleCommandHandler(UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager, IMultiTenantService multiTenantService)
    {
        _userManager = userManager;
        _roleManager = roleManager;
        _isSuperAdmin = multiTenantService.IsSuperAdmin();
    }

    public async Task<Unit> Handle(AddUserRoleCommand request, CancellationToken cancellationToken)
    {
        if (request.RoleName == "Administrator" && !_isSuperAdmin)
        {
            throw new Exception("Only Super Admin can assign Administrator role.");
        }

        var user = await _userManager.FindByIdAsync(request.UserId);
        Guard.Against.NotFound(request.UserId, user);


        var roleExists = await _userManager.IsInRoleAsync(user, request.RoleName);
        if (roleExists)
        {
            throw new Exception($"User is already in role {request.RoleName}");
        }

        var result = await _userManager.AddToRoleAsync(user, request.RoleName);
        if (!result.Succeeded)
        {
            throw new Exception(string.Join(", ", result.Errors.Select(e => e.Description)));
        }

        return Unit.Value;
    }
}

