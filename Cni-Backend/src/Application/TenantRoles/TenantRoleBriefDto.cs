using Backend.Domain.Entities;

namespace Backend.Application.TenantRoles;

public class TenantRoleBriefDto
{
    public Guid Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string? Description { get; init; }
    public bool IsActive { get; init; }

    private class Mapping : Profile
    {
        public Mapping()
        {
            CreateMap<TenantRole, TenantRoleBriefDto>();
        }
    }
}
