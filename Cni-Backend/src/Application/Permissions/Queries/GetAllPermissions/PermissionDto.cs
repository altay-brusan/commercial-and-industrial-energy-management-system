using Backend.Domain.Entities;

namespace Backend.Application.Permissions.Queries.GetAllPermissions;

public class PermissionDto
{
    public int Id { get; init; }
    public string Key { get; init; } = string.Empty;
    public string Name { get; init; } = string.Empty;
    public string? Description { get; init; }
    public string FeatureKey { get; init; } = string.Empty;

    private class Mapping : Profile
    {
        public Mapping()
        {
            CreateMap<Permission, PermissionDto>()
                .ForMember(d => d.FeatureKey, opt => opt.MapFrom(s => s.Feature.Key));
        }
    }
}

public class FeaturePermissionsDto
{
    public string FeatureKey { get; init; } = string.Empty;
    public string FeatureName { get; init; } = string.Empty;
    public List<PermissionDto> Permissions { get; init; } = new();
}
