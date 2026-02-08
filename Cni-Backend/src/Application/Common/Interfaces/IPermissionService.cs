namespace Backend.Application.Common.Interfaces;

public interface IPermissionService
{
    Task<bool> TenantHasFeatureAsync(int tenantId, string featureKey, CancellationToken ct = default);
    Task<bool> UserHasPermissionAsync(string userId, int tenantId, string permissionKey, CancellationToken ct = default);
    Task<List<string>> GetUserPermissionsAsync(string userId, int tenantId, CancellationToken ct = default);
    Task<List<string>> GetTenantFeaturesAsync(int tenantId, CancellationToken ct = default);
    Task<string?> GetFeatureForPermissionAsync(string permissionKey, CancellationToken ct = default);
}
