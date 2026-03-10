using System.Text.Json;
using System.Text.RegularExpressions;
using Backend.Application.Common.Interfaces;
using Backend.Domain.Entities;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;

namespace Backend.Application.Common.Services;

/// <summary>
/// Processes dedicated alarm packages (e.g., Battery.Alarm, Pcs.Alarm) sent by the RTU.
/// The RTU determines when faults are active/recovered — backend records them as DeviceAlarm entities.
/// </summary>
public class AlarmProcessingService
{
    private readonly IMemoryCache _cache;
    private readonly ILogger<AlarmProcessingService> _logger;

    public AlarmProcessingService(IMemoryCache cache, ILogger<AlarmProcessingService> logger)
    {
        _cache = cache;
        _logger = logger;
    }

    /// <summary>
    /// Processes a dedicated alarm package from the RTU.
    /// Expected payload format:
    /// { "code": "CELL_OVERVOLT_WARN", "active": true, "message": "Cell 12 exceeded 3.7V" }
    /// If active=true and no matching active alarm exists, creates one.
    /// If active=false and a matching alarm exists, recovers it.
    /// </summary>
    public async Task ProcessAlarmPackageAsync(
        IApplicationDbContext context,
        Guid deviceId,
        string? path,
        string payload,
        DateTimeOffset timestamp,
        CancellationToken ct)
    {
        var payloadElement = PayloadDeserializer.GetElement(payload);
        if (!payloadElement.HasValue)
            return;

        var root = payloadElement.Value;

        // Extract required fields
        if (!root.TryGetProperty("code", out var codeElement) || codeElement.ValueKind != JsonValueKind.String)
        {
            _logger.LogWarning("Alarm package missing 'code' field on device {DeviceId}", deviceId);
            return;
        }
        var code = codeElement.GetString()!;

        var isActive = true;
        if (root.TryGetProperty("active", out var activeElement) && activeElement.ValueKind == JsonValueKind.False)
            isActive = false;

        string? message = null;
        if (root.TryGetProperty("message", out var msgElement) && msgElement.ValueKind == JsonValueKind.String)
            message = msgElement.GetString();

        // Find the alarm type by code
        var alarmType = await ResolveAlarmTypeByCode(context, code, ct);
        if (alarmType == null)
        {
            _logger.LogWarning("Unknown alarm code {Code} on device {DeviceId} — no matching DeviceAlarmType", code, deviceId);
            return;
        }

        // Check existing active alarm for this device + path + alarm type
        var existingAlarm = await context.DeviceAlarms
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(a => a.DeviceId == deviceId
                                      && a.Path == path
                                      && a.DeviceAlarmTypeId == alarmType.Id
                                      && a.IsActive, ct);

        if (isActive && existingAlarm == null)
        {
            // New fault
            var alarm = new DeviceAlarm
            {
                DeviceId = deviceId,
                DeviceAlarmTypeId = alarmType.Id,
                Path = path,
                OccurredAt = timestamp,
                IsActive = true,
                Details = message
            };
            context.DeviceAlarms.Add(alarm);
            _logger.LogInformation("Alarm created: {Code} on device {DeviceId} path {Path}",
                code, deviceId, path ?? "(device-level)");
        }
        else if (!isActive && existingAlarm != null)
        {
            // Fault cleared
            existingAlarm.RecoveredAt = timestamp;
            existingAlarm.IsActive = false;
            if (message != null)
                existingAlarm.Details = message;
            _logger.LogInformation("Alarm recovered: {Code} on device {DeviceId} path {Path}",
                code, deviceId, path ?? "(device-level)");
        }
    }

    private async Task<DeviceAlarmType?> ResolveAlarmTypeByCode(
        IApplicationDbContext context, string code, CancellationToken ct)
    {
        var cacheKey = $"alarm-type-code:{code}";

        if (_cache.TryGetValue(cacheKey, out DeviceAlarmType? cached))
            return cached;

        var alarmType = await context.DeviceAlarmTypes
            .FirstOrDefaultAsync(t => t.Code == code && t.IsEnabled, ct);

        if (alarmType != null)
        {
            _cache.Set(cacheKey, alarmType, new MemoryCacheEntryOptions
            {
                SlidingExpiration = TimeSpan.FromMinutes(30)
            });
        }

        return alarmType;
    }
}
