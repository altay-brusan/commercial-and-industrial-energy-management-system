namespace Backend.Application.Common.Interfaces;

public interface IPermissionService
{
    Task<bool> TenantHasFeatureAsync(Guid tenantId, string featureKey, CancellationToken ct = default);
    Task<bool> UserHasPermissionAsync(string userId, Guid tenantId, string permissionKey, CancellationToken ct = default);
    Task<List<string>> GetUserPermissionsAsync(string userId, Guid tenantId, CancellationToken ct = default);
    Task<List<string>> GetTenantFeaturesAsync(Guid tenantId, CancellationToken ct = default);
    Task<string?> GetFeatureForPermissionAsync(string permissionKey, CancellationToken ct = default);
}
