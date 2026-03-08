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

    public async Task SyncAllAccounts()
    {
        var accounts = await _context.HuaweiAccounts
            .Include(a => a.StationMappings)
                .ThenInclude(s => s.DeviceMappings)
            .Where(a => a.IsActive)
            .IgnoreQueryFilters()
            .ToListAsync();

        if (accounts.Count == 0)
        {
            _logger.LogDebug("No active Huawei accounts found, skipping sync");
            return;
        }

        _logger.LogInformation("Starting Huawei FusionSolar sync for {Count} account(s)", accounts.Count);
        var errors = new List<string>();

        foreach (var account in accounts)
        {
            try
            {
                await SyncAccountAsync(account);
            }
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

    private async Task SyncAccountAsync(HuaweiAccount account)
    {
        var enabledStations = account.StationMappings
            .Where(s => s.SyncEnabled)
            .ToList();

        if (enabledStations.Count == 0)
        {
            _logger.LogDebug("No enabled stations for account {AccountId}", account.Id);
            return;
        }

        string token;
        try
        {
            token = await _tokenManager.GetTokenAsync(account);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get token for account {AccountId}", account.Id);
            await LogSyncResult(account.Id, "RealTimeKpi", "Failed", 0, 0, $"Login failed: {ex.Message}");
            throw;
        }

        var syncStartedAt = DateTimeOffset.UtcNow;
        var totalRecords = 0;
        var totalDevices = 0;

        foreach (var station in enabledStations)
        {
            try
            {
                var (devices, records) = await SyncStationAsync(account, station, token);
                totalDevices += devices;
                totalRecords += records;
                station.LastSyncUtc = DateTimeOffset.UtcNow;
            }
            catch (HuaweiTokenExpiredException)
            {
                _logger.LogWarning("Token expired during sync, refreshing for account {AccountId}", account.Id);
                _tokenManager.InvalidateToken(account.Id);
                token = await _tokenManager.RefreshTokenAsync(account);

                var (devices, records) = await SyncStationAsync(account, station, token);
                totalDevices += devices;
                totalRecords += records;
                station.LastSyncUtc = DateTimeOffset.UtcNow;
            }
        }

        await _context.SaveChangesAsync(CancellationToken.None);
        await LogSyncResult(account.Id, "RealTimeKpi", "Success", totalDevices, totalRecords, null);

        _logger.LogInformation(
            "Huawei sync completed for account {AccountId}: {Devices} devices, {Records} records in {Elapsed:F1}s",
            account.Id, totalDevices, totalRecords,
            (DateTimeOffset.UtcNow - syncStartedAt).TotalSeconds);
    }

    private async Task<(int devices, int records)> SyncStationAsync(
        HuaweiAccount account, HuaweiStationMapping station, string token)
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
            var devIds = group.Select(d => d.HuaweiDeviceId).ToList();

            List<Application.Common.Models.Huawei.HuaweiDevRealKpiData> kpiData;
            try
            {
                kpiData = await _apiClient.GetDevRealKpiAsync(account.BaseUrl, token, devIds, devTypeId);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to get real-time KPI for devTypeId={DevTypeId}, station={StationCode}",
                    devTypeId, station.StationCode);

                // Log raw request/error
                await LogRawData(account.Id, "getDevRealKpi",
                    JsonSerializer.Serialize(new { devIds = string.Join(",", devIds), devTypeId }),
                    JsonSerializer.Serialize(new { error = ex.Message }),
                    0);
                throw;
            }

            // Log raw response for audit
            await LogRawData(account.Id, "getDevRealKpi",
                JsonSerializer.Serialize(new { devIds = string.Join(",", devIds), devTypeId }),
                JsonSerializer.Serialize(kpiData),
                200);

            foreach (var kpi in kpiData)
            {
                var mappings = deviceMappings.Where(d => d.HuaweiDeviceId == kpi.DevId).ToList();

                foreach (var mapping in mappings)
                {
                    records += await TranslateAndStore(mapping, kpi, devTypeId, timestamp);
                }
            }
        }

        return (deviceMappings.Count, records);
    }

    private async Task<int> TranslateAndStore(
        HuaweiDeviceMapping mapping,
        Application.Common.Models.Huawei.HuaweiDevRealKpiData kpi,
        int devTypeId,
        DateTimeOffset timestamp)
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
                // Check what type of CNI device this mapping points to
                var device = await _context.Devices
                    .Include(d => d.DeviceType)
                    .FirstOrDefaultAsync(d => d.Id == mapping.DeviceId);

                if (device?.DeviceType?.Name == "PCS" || device?.DeviceType?.Name == "Inverter")
                {
                    var pcsData = _translator.TranslatePcs(kpi, mapping.DeviceId, timestamp);
                    _context.DevicePcsData.Add(pcsData);
                    records++;
                }
                else if (device?.DeviceType?.Name == "Solar")
                {
                    var solarData = _translator.TranslateSolar(kpi, mapping.DeviceId, timestamp);
                    _context.DeviceSolarData.Add(solarData);
                    records++;
                }
                else
                {
                    _logger.LogWarning("Unknown device type for mapping {MappingId}, device {DeviceId}",
                        mapping.Id, mapping.DeviceId);
                }
                break;
            }
            default:
                _logger.LogDebug("Skipping unsupported devTypeId={DevTypeId} for mapping {MappingId}",
                    devTypeId, mapping.Id);
                break;
        }

        // Update LastSeenUtc on the device
        await UpdateLastSeen(mapping.DeviceId, timestamp);

        return records;
    }

    private async Task UpdateLastSeen(Guid deviceId, DateTimeOffset timestamp)
    {
        var device = await _context.Devices.FindAsync(deviceId);
        if (device != null)
        {
            device.LastSeenUtc = timestamp;
        }
    }

    private async Task LogRawData(Guid accountId, string endpoint, string? request, string? response, int statusCode)
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

        // Don't await SaveChanges here — it's batched with telemetry inserts
        await Task.CompletedTask;
    }

    private async Task LogSyncResult(Guid accountId, string syncType, string status, int deviceCount, int recordsInserted, string? errorMessage)
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
        await _context.SaveChangesAsync(CancellationToken.None);
    }
}
