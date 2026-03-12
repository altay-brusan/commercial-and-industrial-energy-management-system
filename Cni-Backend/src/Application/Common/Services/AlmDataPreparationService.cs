using System.Globalization;
using System.Text;
using System.Text.Json;
using Backend.Application.Common.Interfaces;
using Backend.Application.Common.Models.Alm;
using Backend.Domain.Constants;
using Backend.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Backend.Application.Common.Services;

public record AlmPreparedData(JsonElement Schema, string CsvData, int DataPointCount);

public class AlmDataPreparationService
{
    private readonly IApplicationDbContext _context;
    private readonly ILogger<AlmDataPreparationService> _logger;

    public AlmDataPreparationService(IApplicationDbContext context, ILogger<AlmDataPreparationService> logger)
    {
        _context = context;
        _logger = logger;
    }

    /// <summary>
    /// Dispatches data preparation to the appropriate method based on job type.
    /// New modules: add a case here and implement the corresponding PrepareXxxDataAsync method.
    /// </summary>
    public Task<AlmPreparedData?> PrepareDataAsync(Guid facilityId, AlmJobType jobType, int dataDays, CancellationToken ct)
    {
        return jobType switch
        {
            AlmJobType.PowerConsumption => PreparePowerConsumptionDataAsync(facilityId, dataDays, ct),
            AlmJobType.PowerGeneration => PreparePowerGenerationDataAsync(facilityId, dataDays, ct),
            _ => throw new ArgumentOutOfRangeException(nameof(jobType), jobType, "Unsupported ALM job type for data preparation")
        };
    }

    /// <summary>
    /// Prepares power consumption (PC) data: hourly SmartMeter data.
    /// Returns null if insufficient data.
    /// </summary>
    public async Task<AlmPreparedData?> PreparePowerConsumptionDataAsync(
        Guid facilityId, int dataDays, CancellationToken ct)
    {
        var facility = await _context.Facilities
            .AsNoTracking()
            .Where(f => f.Id == facilityId)
            .Select(f => new { f.Id, f.Latitude, f.Longitude })
            .FirstOrDefaultAsync(ct);

        if (facility == null)
        {
            _logger.LogWarning("Facility {FacilityId} not found for PC data preparation", facilityId);
            return null;
        }

        // Find SmartMeter device IDs for this facility
        var meterDeviceIds = await GetDeviceIdsByType(facilityId, DeviceTypeNames.SmartMeter, "Grid", ct);
        if (meterDeviceIds.Count == 0)
        {
            _logger.LogWarning("No SmartMeter devices found for facility {FacilityId}", facilityId);
            return null;
        }

        var endUtc = DateTimeOffset.UtcNow;
        var startUtc = endUtc.AddDays(-dataDays);

        var rawData = await _context.DeviceSmartMeterData
            .AsNoTracking()
            .Where(d => meterDeviceIds.Contains(d.DeviceId) &&
                        d.DeviceLocalTime >= startUtc &&
                        d.DeviceLocalTime <= endUtc)
            .OrderBy(d => d.DeviceLocalTime)
            .Select(d => new { d.DeviceLocalTime, d.Payload })
            .ToListAsync(ct);

        if (rawData.Count == 0)
        {
            _logger.LogWarning("No SmartMeter data found for facility {FacilityId} in last {Days} days",
                facilityId, dataDays);
            return null;
        }

        // Aggregate into hourly buckets (sum activePower across meters)
        var hourlyBuckets = new SortedDictionary<DateTimeOffset, double>();
        foreach (var entry in rawData)
        {
            var power = ExtractDouble(entry.Payload, "activePower");
            if (power == null) continue;

            var bucket = HourBucket(entry.DeviceLocalTime);
            hourlyBuckets.TryAdd(bucket, 0);
            hourlyBuckets[bucket] += power.Value;
        }

        if (hourlyBuckets.Count < 24)
        {
            _logger.LogWarning("Insufficient hourly data points ({Count}) for facility {FacilityId}",
                hourlyBuckets.Count, facilityId);
            return null;
        }

        // Build CSV
        var csv = new StringBuilder();
        csv.AppendLine("UTCTime,LoadTotalPowerW");
        foreach (var (timestamp, totalPower) in hourlyBuckets)
        {
            csv.AppendLine($"{timestamp:yyyy-MM-dd HH:mm:ss},{totalPower.ToString("F2", CultureInfo.InvariantCulture)}");
        }

        // Build schema
        var schema = new AlmPcSchema
        {
            Uid = facilityId.ToString(),
            Latitude = facility.Latitude,
            Longitude = facility.Longitude
        };
        var schemaElement = JsonSerializer.SerializeToElement(schema);

        return new AlmPreparedData(schemaElement, csv.ToString(), hourlyBuckets.Count);
    }

    /// <summary>
    /// Prepares power generation (PG) data: 1 day of PV Solar data.
    /// Returns null if insufficient data.
    /// </summary>
    public async Task<AlmPreparedData?> PreparePowerGenerationDataAsync(
        Guid facilityId, int dataDays, CancellationToken ct)
    {
        var facility = await _context.Facilities
            .AsNoTracking()
            .Where(f => f.Id == facilityId)
            .Select(f => new { f.Id, f.Latitude, f.Longitude })
            .FirstOrDefaultAsync(ct);

        if (facility == null)
        {
            _logger.LogWarning("Facility {FacilityId} not found for PG data preparation", facilityId);
            return null;
        }

        // Find Solar device IDs for this facility
        var solarDeviceIds = await GetDeviceIdsByType(facilityId, DeviceTypeNames.Solar, "Solar", ct);
        if (solarDeviceIds.Count == 0)
        {
            _logger.LogWarning("No Solar devices found for facility {FacilityId}", facilityId);
            return null;
        }

        var endUtc = DateTimeOffset.UtcNow;
        var startUtc = endUtc.AddDays(-dataDays);

        // Query solar data per device
        var rawData = await _context.DeviceSolarData
            .AsNoTracking()
            .Where(d => solarDeviceIds.Contains(d.DeviceId) &&
                        d.DeviceLocalTime >= startUtc &&
                        d.DeviceLocalTime <= endUtc)
            .OrderBy(d => d.DeviceLocalTime)
            .Select(d => new { d.DeviceId, d.DeviceLocalTime, d.Payload })
            .ToListAsync(ct);

        if (rawData.Count == 0)
        {
            _logger.LogWarning("No Solar data found for facility {FacilityId} in last {Days} days",
                facilityId, dataDays);
            return null;
        }

        // Build per-device time-series, then pivot into CSV
        // Column names: UTCTime, PV1W, PV2W, ...
        var deviceOrder = solarDeviceIds.Select((id, idx) => (id, idx)).ToDictionary(x => x.id, x => x.idx);
        var pvColumnNames = solarDeviceIds.Select((_, idx) => $"PV{idx + 1}W").ToList();

        // Group by timestamp bucket, collect power per device
        var timeBuckets = new SortedDictionary<DateTimeOffset, double?[]>();
        foreach (var entry in rawData)
        {
            var power = ExtractDouble(entry.Payload, "activePower");
            var bucket = HourBucket(entry.DeviceLocalTime);
            if (!timeBuckets.TryGetValue(bucket, out var values))
            {
                values = new double?[solarDeviceIds.Count];
                timeBuckets[bucket] = values;
            }

            if (deviceOrder.TryGetValue(entry.DeviceId, out var idx) && power.HasValue)
            {
                values[idx] = (values[idx] ?? 0) + power.Value;
            }
        }

        if (timeBuckets.Count < 4)
        {
            _logger.LogWarning("Insufficient solar data points ({Count}) for facility {FacilityId}",
                timeBuckets.Count, facilityId);
            return null;
        }

        // Build CSV
        var csv = new StringBuilder();
        csv.AppendLine("UTCTime," + string.Join(",", pvColumnNames));
        foreach (var (timestamp, values) in timeBuckets)
        {
            csv.Append(timestamp.ToString("yyyy-MM-dd HH:mm:ss"));
            foreach (var val in values)
            {
                csv.Append(',');
                csv.Append(val?.ToString("F2", CultureInfo.InvariantCulture) ?? "0.00");
            }
            csv.AppendLine();
        }

        // Build schema
        var schema = new AlmPgSchema
        {
            Uid = facilityId.ToString(),
            Latitude = facility.Latitude,
            Longitude = facility.Longitude,
            PvPower = pvColumnNames
        };
        var schemaElement = JsonSerializer.SerializeToElement(schema);

        return new AlmPreparedData(schemaElement, csv.ToString(), timeBuckets.Count);
    }

    private async Task<List<Guid>> GetDeviceIdsByType(
        Guid facilityId, string deviceTypeName, string logicalDeviceCategory, CancellationToken ct)
    {
        return await _context.Devices
            .AsNoTracking()
            .Where(d => d.Rtu != null && d.Rtu.FacilityId == facilityId &&
                        ((d.DeviceType != null && d.DeviceType.Name == deviceTypeName) ||
                         (d.LogicalDevice != null && d.LogicalDevice.Category == logicalDeviceCategory)))
            .Select(d => d.Id)
            .ToListAsync(ct);
    }

    private static double? ExtractDouble(string? payload, string path)
    {
        if (string.IsNullOrWhiteSpace(payload) || payload == "{}") return null;
        try
        {
            using var doc = JsonDocument.Parse(payload);
            var val = PayloadDeserializer.ExtractValue(doc.RootElement, path);
            return val is double d ? d : null;
        }
        catch
        {
            return null;
        }
    }

    private static DateTimeOffset HourBucket(DateTimeOffset time)
    {
        var utc = time.ToUniversalTime();
        return new DateTimeOffset(utc.Year, utc.Month, utc.Day, utc.Hour, 0, 0, TimeSpan.Zero);
    }
}
