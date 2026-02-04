using Backend.Application.Common.Interfaces;

namespace Backend.Web.Services;

public class MultiTenantService : IMultiTenantService
{
    private int? _authenticatedTenantId;
    private int? _selectedTenantId;
    private bool _isSuperAdmin;

    /// <inheritdoc />
    public int? GetAuthenticatedTenantId() => _authenticatedTenantId;

    /// <inheritdoc />
    public int? GetSelectedTenantId() => _isSuperAdmin ? _selectedTenantId : null;

    /// <inheritdoc />
    public int? GetEffectiveTenantId()
    {
        if (_isSuperAdmin)
        {
            // Super admin: use selected tenant if set, otherwise null (shows all data)
            return _selectedTenantId;
        }

        // Regular user: always use their authenticated tenant
        return _authenticatedTenantId;
    }

    /// <inheritdoc />
    public bool IsSuperAdmin() => _isSuperAdmin;

    /// <inheritdoc />
    public bool IsTenantScoped()
    {
        if (_isSuperAdmin)
        {
            // Admin is scoped only if they've selected a tenant
            return _selectedTenantId.HasValue;
        }

        // Regular users are always scoped to their tenant
        return _authenticatedTenantId.HasValue;
    }

    /// <inheritdoc />
    public bool IsImpersonating() => _isSuperAdmin && _selectedTenantId.HasValue;

    /// <inheritdoc />
    public void SetSelectedTenantId(int? tenantId)
    {
        _selectedTenantId = tenantId;
    }

    /// <inheritdoc />
    public void ValidateTenantAccess(int tenantId)
    {
        if (_isSuperAdmin)
        {
            // Admins can access any tenant
            return;
        }

        if (_authenticatedTenantId != tenantId)
        {
            throw new UnauthorizedAccessException($"Access denied to tenant {tenantId}");
        }
    }

    /// <inheritdoc />
    public int GetTenantIdForCreation(int? requestedTenantId = null)
    {
        if (!_isSuperAdmin)
        {
            // Regular user: must use their own tenant
            if (!_authenticatedTenantId.HasValue)
            {
                throw new InvalidOperationException("User is not associated with a tenant");
            }
            return _authenticatedTenantId.Value;
        }

        // Super admin: priority order
        // 1. Selected tenant (from header)
        if (_selectedTenantId.HasValue)
        {
            return _selectedTenantId.Value;
        }

        // 2. Requested tenant (from request body)
        if (requestedTenantId.HasValue && requestedTenantId.Value > 0)
        {
            return requestedTenantId.Value;
        }

        // 3. Error - admin must select a tenant for create operations
        throw new InvalidOperationException(
            "Super admin must select a tenant via X-Selected-Tenant-Id header or provide TenantId in request");
    }

    /// <summary>
    /// Initializes the service with values from the middleware.
    /// Called by MultiTenantMiddleware after extracting values from the request.
    /// </summary>
    public void Initialize(int? authenticatedTenantId, bool isSuperAdmin, int? selectedTenantId)
    {
        _authenticatedTenantId = authenticatedTenantId;
        _isSuperAdmin = isSuperAdmin;
        _selectedTenantId = selectedTenantId;
    }

    #region Legacy Methods (Deprecated)

    /// <inheritdoc />
    [Obsolete("Use GetEffectiveTenantId() for queries or GetAuthenticatedTenantId() for the user's home tenant")]
    public int? GetCurrentTenantId() => GetEffectiveTenantId();

    /// <inheritdoc />
    [Obsolete("Use SetSelectedTenantId() instead")]
    public int? SetCurrentTenantId(int? tenantId)
    {
        _authenticatedTenantId = tenantId;
        return _authenticatedTenantId;
    }

    /// <inheritdoc />
    [Obsolete("Super admin status is now set automatically by MultiTenantMiddleware")]
    public bool SetSuperAdmin(bool isAdmin)
    {
        _isSuperAdmin = isAdmin;
        return _isSuperAdmin;
    }

    #endregion
}
