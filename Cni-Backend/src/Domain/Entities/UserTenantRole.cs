using Backend.Domain.Common;

namespace Backend.Domain.Entities;

public class UserTenantRole : BaseAuditableEntity
{
    public string UserId { get; set; } = string.Empty;
    public ApplicationUser User { get; set; } = null!;

    public Guid TenantRoleId { get; set; }
    public TenantRole TenantRole { get; set; } = null!;
}
