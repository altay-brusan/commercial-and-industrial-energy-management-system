using Backend.Application.Common.Security;
using Backend.Domain.Entities;
using Microsoft.AspNetCore.Identity;

namespace Backend.Application.Users.Queries.GetUserPermissions;
[Authorize]
public record GetUserPermissionsQuery : IRequest<List<string>>
{
    public string UserId { get; init; } = string.Empty;
}
public class GetUserPermissionsQueryHandler : IRequestHandler<GetUserPermissionsQuery, List<string>>
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly RoleManager<IdentityRole> _roleManager;

    public GetUserPermissionsQueryHandler(UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager)
    {
        _userManager = userManager;
        _roleManager = roleManager;
    }

    public async Task<List<string>> Handle(GetUserPermissionsQuery request, CancellationToken cancellationToken)
    {
        var user = await _userManager.FindByIdAsync(request.UserId);
        if (user == null)
            throw new NotFoundException(request.UserId, $"User not found '{request.UserId}'.");

        var roles = await _userManager.GetRolesAsync(user);
        var permissions = new List<string>();

        foreach (var role in roles)
        {
            var identityRole = await _roleManager.FindByNameAsync(role);
            if (identityRole != null)
            {
                var claims = await _roleManager.GetClaimsAsync(identityRole);
                permissions.AddRange(claims.Where(c => c.Type == "Permission").Select(c => c.Value));
            }
        }

        return permissions.Distinct().ToList();
    }
}
