namespace Backend.Domain.Entities;

/// <summary>
/// Represents a Tenant (Holding Company) - the top-level organizational unit.
/// A Tenant owns multiple Firms (Sites/Factories).
/// </summary>
public class Tenant : BaseAuditableEntity
{
    /// <summary>
    /// Unique identifier for the tenant (e.g., "acme-corp")
    /// </summary>
    public string? Identifier { get; set; }

    /// <summary>
    /// Display name of the tenant (e.g., "ACME Corporation")
    /// </summary>
    public string? Name { get; set; }

    public Guid? PlanId { get; set; }
    public Plan? Plan { get; set; }

    // Navigation properties
    public ICollection<Firm>? Firms { get; set; }
    public ICollection<TenantRole> TenantRoles { get; set; } = new List<TenantRole>();
}
