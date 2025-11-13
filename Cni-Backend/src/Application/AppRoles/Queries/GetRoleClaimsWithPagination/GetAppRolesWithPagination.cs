using Backend.Application.Common.Security;
using Microsoft.AspNetCore.Identity;

namespace Backend.Application.AppRoles.Queries.GetRoleClaimsWithPagination;
[Authorize(Roles =  "Administrator,Tenant.Admin")]
public record GetRoleClaimsQuery : IRequest<List<RoleClaimDto>>
{
    public string RoleName { get; init; } = string.Empty;
}

public class GetRoleClaimsQueryHandler : IRequestHandler<GetRoleClaimsQuery, List<RoleClaimDto>>
{
    private readonly RoleManager<IdentityRole> _roleManager;

    public GetRoleClaimsQueryHandler(RoleManager<IdentityRole> roleManager)
    {
        _roleManager = roleManager;
    }

    public async Task<List<RoleClaimDto>> Handle(GetRoleClaimsQuery request, CancellationToken cancellationToken)
    {
        var role = await _roleManager.FindByNameAsync(request.RoleName);
        if (role == null)
            throw new Exception($"Role not found '{request.RoleName}'.");

        var claims = await _roleManager.GetClaimsAsync(role);

        return claims.Select(c => new RoleClaimDto
        {
            ClaimType = c.Type,
            ClaimValue = c.Value
        }).ToList();
    }
}
