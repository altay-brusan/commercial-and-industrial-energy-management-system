using Microsoft.AspNetCore.Identity;

namespace Backend.Application.AppRoles.Queries.GetRoleClaimsWithPagination;

public class RoleClaimDto
{
    public string ClaimType { get; set; } = string.Empty;
    public string ClaimValue { get; set; } = string.Empty;

}
