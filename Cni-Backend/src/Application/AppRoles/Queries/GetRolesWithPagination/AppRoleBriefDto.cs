using Microsoft.AspNetCore.Identity;

namespace Backend.Application.AppRoles.Queries.GetAppRolesWithPagination;

public class AppRoleBriefDto
{
    public string Id { get; set; } = default!;
    public string? Name { get; set; }

    private class Mapping : Profile
    {
        public Mapping()
        {
            CreateMap<IdentityRole, AppRoleBriefDto>();
        }
    }
}
