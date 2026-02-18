using Backend.Domain.Common;

namespace Backend.Domain.Entities;

public class TenantRole : BaseAuditableEntity, IMultiTenantEntity
{
    public Guid TenantId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsActive { get; set; } = true;

    public Tenant Tenant { get; set; } = null!;
    public ICollection<TenantRolePermission> TenantRolePermissions { get; set; } = new List<TenantRolePermission>();
    public ICollection<UserTenantRole> UserTenantRoles { get; set; } = new List<UserTenantRole>();
}
