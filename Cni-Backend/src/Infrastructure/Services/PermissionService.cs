using Backend.Application.Common.Interfaces;
using Backend.Domain.Entities;
using Backend.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Backend.Infrastructure.Services;

public class PermissionService : IPermissionService
{
    private readonly ApplicationDbContext _context;

    public PermissionService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<bool> TenantHasFeatureAsync(int tenantId, string featureKey, CancellationToken ct = default)
    {
        return await _context.Tenants
            .IgnoreQueryFilters()
            .Where(t => t.Id == tenantId && t.PlanId != null)
            .SelectMany(t => t.Plan!.PlanFeatures)
            .AnyAsync(pf => pf.Feature.Key == featureKey && pf.Feature.IsActive, ct);
    }

    public async Task<bool> UserHasPermissionAsync(string userId, int tenantId, string permissionKey, CancellationToken ct = default)
    {
        return await _context.Set<UserTenantRole>()
            .IgnoreQueryFilters()
            .Where(utr => utr.UserId == userId
                && utr.TenantRole.TenantId == tenantId
                && utr.TenantRole.IsActive)
            .SelectMany(utr => utr.TenantRole.TenantRolePermissions)
            .AnyAsync(trp => trp.Permission.Key == permissionKey, ct);
    }

    public async Task<List<string>> GetUserPermissionsAsync(string userId, int tenantId, CancellationToken ct = default)
    {
        return await _context.Set<UserTenantRole>()
            .IgnoreQueryFilters()
            .Where(utr => utr.UserId == userId
                && utr.TenantRole.TenantId == tenantId
                && utr.TenantRole.IsActive)
            .SelectMany(utr => utr.TenantRole.TenantRolePermissions)
            .Select(trp => trp.Permission.Key)
            .Distinct()
            .ToListAsync(ct);
    }

    public async Task<List<string>> GetTenantFeaturesAsync(int tenantId, CancellationToken ct = default)
    {
        return await _context.Tenants
            .IgnoreQueryFilters()
            .Where(t => t.Id == tenantId && t.PlanId != null)
            .SelectMany(t => t.Plan!.PlanFeatures)
            .Where(pf => pf.Feature.IsActive)
            .Select(pf => pf.Feature.Key)
            .Distinct()
            .ToListAsync(ct);
    }

    public async Task<string?> GetFeatureForPermissionAsync(string permissionKey, CancellationToken ct = default)
    {
        return await _context.Set<Permission>()
            .IgnoreQueryFilters()
            .Where(p => p.Key == permissionKey)
            .Select(p => p.Feature.Key)
            .FirstOrDefaultAsync(ct);
    }
}
