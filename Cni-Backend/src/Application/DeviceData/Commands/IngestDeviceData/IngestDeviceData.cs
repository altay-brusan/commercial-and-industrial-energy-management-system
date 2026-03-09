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
    public string DeviceSn { get; init; } = string.Empty;
    public string Kind { get; init; } = string.Empty;
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

        var deviceId = await ResolveDeviceId(request.DeviceSn, rtuId.Value, cancellationToken);
        if (deviceId == null)
        {
            _logger.LogWarning("Device not found for serial number {DeviceSn} under RTU {RtuSn}", request.DeviceSn, request.RtuSn);
            return false;
        }

        var category = DeviceDataCategorizer.Categorize(request.Kind);
        var now = DateTimeOffset.UtcNow;
        var deviceLocalTime = request.DeviceLocalTime ?? now;

        switch (category)
        {
            case DeviceDataCategory.Bess:
                _context.DeviceBatteryData.Add(new DeviceBatteryData
                {
                    DeviceId = deviceId.Value,
                    Kind = request.Kind,
                    Payload = request.Payload,
                    DeviceLocalTime = deviceLocalTime,
                    Created = now,
                    LastModified = now
                });
                break;

            case DeviceDataCategory.Pcs:
                _context.DevicePcsData.Add(new DevicePcsData
                {
                    DeviceId = deviceId.Value,
                    Kind = request.Kind,
                    Payload = request.Payload,
                    DeviceLocalTime = deviceLocalTime,
                    Created = now,
                    LastModified = now
                });
                break;

            case DeviceDataCategory.Solar:
                _context.DeviceSolarData.Add(new DeviceSolarData
                {
                    DeviceId = deviceId.Value,
                    Kind = request.Kind,
                    Payload = request.Payload,
                    DeviceLocalTime = deviceLocalTime,
                    Created = now,
                    LastModified = now
                });
                break;

            case DeviceDataCategory.SmartMeter:
                _context.DeviceSmartMeterData.Add(new DeviceSmartMeterData
                {
                    DeviceId = deviceId.Value,
                    Kind = request.Kind,
                    Payload = request.Payload,
                    DeviceLocalTime = deviceLocalTime,
                    Created = now,
                    LastModified = now
                });
                break;

            default:
                _logger.LogWarning("Unknown device data category for kind {Kind}, storing as battery data", request.Kind);
                _context.DeviceBatteryData.Add(new DeviceBatteryData
                {
                    DeviceId = deviceId.Value,
                    Kind = request.Kind,
                    Payload = request.Payload,
                    DeviceLocalTime = deviceLocalTime,
                    Created = now,
                    LastModified = now
                });
                break;
        }

        // Update LastSeenUtc on both RTU and Device via tracked entities
        var rtu = await _context.Rtus.IgnoreQueryFilters()
            .FirstAsync(r => r.Id == rtuId.Value, cancellationToken);
        rtu.LastSeenUtc = now;

        var device = await _context.Devices.IgnoreQueryFilters()
            .FirstAsync(d => d.Id == deviceId.Value, cancellationToken);
        device.LastSeenUtc = now;

        await _context.SaveChangesAsync(cancellationToken);

        return true;
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

    private async Task<Guid?> ResolveDeviceId(string deviceSn, Guid rtuId, CancellationToken cancellationToken)
    {
        var cacheKey = $"device:sn:{deviceSn}:rtu:{rtuId}";

        if (_cache.TryGetValue(cacheKey, out Guid cachedId))
            return cachedId;

        var deviceId = await _context.Devices
            .IgnoreQueryFilters()
            .Where(d => d.SerialNumber == deviceSn && d.RtuId == rtuId)
            .Select(d => (Guid?)d.Id)
            .FirstOrDefaultAsync(cancellationToken);

        if (deviceId.HasValue)
            _cache.Set(cacheKey, deviceId.Value, CacheOptions);

        return deviceId;
    }
}
