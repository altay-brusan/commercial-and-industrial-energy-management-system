using Backend.Application.Common.Interfaces;

namespace Backend.Web.Services;

public class MultiTenantService : IMultiTenantService
{
    private int? _currentTenantId = null;
    private bool _isAdmin = false;
    public int? GetCurrentTenantId()
    {
        return _currentTenantId;
    }

    public int? SetCurrentTenantId(int? tenantId)
    {
        _currentTenantId = tenantId;
        return _currentTenantId;
    }
    public bool SetSuperAdmin(bool isAdmin)
    {
        _isAdmin = isAdmin;
        return _isAdmin;
    }
    public bool IsSuperAdmin() { 
    
        return _isAdmin;
    }
}
