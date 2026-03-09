using System.Text.Json;
using Backend.Application.Common.Interfaces;
using Backend.Domain.Entities.Huawei;
using Backend.Infrastructure.Huawei;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Backend.Infrastructure.BackgroundJobs;

public class HuaweiFusionSolarSyncWorker
{
    private readonly ILogger<HuaweiFusionSolarSyncWorker> _logger;
    private readonly IApplicationDbContext _context;
    private readonly HuaweiTokenManager _tokenManager;
    private readonly IHuaweiApiClient _apiClient;
    private readonly HuaweiDataTranslator _translator;

    // Huawei device type IDs
    private const int DevTypeInverter = 1;
    private const int DevTypeBattery = 39;

    public HuaweiFusionSolarSyncWorker(
        ILogger<HuaweiFusionSolarSyncWorker> logger,
        IApplicationDbContext context,
        HuaweiTokenManager tokenManager,
        IHuaweiApiClient apiClient,
        HuaweiDataTranslator translator)
    {
        _logger = logger;
        _context = context;
        _tokenManager = tokenManager;
        _apiClient = apiClient;
        _translator = translator;
    }

    public async Task SyncAllAccounts(CancellationToken ct = default)
    {
        var accounts = await _context.HuaweiAccounts
            .Include(a => a.StationMappings.Where(s => s.SyncEnabled))
                .ThenInclude(s => s.DeviceMappings)
            .Where(a => a.IsActive)
            .IgnoreQueryFilters()
            .ToListAsync(ct);

        if (accounts.Count == 0)
        {
            _logger.LogDebug("No active Huawei accounts found, skipping sync");
            return;
        }

        _logger.LogInformation("Starting Huawei FusionSolar sync for {Count} account(s)", accounts.Count);
        var errors = new List<string>();

        foreach (var account in accounts)
        {
            ct.ThrowIfCancellationRequested();
            try
            {
                await SyncAccountAsync(account, ct);
            }
            catch (OperationCanceledException) { throw; }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to sync Huawei account {AccountId} ({UserName})",
                    account.Id, account.UserName);
                errors.Add($"{account.UserName}: {ex.Message}");
            }
        }

        if (errors.Count > 0)
        {
            throw new AggregateException(
                $"Huawei sync failed for {errors.Count} account(s): {string.Join("; ", errors)}",
                errors.Select(e => new InvalidOperationException(e)));
        }
    }

    private async Task SyncAccountAsync(HuaweiAccount account, CancellationToken ct)
    {
        var enabledStations = account.StationMappings
            .Where(s => s.SyncEnabled)
            .ToList();

        if (enabledStations.Count == 0)
        {
            _logger.LogDebug("No enabled stations for account {AccountId}", account.Id);
            return;
        }

        // Pre-load all devices referenced by mappings to avoid N+1 queries
        var allDeviceIds = enabledStations
            .SelectMany(s => s.DeviceMappings)
            .Select(d => d.DeviceId)
            .Distinct()
            .ToList();

        var deviceLookup = await _context.Devices
            .Include(d => d.DeviceType)
            .Where(d => allDeviceIds.Contains(d.Id))
            .IgnoreQueryFilters()
            .ToDictionaryAsync(d => d.Id, ct);

        string token;
        try
        {
            token = await _tokenManager.GetTokenAsync(account, ct);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get token for account {AccountId}", account.Id);
            LogSyncResult(account.Id, "RealTimeKpi", "Failed", 0, 0, $"Login failed: {ex.Message}");
            await _context.SaveChangesAsync(ct);
            throw;
        }

        var syncStartedAt = DateTimeOffset.UtcNow;
        var totalRecords = 0;
        var totalDevices = 0;

        foreach (var station in enabledStations)
        {
            ct.ThrowIfCancellationRequested();
            try
            {
                var (devices, records) = await SyncStationAsync(account, station, token, deviceLookup, ct);
                totalDevices += devices;
                totalRecords += records;
                station.LastSyncUtc = DateTimeOffset.UtcNow;
            }
            catch (HuaweiTokenExpiredException)
            {
                _logger.LogWarning("Token expired during sync, refreshing for account {AccountId}", account.Id);
                _tokenManager.InvalidateToken(account.Id);
                token = await _tokenManager.RefreshTokenAsync(account, ct);

                var (devices, records) = await SyncStationAsync(account, station, token, deviceLookup, ct);
                totalDevices += devices;
                totalRecords += records;
                station.LastSyncUtc = DateTimeOffset.UtcNow;
            }
            catch (HuaweiRateLimitException ex)
            {
                _logger.LogWarning(ex, "Rate limited by Huawei API for account {AccountId}, skipping remaining stations", account.Id);
                break;
            }
        }

        LogSyncResult(account.Id, "RealTimeKpi", "Success", totalDevices, totalRecords, null);
        await _context.SaveChangesAsync(ct);

        _logger.LogInformation(
            "Huawei sync completed for account {AccountId}: {Devices} devices, {Records} records in {Elapsed:F1}s",
            account.Id, totalDevices, totalRecords,
            (DateTimeOffset.UtcNow - syncStartedAt).TotalSeconds);
    }

    private async Task<(int devices, int records)> SyncStationAsync(
        HuaweiAccount account, HuaweiStationMapping station, string token,
        Dictionary<Guid, Domain.Entities.Device> deviceLookup,
        CancellationToken ct)
    {
        var deviceMappings = station.DeviceMappings.ToList();
        if (deviceMappings.Count == 0)
            return (0, 0);

        var timestamp = DateTimeOffset.UtcNow;
        var records = 0;

        // Group by Huawei device type for batched API calls
        var byDevType = deviceMappings.GroupBy(d => d.HuaweiDevTypeId);

        foreach (var group in byDevType)
        {
            var devTypeId = group.Key;
            var devIds = group.Select(d => d.HuaweiDeviceId).Distinct().ToList();

            List<Application.Common.Models.Huawei.HuaweiDevRealKpiData> kpiData;
            try
            {
                kpiData = await _apiClient.GetDevRealKpiAsync(account.BaseUrl, token, devIds, devTypeId, ct);
            }
            catch (HuaweiTokenExpiredException) { throw; }
            catch (HuaweiRateLimitException) { throw; }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to get real-time KPI for devTypeId={DevTypeId}, station={StationCode}",
                    devTypeId, station.StationCode);

                LogRawData(account.Id, "getDevRealKpi",
                    JsonSerializer.Serialize(new { devIds = string.Join(",", devIds), devTypeId }),
                    JsonSerializer.Serialize(new { error = ex.Message }),
                    0);
                throw;
            }

            // Log raw response for audit
            LogRawData(account.Id, "getDevRealKpi",
                JsonSerializer.Serialize(new { devIds = string.Join(",", devIds), devTypeId }),
                JsonSerializer.Serialize(kpiData),
                200);

            foreach (var kpi in kpiData)
            {
                var mappings = deviceMappings.Where(d => d.HuaweiDeviceId == kpi.DevId).ToList();

                foreach (var mapping in mappings)
                {
                    records += TranslateAndStore(mapping, kpi, devTypeId, timestamp, deviceLookup);
                }
            }
        }

        return (deviceMappings.Count, records);
    }

    private int TranslateAndStore(
        HuaweiDeviceMapping mapping,
        Application.Common.Models.Huawei.HuaweiDevRealKpiData kpi,
        int devTypeId,
        DateTimeOffset timestamp,
        Dictionary<Guid, Domain.Entities.Device> deviceLookup)
    {
        var records = 0;

        switch (devTypeId)
        {
            case DevTypeBattery:
            {
                var batteryData = _translator.TranslateBattery(kpi, mapping.DeviceId, timestamp);
                _context.DeviceBatteryData.Add(batteryData);
                records++;
                break;
            }
            case DevTypeInverter:
            {
                // Inverter maps to either PCS or Solar device based on mapping
                deviceLookup.TryGetValue(mapping.DeviceId, out var device);
                var deviceTypeName = device?.DeviceType?.Name;

                if (deviceTypeName is "PCS" or "Inverter")
                {
                    var pcsData = _translator.TranslatePcs(kpi, mapping.DeviceId, timestamp);
                    _context.DevicePcsData.Add(pcsData);
                    records++;
                }
                else if (deviceTypeName == "Solar")
                {
                    var solarData = _translator.TranslateSolar(kpi, mapping.DeviceId, timestamp);
                    _context.DeviceSolarData.Add(solarData);
                    records++;
                }
                else
                {
                    _logger.LogWarning("Unknown device type '{DeviceType}' for mapping {MappingId}, device {DeviceId}",
                        deviceTypeName, mapping.Id, mapping.DeviceId);
                }
                break;
            }
            default:
                _logger.LogDebug("Skipping unsupported devTypeId={DevTypeId} for mapping {MappingId}",
                    devTypeId, mapping.Id);
                break;
        }

        // Update LastSeenUtc on the device
        if (deviceLookup.TryGetValue(mapping.DeviceId, out var dev))
        {
            dev.LastSeenUtc = timestamp;
        }

        return records;
    }

    private void LogRawData(Guid accountId, string endpoint, string? request, string? response, int statusCode)
    {
        var log = new HuaweiRawDataLog
        {
            HuaweiAccountId = accountId,
            Endpoint = endpoint,
            RequestPayload = request,
            ResponsePayload = response,
            HttpStatusCode = statusCode,
            CollectedAtUtc = DateTimeOffset.UtcNow
        };
        _context.HuaweiRawDataLogs.Add(log);
    }

    private void LogSyncResult(Guid accountId, string syncType, string status, int deviceCount, int recordsInserted, string? errorMessage)
    {
        var log = new HuaweiSyncLog
        {
            HuaweiAccountId = accountId,
            SyncType = syncType,
            Status = status,
            DeviceCount = deviceCount,
            RecordsInserted = recordsInserted,
            ErrorMessage = errorMessage,
            StartedAtUtc = DateTimeOffset.UtcNow,
            CompletedAtUtc = DateTimeOffset.UtcNow
        };
        _context.HuaweiSyncLogs.Add(log);
    }
}
