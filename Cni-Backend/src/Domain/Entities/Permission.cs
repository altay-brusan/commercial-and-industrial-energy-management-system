using Backend.Domain.Common;

namespace Backend.Domain.Entities;

public class Permission : BaseAuditableEntity
{
    public string Key { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }

    public int FeatureId { get; set; }
    public Feature Feature { get; set; } = null!;

    public ICollection<TenantRolePermission> TenantRolePermissions { get; set; } = new List<TenantRolePermission>();
}
