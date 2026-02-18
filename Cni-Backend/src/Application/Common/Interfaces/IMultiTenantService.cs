namespace Backend.Application.Common.Interfaces;

public interface IMultiTenantService
{
    /// <summary>
    /// Gets the tenant ID from the authenticated user's JWT claims.
    /// </summary>
    Guid? GetAuthenticatedTenantId();

    /// <summary>
    /// Gets the tenant ID selected by a super admin via the X-Selected-Tenant-Id header.
    /// Returns null if not a super admin or no tenant selected.
    /// </summary>
    Guid? GetSelectedTenantId();

    /// <summary>
    /// Gets the effective tenant ID to use for queries.
    /// For super admins: returns selected tenant if set, otherwise null (shows all data).
    /// For regular users: returns authenticated tenant.
    /// </summary>
    Guid? GetEffectiveTenantId();

    /// <summary>
    /// Returns true if the current user is a super admin (Administrator role).
    /// </summary>
    bool IsSuperAdmin();

    /// <summary>
    /// Returns true if the context is scoped to a single tenant.
    /// True for regular users (always scoped to their tenant).
    /// True for admins who have selected a specific tenant.
    /// False for admins without a tenant selection (overview mode).
    /// </summary>
    bool IsTenantScoped();

    /// <summary>
    /// Returns true if a super admin is currently impersonating/viewing a specific tenant.
    /// </summary>
    bool IsImpersonating();

    /// <summary>
    /// Sets the selected tenant ID for admin tenant impersonation.
    /// Called by middleware when processing X-Selected-Tenant-Id header.
    /// </summary>
    void SetSelectedTenantId(Guid? tenantId);

    /// <summary>
    /// Validates that the current user has access to the specified tenant.
    /// Throws UnauthorizedAccessException if access is denied.
    /// </summary>
    void ValidateTenantAccess(Guid tenantId);

    /// <summary>
    /// Gets the tenant ID to use when creating a new entity.
    /// For regular users: returns their authenticated tenant ID.
    /// For super admins: returns selected tenant (from header) or requestedTenantId if provided.
    /// Throws InvalidOperationException if admin hasn't selected a tenant and no requestedTenantId provided.
    /// </summary>
    Guid GetTenantIdForCreation(Guid? requestedTenantId = null);

    #region Legacy Methods (Deprecated)

    /// <summary>
    /// Gets the current tenant ID. Use GetEffectiveTenantId() instead for new code.
    /// </summary>
    [Obsolete("Use GetEffectiveTenantId() for queries or GetAuthenticatedTenantId() for the user's home tenant")]
    Guid? GetCurrentTenantId();

    /// <summary>
    /// Sets the current tenant ID. Use SetSelectedTenantId() instead.
    /// </summary>
    [Obsolete("Use SetSelectedTenantId() instead")]
    Guid? SetCurrentTenantId(Guid? tenantId);

    /// <summary>
    /// Sets the super admin flag. This is now handled automatically by middleware.
    /// </summary>
    [Obsolete("Super admin status is now set automatically by MultiTenantMiddleware")]
    bool SetSuperAdmin(bool isAdmin);

    #endregion
}
