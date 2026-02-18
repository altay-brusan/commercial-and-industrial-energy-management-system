using Backend.Domain.Common;

namespace Backend.Domain.Entities;

public class TenantRolePermission : BaseAuditableEntity
{
    public Guid TenantRoleId { get; set; }
    public TenantRole TenantRole { get; set; } = null!;

    public Guid PermissionId { get; set; }
    public Permission Permission { get; set; } = null!;
}
