using Backend.Domain.Common;

namespace Backend.Domain.Entities;

public class TenantRolePermission : BaseAuditableEntity
{
    public int TenantRoleId { get; set; }
    public TenantRole TenantRole { get; set; } = null!;

    public int PermissionId { get; set; }
    public Permission Permission { get; set; } = null!;
}
