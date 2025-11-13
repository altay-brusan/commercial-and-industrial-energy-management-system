using Backend.Application.Common.Security;
using Microsoft.AspNetCore.Identity;
namespace Backend.Application.AppRoles.Commands.RemoveRoleClaim;
[Authorize(Roles = "Administrator")]
public record RemoveRoleClaimCommand : IRequest<bool>
{
    public string RoleName { get; init; } = string.Empty;
    public string ClaimType { get; init; } = "Permission";
    public string ClaimValue { get; init; } = string.Empty;
}
public class RemoveRoleClaimCommandHandler : IRequestHandler<RemoveRoleClaimCommand, bool>
{
    private readonly RoleManager<IdentityRole> _roleManager;

    public RemoveRoleClaimCommandHandler(RoleManager<IdentityRole> roleManager)
    {
        _roleManager = roleManager;
    }

    public async Task<bool> Handle(RemoveRoleClaimCommand request, CancellationToken cancellationToken)
    {
        var role = await _roleManager.FindByNameAsync(request.RoleName);
        if (role == null)
            throw new NotFoundException(request.RoleName, "Role not found.");

        var claims = await _roleManager.GetClaimsAsync(role);
        var claimToRemove = claims.FirstOrDefault(c => c.Type == request.ClaimType && c.Value == request.ClaimValue);

        if (claimToRemove == null)
            throw new NotFoundException(request.RoleName, "Bu claim role atanmış değil.");

        var result = await _roleManager.RemoveClaimAsync(role, claimToRemove);
        return result.Succeeded;
    }
}
