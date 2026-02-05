using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Backend.Application.Common.Security;
using Microsoft.AspNetCore.Identity;

namespace Backend.Application.AppRoles.Commands.AddRoleClaim;
[Authorize(Roles = "Administrator")]
public record AddRoleClaimCommand : IRequest<bool>
{
    public string RoleName { get; init; } = string.Empty;
    public string ClaimType { get; init; } = "Permission"; // Varsayılan olarak "Permission"
    public string ClaimValue { get; init; } = string.Empty;
}
public class AddRoleClaimCommandHandler : IRequestHandler<AddRoleClaimCommand, bool>
{
    private readonly RoleManager<IdentityRole> _roleManager;

    public AddRoleClaimCommandHandler(RoleManager<IdentityRole> roleManager)
    {
        _roleManager = roleManager;
    }

    public async Task<bool> Handle(AddRoleClaimCommand request, CancellationToken cancellationToken)
    {
        var role = await _roleManager.FindByNameAsync(request.RoleName);
        if (role == null)
            throw new Exception($"Role not found '{request.RoleName}'.");

        var result = await _roleManager.AddClaimAsync(role, new Claim(request.ClaimType, request.ClaimValue));

        return result.Succeeded;
    }
}
