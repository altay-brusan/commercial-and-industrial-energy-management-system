namespace Backend.Application.AdminTenantContext.Queries.GetAvailableTenants;

public class TenantSelectorDto
{
    /// <summary>
    /// The tenant ID to use in X-Selected-Tenant-Id header.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Display name for the tenant selector UI.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Unique identifier for the tenant.
    /// </summary>
    public string? Identifier { get; set; }
}
