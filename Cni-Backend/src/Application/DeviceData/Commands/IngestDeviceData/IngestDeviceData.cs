using Backend.Application.Common.Interfaces;
using Backend.Application.Common.Services;
using Backend.Domain.Entities;
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
    private readonly AlarmProcessingService _alarmProcessor;

    private static readonly MemoryCacheEntryOptions CacheOptions = new()
    {
        SlidingExpiration = TimeSpan.FromMinutes(5)
    };

    public IngestDeviceDataCommandHandler(
        IApplicationDbContext context,
        IMemoryCache cache,
        ILogger<IngestDeviceDataCommandHandler> logger,
        ILoggerFactory loggerFactory)
    {
        _context = context;
        _cache = cache;
        _logger = logger;
        _alarmProcessor = new AlarmProcessingService(cache, loggerFactory.CreateLogger<AlarmProcessingService>());
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
                    Path = request.Path,
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
                    Path = request.Path,
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
                    Path = request.Path,
                    Payload = request.Payload,
                    DeviceLocalTime = deviceLocalTime,
                    Created = now,
                    LastModified = now
                });
                break;

            case DeviceDataCategory.SmartMeter:
            case DeviceDataCategory.Wind: // Wind shares SmartMeter table until a dedicated table is added
                _context.DeviceSmartMeterData.Add(new DeviceSmartMeterData
                {
                    DeviceId = deviceId.Value,
                    Kind = request.Kind,
                    Path = request.Path,
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
                    Path = request.Path,
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
            .Include(d => d.DeviceType)
            .FirstAsync(d => d.Id == deviceId.Value, cancellationToken);
        device.LastSeenUtc = now;

        // Process alarm flags from the payload (creates/recovers DeviceAlarm records)
        await _alarmProcessor.ProcessAsync(
            _context,
            deviceId.Value,
            device.DeviceTypeId,
            device.Manufacturer,
            device.Model,
            request.Kind,
            request.Path,
            request.Payload,
            deviceLocalTime,
            cancellationToken);

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
