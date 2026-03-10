using Backend.Application.Common.Interfaces;
using Backend.Application.Common.Services;
using Backend.Domain.Entities.DeviceData;
using Backend.Domain.Enums;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;

namespace Backend.Application.DeviceData.Commands.IngestDeviceData;

public record IngestDeviceDataCommand : IRequest<bool>
{
    public string RtuSn { get; init; } = string.Empty;
    public string DeviceName { get; init; } = string.Empty;
    public string Kind { get; init; } = string.Empty;
    public string? Path { get; init; }
    public string Payload { get; init; } = "{}";
    public DateTimeOffset? DeviceLocalTime { get; init; }
}

public class IngestDeviceDataCommandHandler : IRequestHandler<IngestDeviceDataCommand, bool>
{
    private readonly IApplicationDbContext _context;
    private readonly IMemoryCache _cache;
    private readonly ILogger<IngestDeviceDataCommandHandler> _logger;

    private static readonly MemoryCacheEntryOptions CacheOptions = new()
    {
        SlidingExpiration = TimeSpan.FromMinutes(5)
    };

    private static readonly TimeSpan LastSeenThrottle = TimeSpan.FromSeconds(30);

    public IngestDeviceDataCommandHandler(
        IApplicationDbContext context,
        IMemoryCache cache,
        ILogger<IngestDeviceDataCommandHandler> logger)
    {
        _context = context;
        _cache = cache;
        _logger = logger;
    }

    public async Task<bool> Handle(IngestDeviceDataCommand request, CancellationToken cancellationToken)
    {
        var rtuId = await ResolveRtuId(request.RtuSn, cancellationToken);
        if (rtuId == null)
        {
            _logger.LogWarning("RTU not found for serial number {RtuSn}", request.RtuSn);
            return false;
        }

        var deviceId = await ResolveDeviceId(request.DeviceName, rtuId.Value, cancellationToken);
        if (deviceId == null)
        {
            _logger.LogWarning("Device not found for name {DeviceName} under RTU {RtuSn}", request.DeviceName, request.RtuSn);
            return false;
        }

        var now = DateTimeOffset.UtcNow;
        var deviceLocalTime = request.DeviceLocalTime ?? now;

        // Handle alarm packages separately — they don't produce telemetry records
        if (DeviceDataCategorizer.IsAlarmPackage(request.Kind))
        {
            await UpdateLastSeenThrottled(rtuId.Value, deviceId.Value, now, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);
            return true;
        }

        var category = DeviceDataCategorizer.Categorize(request.Kind);


        var device = await _context.Devices.IgnoreQueryFilters()
            .FirstAsync(d => d.Id == deviceId.Value, cancellationToken);
        device.LastSeenUtc = now;


        if (category == DeviceDataCategory.Unknown)
        {
            _logger.LogWarning("Unrecognized kind {Kind} for device {DeviceName} — message rejected", request.Kind, request.DeviceName);
            return false;
        }

        StoreTelemetry(category, deviceId.Value, request.Kind, request.Path, request.Payload, deviceLocalTime, now);
        await UpdateLastSeenThrottled(rtuId.Value, deviceId.Value, now, cancellationToken);

        await _context.SaveChangesAsync(cancellationToken);

        return true;
    }

    private void StoreTelemetry(
        DeviceDataCategory category, Guid deviceId,
        string kind, string? path, string payload,
        DateTimeOffset deviceLocalTime, DateTimeOffset now)
    {
        DeviceDataBase entity = category switch
        {
            DeviceDataCategory.Bess => new DeviceBatteryData(),
            DeviceDataCategory.Pcs => new DevicePcsData(),
            DeviceDataCategory.Solar => new DeviceSolarData(),
            DeviceDataCategory.SmartMeter or DeviceDataCategory.Wind => new DeviceSmartMeterData(),
            _ => throw new InvalidOperationException($"Unexpected category {category}")
        };

        entity.DeviceId = deviceId;
        entity.Kind = kind;
        entity.Path = path;
        entity.Payload = payload;
        entity.DeviceLocalTime = deviceLocalTime;
        entity.Created = now;
        entity.LastModified = now;

        switch (entity)
        {
            case DeviceBatteryData e: _context.DeviceBatteryData.Add(e); break;
            case DevicePcsData e: _context.DevicePcsData.Add(e); break;
            case DeviceSolarData e: _context.DeviceSolarData.Add(e); break;
            case DeviceSmartMeterData e: _context.DeviceSmartMeterData.Add(e); break;
        }
    }

    /// <summary>
    /// Throttles LastSeenUtc updates to avoid excessive DB writes.
    /// Only updates if the last write was more than 30 seconds ago.
    /// </summary>
    private async Task UpdateLastSeenThrottled(Guid rtuId, Guid deviceId, DateTimeOffset now, CancellationToken ct)
    {
        var rtuCacheKey = $"lastseen:rtu:{rtuId}";
        if (!_cache.TryGetValue(rtuCacheKey, out DateTimeOffset _))
        {
            var rtu = await _context.Rtus.IgnoreQueryFilters()
                .FirstAsync(r => r.Id == rtuId, ct);
            rtu.LastSeenUtc = now;
            _cache.Set(rtuCacheKey, now, new MemoryCacheEntryOptions { AbsoluteExpirationRelativeToNow = LastSeenThrottle });
        }

        var deviceCacheKey = $"lastseen:device:{deviceId}";
        if (!_cache.TryGetValue(deviceCacheKey, out DateTimeOffset _))
        {
            var device = await _context.Devices.IgnoreQueryFilters()
                .FirstAsync(d => d.Id == deviceId, ct);
            device.LastSeenUtc = now;
            _cache.Set(deviceCacheKey, now, new MemoryCacheEntryOptions { AbsoluteExpirationRelativeToNow = LastSeenThrottle });
        }
    }

    private async Task<Guid?> ResolveRtuId(string rtuSn, CancellationToken cancellationToken)
    {
        var cacheKey = $"rtu:sn:{rtuSn}";

        if (_cache.TryGetValue(cacheKey, out Guid cachedId))
            return cachedId;

        var rtuId = await _context.Rtus
            .IgnoreQueryFilters()
            .Where(r => r.RtuSerialNumber == rtuSn)
            .Select(r => (Guid?)r.Id)
            .FirstOrDefaultAsync(cancellationToken);

        if (rtuId.HasValue)
            _cache.Set(cacheKey, rtuId.Value, CacheOptions);

        return rtuId;
    }

    private async Task<Guid?> ResolveDeviceId(string deviceName, Guid rtuId, CancellationToken cancellationToken)
    {
        var cacheKey = $"device:name:{deviceName}:rtu:{rtuId}";

        if (_cache.TryGetValue(cacheKey, out Guid cachedId))
            return cachedId;

        var deviceId = await _context.Devices
            .IgnoreQueryFilters()
            .Where(d => d.Name == deviceName && d.RtuId == rtuId)
            .Select(d => (Guid?)d.Id)
            .FirstOrDefaultAsync(cancellationToken);

        if (deviceId.HasValue)
            _cache.Set(cacheKey, deviceId.Value, CacheOptions);

        return deviceId;
    }
}
