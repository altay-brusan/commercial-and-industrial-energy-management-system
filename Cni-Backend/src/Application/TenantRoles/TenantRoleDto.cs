using Backend.Application.Permissions.Queries.GetAllPermissions;
using Backend.Domain.Entities;

namespace Backend.Application.TenantRoles;

public class TenantRoleDto
{
    public int Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string? Description { get; init; }
    public bool IsActive { get; init; }
    public List<PermissionDto> Permissions { get; init; } = new();

    private class Mapping : Profile
    {
        public Mapping()
        {
            CreateMap<TenantRole, TenantRoleDto>()
                .ForMember(d => d.Permissions, opt => opt.MapFrom(s =>
                    s.TenantRolePermissions.Select(trp => trp.Permission)));
        }
    }
}
