using System.Text.Json;
using Backend.Application.Common.Interfaces;
using Backend.Domain.Entities;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;

namespace Backend.Application.DeviceData.Commands.IngestDeviceAlarm;

public record IngestDeviceAlarmCommand : IRequest<IngestDeviceAlarmResultDto>
{
    public string RtuSn { get; init; } = string.Empty;
    public string DeviceName { get; init; } = string.Empty;
    public List<AlarmPayloadItem> Alarms { get; init; } = new();
    public DateTimeOffset? DeviceLocalTime { get; init; }
}

public record AlarmPayloadItem
{
    public string Code { get; init; } = string.Empty;
    public string? Severity { get; init; }
    public bool Active { get; init; }
    public string? Message { get; init; }
    public double? Value { get; init; }
    public double? Threshold { get; init; }
    public DateTimeOffset? RaisedAt { get; init; }
}

public record IngestDeviceAlarmResultDto
{
    public bool Success { get; init; }
    public int Created { get; init; }
    public int Recovered { get; init; }
}

public class IngestDeviceAlarmCommandHandler : IRequestHandler<IngestDeviceAlarmCommand, IngestDeviceAlarmResultDto>
{
    private readonly IApplicationDbContext _context;
    private readonly IMemoryCache _cache;
    private readonly ILogger<IngestDeviceAlarmCommandHandler> _logger;

    private static readonly MemoryCacheEntryOptions CacheOptions = new()
    {
        SlidingExpiration = TimeSpan.FromMinutes(5)
    };

    private static readonly MemoryCacheEntryOptions AlarmTypeCacheOptions = new()
    {
        SlidingExpiration = TimeSpan.FromMinutes(30)
    };

    public IngestDeviceAlarmCommandHandler(
        IApplicationDbContext context,
        IMemoryCache cache,
        ILogger<IngestDeviceAlarmCommandHandler> logger)
    {
        _context = context;
        _cache = cache;
        _logger = logger;
    }

    public async Task<IngestDeviceAlarmResultDto> Handle(IngestDeviceAlarmCommand request, CancellationToken cancellationToken)
    {
        // 1. Resolve RTU by serial number
        var rtu = await ResolveRtu(request.RtuSn, cancellationToken);
        if (rtu == null)
        {
            _logger.LogWarning("RTU not found for serial number {RtuSn}", request.RtuSn);
            return new IngestDeviceAlarmResultDto { Success = false };
        }

        // 2. Resolve device by name under this RTU
        var device = await ResolveDeviceByName(request.DeviceName, rtu.Value.Id, cancellationToken);
        if (device == null)
        {
            _logger.LogWarning("Device not found with name {DeviceName} under RTU {RtuSn}", request.DeviceName, request.RtuSn);
            return new IngestDeviceAlarmResultDto { Success = false };
        }

        var deviceId = device.Value.Id;
        var deviceTypeId = device.Value.DeviceTypeId;
        var model = device.Value.Model;
        var manufacturer = device.Value.Manufacturer;
        var timestamp = request.DeviceLocalTime ?? DateTimeOffset.UtcNow;

        // 3. Load existing active alarms for this device
        var activeAlarms = await _context.DeviceAlarms
            .IgnoreQueryFilters()
            .Where(a => a.DeviceId == deviceId && a.IsActive)
            .Include(a => a.AlarmType)
            .ToListAsync(cancellationToken);

        var activeByCode = activeAlarms
            .Where(a => a.AlarmType != null)
            .ToDictionary(a => a.AlarmType!.Code, StringComparer.OrdinalIgnoreCase);

        int created = 0, recovered = 0;

        // 4. Track which codes are reported as active in this message
        var reportedActiveCodes = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        foreach (var alarm in request.Alarms)
        {
            if (string.IsNullOrWhiteSpace(alarm.Code))
                continue;

            // Resolve or auto-create DeviceAlarmType
            var alarmType = await ResolveAlarmType(alarm, deviceTypeId, model, manufacturer, cancellationToken);

            if (alarm.Active)
            {
                reportedActiveCodes.Add(alarm.Code);

                if (!activeByCode.ContainsKey(alarm.Code))
                {
                    // New fault — create alarm
                    var details = BuildDetails(alarm);
                    var deviceAlarm = new DeviceAlarm
                    {
                        DeviceId = deviceId,
                        DeviceAlarmTypeId = alarmType.Id,
                        OccurredAt = alarm.RaisedAt ?? timestamp,
                        IsActive = true,
                        Details = details,
                        RtuSerialNumber = request.RtuSn,
                        DeviceName = request.DeviceName
                    };
                    _context.DeviceAlarms.Add(deviceAlarm);
                    created++;
                    _logger.LogInformation("Alarm created: {Code} on device {DeviceName} via RTU {RtuSn}",
                        alarm.Code, request.DeviceName, request.RtuSn);
                }
            }
            else
            {
                // Explicitly cleared — recover if exists
                if (activeByCode.TryGetValue(alarm.Code, out var existing))
                {
                    existing.RecoveredAt = timestamp;
                    existing.IsActive = false;
                    recovered++;
                    _logger.LogInformation("Alarm recovered: {Code} on device {DeviceName} via RTU {RtuSn}",
                        alarm.Code, request.DeviceName, request.RtuSn);
                }
            }
        }

        // 5. Auto-recover active alarms NOT reported in this message (snapshot comparison)
        foreach (var (code, existingAlarm) in activeByCode)
        {
            if (!reportedActiveCodes.Contains(code))
            {
                existingAlarm.RecoveredAt = timestamp;
                existingAlarm.IsActive = false;
                recovered++;
                _logger.LogInformation("Alarm auto-recovered (not in snapshot): {Code} on device {DeviceName}",
                    code, request.DeviceName);
            }
        }

        await _context.SaveChangesAsync(cancellationToken);

        return new IngestDeviceAlarmResultDto
        {
            Success = true,
            Created = created,
            Recovered = recovered
        };
    }

    private static string? BuildDetails(AlarmPayloadItem alarm)
    {
        var hasDetails = alarm.Value.HasValue || alarm.Threshold.HasValue || !string.IsNullOrEmpty(alarm.Message);
        if (!hasDetails) return null;

        var details = new Dictionary<string, object?>();
        if (!string.IsNullOrEmpty(alarm.Message)) details["message"] = alarm.Message;
        if (alarm.Value.HasValue) details["value"] = alarm.Value.Value;
        if (alarm.Threshold.HasValue) details["threshold"] = alarm.Threshold.Value;

        return JsonSerializer.Serialize(details);
    }

    private async Task<DeviceAlarmType> ResolveAlarmType(
        AlarmPayloadItem alarm, Guid deviceTypeId, string model, string? manufacturer, CancellationToken ct)
    {
        var cacheKey = $"alarm-type-code:{alarm.Code}:{model}";
        if (_cache.TryGetValue(cacheKey, out DeviceAlarmType? cached) && cached != null)
            return cached;

        var alarmType = await _context.DeviceAlarmTypes
            .FirstOrDefaultAsync(t => t.Code == alarm.Code
                && t.Model == model
                && t.IsEnabled, ct);

        if (alarmType == null)
        {
            // Auto-create alarm type from the incoming data
            alarmType = new DeviceAlarmType
            {
                DeviceTypeId = deviceTypeId,
                Model = model,
                Manufacturer = manufacturer,
                Code = alarm.Code,
                Kind = "Device.Alarm",
                Name = alarm.Code,
                Category = "General",
                Severity = MapSeverity(alarm.Severity),
                FaultKey = alarm.Code,
                DetectionStrategy = "Explicit",
                IsEnabled = true
            };
            _context.DeviceAlarmTypes.Add(alarmType);
            await _context.SaveChangesAsync(ct);
            _logger.LogInformation("Auto-created alarm type: {Code} for model {Model}", alarm.Code, model);
        }

        _cache.Set(cacheKey, alarmType, AlarmTypeCacheOptions);
        return alarmType;
    }

    private static string MapSeverity(string? severity)
    {
        if (string.IsNullOrEmpty(severity)) return "Warning";
        return severity.ToLower() switch
        {
            "critical" => "Critical",
            "major" or "high" => "Major",
            "minor" or "medium" => "Minor",
            "warning" or "low" => "Warning",
            _ => "Warning"
        };
    }

    private async Task<(Guid Id, Guid FacilityId)?> ResolveRtu(string rtuSn, CancellationToken ct)
    {
        var cacheKey = $"rtu:sn:{rtuSn}";
        if (_cache.TryGetValue(cacheKey, out (Guid Id, Guid FacilityId) cachedRtu))
            return cachedRtu;

        var rtu = await _context.Rtus
            .IgnoreQueryFilters()
            .Where(r => r.RtuSerialNumber == rtuSn)
            .Select(r => new { r.Id, r.FacilityId })
            .FirstOrDefaultAsync(ct);

        if (rtu == null) return null;

        var result = (rtu.Id, rtu.FacilityId);
        _cache.Set(cacheKey, result, CacheOptions);
        return result;
    }

    private async Task<(Guid Id, Guid DeviceTypeId, string Model, string? Manufacturer)?> ResolveDeviceByName(
        string deviceName, Guid rtuId, CancellationToken ct)
    {
        var cacheKey = $"device:name:{deviceName}:rtu:{rtuId}";
        if (_cache.TryGetValue(cacheKey, out (Guid Id, Guid DeviceTypeId, string Model, string? Manufacturer) cachedDevice))
            return cachedDevice;

        var device = await _context.Devices
            .IgnoreQueryFilters()
            .Where(d => d.Name == deviceName && d.RtuId == rtuId)
            .Select(d => new { d.Id, d.DeviceTypeId, d.Model, d.Manufacturer })
            .FirstOrDefaultAsync(ct);

        if (device == null) return null;

        var result = (device.Id, device.DeviceTypeId, device.Model, device.Manufacturer);
        _cache.Set(cacheKey, result, CacheOptions);
        return result;
    }
}
