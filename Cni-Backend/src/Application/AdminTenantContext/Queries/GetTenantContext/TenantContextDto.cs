namespace Backend.Application.AdminTenantContext.Queries.GetTenantContext;

public class TenantContextDto
{
    /// <summary>
    /// The tenant ID from the authenticated user's JWT claims.
    /// </summary>
    public int? AuthenticatedTenantId { get; set; }

    /// <summary>
    /// The tenant ID selected by the admin via X-Selected-Tenant-Id header.
    /// </summary>
    public int? SelectedTenantId { get; set; }

    /// <summary>
    /// The effective tenant ID used for queries.
    /// </summary>
    public int? EffectiveTenantId { get; set; }

    /// <summary>
    /// Whether the current user is a super admin.
    /// </summary>
    public bool IsSuperAdmin { get; set; }

    /// <summary>
    /// Whether the context is scoped to a single tenant.
    /// </summary>
    public bool IsTenantScoped { get; set; }

    /// <summary>
    /// Whether the admin is currently impersonating/viewing a specific tenant.
    /// </summary>
    public bool IsImpersonating { get; set; }
}
