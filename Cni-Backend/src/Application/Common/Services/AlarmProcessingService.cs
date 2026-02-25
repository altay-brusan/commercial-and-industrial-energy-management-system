using System.Text.Json;
using System.Text.RegularExpressions;
using Backend.Application.Common.Interfaces;
using Backend.Domain.Entities;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;

namespace Backend.Application.Common.Services;

/// <summary>
/// Processes telemetry payloads to auto-create/recover DeviceAlarm records
/// based on the DeviceAlarmType catalog. Supports multiple detection strategies:
/// BooleanFlag (BESS containers), MessageMatch, ErrorCode.
/// </summary>
public class AlarmProcessingService
{
    private readonly IMemoryCache _cache;
    private readonly ILogger<AlarmProcessingService> _logger;

    private static readonly MemoryCacheEntryOptions TypeCacheOptions = new()
    {
        SlidingExpiration = TimeSpan.FromMinutes(30)
    };

    public AlarmProcessingService(IMemoryCache cache, ILogger<AlarmProcessingService> logger)
    {
        _cache = cache;
        _logger = logger;
    }

    /// <summary>
    /// Checks if the incoming message Kind has any alarm type definitions,
    /// and if so, creates or recovers DeviceAlarm records on the context's change tracker.
    /// The caller is responsible for calling SaveChangesAsync.
    /// </summary>
    public async Task ProcessAsync(
        IApplicationDbContext context,
        Guid deviceId,
        Guid deviceTypeId,
        string? manufacturer,
        string? model,
        string kind,
        string payload,
        DateTimeOffset timestamp,
        CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(model))
            return;

        // 1. Load alarm types for this device's model, filtered to this Kind
        var alarmTypes = await GetAlarmTypesForKind(context, deviceTypeId, manufacturer, model, kind, ct);
        if (alarmTypes.Count == 0)
            return;

        // 2. Parse payload
        var payloadElement = PayloadDeserializer.GetElement(payload);
        if (!payloadElement.HasValue)
            return;

        // 3. Resolve target device for rack-level addressing (bmuLoc)
        var targetDeviceId = deviceId;
        var bmuLoc = ExtractBmuLoc(payloadElement.Value);
        if (bmuLoc.HasValue)
        {
            var resolver = new DeviceAddressResolver(context, _cache);
            var childId = await resolver.ResolveChildByHierarchyIndex(deviceId, bmuLoc.Value, ct);
            if (childId.HasValue)
                targetDeviceId = childId.Value;
        }

        // 4. Load existing active alarms for this device, keyed by AlarmTypeId
        var activeAlarms = await context.DeviceAlarms
            .IgnoreQueryFilters()
            .Where(a => a.DeviceId == targetDeviceId && a.IsActive)
            .ToListAsync(ct);

        var activeAlarmsByTypeId = activeAlarms
            .ToDictionary(a => a.DeviceAlarmTypeId);

        // 5. Process each alarm type using its detection strategy
        foreach (var alarmType in alarmTypes)
        {
            var isFaulted = DetectFault(alarmType, payloadElement.Value);

            if (isFaulted && !activeAlarmsByTypeId.ContainsKey(alarmType.Id))
            {
                // New fault — create alarm record
                var alarm = new DeviceAlarm
                {
                    DeviceId = targetDeviceId,
                    DeviceAlarmTypeId = alarmType.Id,
                    OccurredAt = timestamp,
                    IsActive = true
                };
                context.DeviceAlarms.Add(alarm);
                _logger.LogInformation("Alarm created: {Code} on device {DeviceId}", alarmType.Code, targetDeviceId);
            }
            else if (!isFaulted && activeAlarmsByTypeId.TryGetValue(alarmType.Id, out var existingAlarm))
            {
                // Fault cleared — recover alarm
                existingAlarm.RecoveredAt = timestamp;
                existingAlarm.IsActive = false;
                _logger.LogInformation("Alarm recovered: {Code} on device {DeviceId}", alarmType.Code, targetDeviceId);
            }
        }
    }

    /// <summary>
    /// Determines if a fault is active based on the alarm type's detection strategy.
    /// </summary>
    private static bool DetectFault(DeviceAlarmType alarmType, JsonElement payload)
    {
        var value = PayloadDeserializer.ExtractValue(payload, alarmType.FaultKey);

        return alarmType.DetectionStrategy switch
        {
            "BooleanFlag" => value is true,
            "MessageMatch" => DetectMessageMatch(value, alarmType.MatchPattern),
            "ErrorCode" => DetectErrorCode(value),
            _ => value is true // fallback to boolean
        };
    }

    /// <summary>
    /// MessageMatch: fault is active if the value is a non-empty string
    /// that matches the optional pattern (regex), or any non-empty string if no pattern.
    /// </summary>
    private static bool DetectMessageMatch(object? value, string? matchPattern)
    {
        if (value is not string strValue || string.IsNullOrWhiteSpace(strValue))
            return false;

        if (string.IsNullOrWhiteSpace(matchPattern))
            return true; // Any non-empty message = fault

        return Regex.IsMatch(strValue, matchPattern, RegexOptions.IgnoreCase);
    }

    /// <summary>
    /// ErrorCode: fault is active if the value is a non-zero number.
    /// </summary>
    private static bool DetectErrorCode(object? value)
    {
        return value switch
        {
            double d => d != 0,
            int i => i != 0,
            long l => l != 0,
            _ => false
        };
    }

    private async Task<List<DeviceAlarmType>> GetAlarmTypesForKind(
        IApplicationDbContext context,
        Guid deviceTypeId,
        string? manufacturer,
        string model,
        string kind,
        CancellationToken ct)
    {
        var cacheKey = $"alarm-types:{deviceTypeId}:{manufacturer ?? ""}:{model}:{kind}";

        if (_cache.TryGetValue(cacheKey, out List<DeviceAlarmType>? cached) && cached != null)
            return cached;

        var types = await context.DeviceAlarmTypes
            .Where(t => t.DeviceTypeId == deviceTypeId
                        && t.Model == model
                        && t.Kind == kind
                        && t.IsEnabled
                        && (t.Manufacturer == null || t.Manufacturer == manufacturer))
            .ToListAsync(ct);

        _cache.Set(cacheKey, types, TypeCacheOptions);
        return types;
    }

    /// <summary>
    /// Extracts bmuLoc (rack address) from the payload if present.
    /// Edge devices send this as an integer field to identify which rack the data belongs to.
    /// </summary>
    private static int? ExtractBmuLoc(JsonElement payload)
    {
        if (payload.TryGetProperty("bmuLoc", out var bmuLocElement))
        {
            if (bmuLocElement.ValueKind == JsonValueKind.Number && bmuLocElement.TryGetInt32(out var loc))
                return loc;
        }

        return null;
    }
}
