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
}
