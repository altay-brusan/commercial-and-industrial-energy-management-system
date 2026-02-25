using Backend.Application.Common.Interfaces;
using Microsoft.Extensions.Caching.Memory;

namespace Backend.Application.Common.Services;

/// <summary>
/// Resolves edge device addresses (e.g., bmuLoc integer) to Device GUIDs
/// using the HierarchyIndex on child devices.
/// </summary>
public class DeviceAddressResolver
{
    private readonly IApplicationDbContext _context;
    private readonly IMemoryCache _cache;

    private static readonly MemoryCacheEntryOptions CacheOptions = new()
    {
        SlidingExpiration = TimeSpan.FromMinutes(5)
    };

    public DeviceAddressResolver(IApplicationDbContext context, IMemoryCache cache)
    {
        _context = context;
        _cache = cache;
    }

    /// <summary>
    /// Resolves a child device by its parent and 0-based hierarchy index.
    /// Returns the child Device GUID, or null if not found.
    /// </summary>
    public async Task<Guid?> ResolveChildByHierarchyIndex(
        Guid parentDeviceId, int hierarchyIndex, CancellationToken ct)
    {
        var cacheKey = $"addr:{parentDeviceId}:{hierarchyIndex}";
        if (_cache.TryGetValue(cacheKey, out Guid cachedId))
            return cachedId;

        var childId = await _context.Devices
            .IgnoreQueryFilters()
            .Where(d => d.ParentDeviceId == parentDeviceId && d.HierarchyIndex == hierarchyIndex)
            .Select(d => d.Id)
            .FirstOrDefaultAsync(ct);

        if (childId != Guid.Empty)
        {
            _cache.Set(cacheKey, childId, CacheOptions);
            return childId;
        }

        return null;
    }
}
