namespace Backend.Domain.Constants;

public abstract class Roles
{
    /// <summary>
    /// Super administrator with full system access across all tenants
    /// </summary>
    public const string Administrator = nameof(Administrator);

    /// <summary>
    /// Tenant administrator with full access within their tenant
    /// </summary>
    public const string TenantAdmin = nameof(TenantAdmin);

    /// <summary>
    /// Regular user with limited access within their tenant
    /// </summary>
    public const string User = nameof(User);

    /// <summary>
    /// Operator with view and settings control access within their tenant
    /// </summary>
    public const string Operator = nameof(Operator);

    /// <summary>
    /// Viewer with read-only access within their tenant
    /// </summary>
    public const string Viewer = nameof(Viewer);
}
