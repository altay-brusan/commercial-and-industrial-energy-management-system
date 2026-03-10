using Backend.Application.Common.Interfaces;
using Backend.Application.Common.Security;
using Backend.Application.Common.Services;
using Backend.Domain.Constants;

namespace Backend.Application.Dashboard.Queries.GetSourceDistribution;

public record SourceDistributionChartDto
{
    public string Period { get; init; } = string.Empty;
    public List<SourceDistributionDataPoint> DataPoints { get; init; } = new();
}

public record SourceDistributionDataPoint
{
    public DateTimeOffset Time { get; init; }
    public decimal? GridKW { get; init; }
    public decimal? SolarKW { get; init; }
    public decimal? BessKW { get; init; }
}

[Authorize(Roles = "Administrator,Tenant.Admin")]
[RequirePermission(Backend.Domain.Constants.Permissions.DashboardView)]
public record GetSourceDistributionQuery : IRequest<SourceDistributionChartDto>
{
    public Guid FacilityId { get; init; }
    public string? Period { get; init; }
}

public class GetSourceDistributionQueryHandler : IRequestHandler<GetSourceDistributionQuery, SourceDistributionChartDto>
{
    private readonly IApplicationDbContext _context;

    public GetSourceDistributionQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<SourceDistributionChartDto> Handle(GetSourceDistributionQuery request, CancellationToken ct)
    {
        var facilityId = request.FacilityId;
        var (start, end) = DashboardHelpers.GetDateRange(request.Period);
        var periodLabel = request.Period?.ToLowerInvariant() ?? "today";

        var devices = await _context.Devices
            .Where(d => d.Rtu != null && d.Rtu.FacilityId == facilityId)
            .Include(d => d.DeviceType)
            .ToListAsync(ct);

        var meterIds = devices
            .Where(d => d.DeviceType != null && d.DeviceType.Name == DeviceTypeNames.SmartMeter)
            .Select(d => d.Id).ToList();

        var solarIds = devices
            .Where(d => d.DeviceType != null && d.DeviceType.Name == DeviceTypeNames.Solar)
            .Select(d => d.Id).ToList();

        var batteryIds = devices
            .Where(d => d.DeviceType != null &&
                (d.DeviceType.Name == DeviceTypeNames.EnergyStorage ||
                 d.DeviceType.Name == DeviceTypeNames.EnergyStorageBattery))
            .Select(d => d.Id).ToList();

        var gridByHour = new Dictionary<DateTimeOffset, List<decimal>>();
        var solarByHour = new Dictionary<DateTimeOffset, List<decimal>>();
        var bessByHour = new Dictionary<DateTimeOffset, List<decimal>>();

        if (meterIds.Count > 0)
        {
            var data = await _context.DeviceSmartMeterData
                .Where(d => meterIds.Contains(d.DeviceId) && d.DeviceLocalTime >= start && d.DeviceLocalTime <= end)
                .OrderBy(d => d.DeviceLocalTime)
                .ToListAsync(ct);

            foreach (var entry in data)
            {
                var el = PayloadDeserializer.GetElement(entry.Payload);
                var p = DashboardHelpers.GetDecimal(el, "activePower");
                if (!p.HasValue) continue;
                var bucket = DashboardHelpers.HourBucket(entry.DeviceLocalTime);
                if (!gridByHour.TryGetValue(bucket, out var list)) { list = new List<decimal>(); gridByHour[bucket] = list; }
                list.Add(p.Value);
            }
        }

        if (solarIds.Count > 0)
        {
            var data = await _context.DeviceSolarData
                .Where(d => solarIds.Contains(d.DeviceId) && d.DeviceLocalTime >= start && d.DeviceLocalTime <= end)
                .OrderBy(d => d.DeviceLocalTime)
                .ToListAsync(ct);

            foreach (var entry in data)
            {
                var el = PayloadDeserializer.GetElement(entry.Payload);
                var p = DashboardHelpers.GetDecimal(el, "activePower");
                if (!p.HasValue) continue;
                var bucket = DashboardHelpers.HourBucket(entry.DeviceLocalTime);
                if (!solarByHour.TryGetValue(bucket, out var list)) { list = new List<decimal>(); solarByHour[bucket] = list; }
                list.Add(p.Value);
            }
        }

        if (batteryIds.Count > 0)
        {
            var data = await _context.DeviceBatteryData
                .Where(d => batteryIds.Contains(d.DeviceId) && d.DeviceLocalTime >= start && d.DeviceLocalTime <= end)
                .OrderBy(d => d.DeviceLocalTime)
                .ToListAsync(ct);

            foreach (var entry in data)
            {
                var el = PayloadDeserializer.GetElement(entry.Payload);
                var p = DashboardHelpers.GetDecimal(el, "power");
                if (!p.HasValue) continue;
                var bucket = DashboardHelpers.HourBucket(entry.DeviceLocalTime);
                if (!bessByHour.TryGetValue(bucket, out var list)) { list = new List<decimal>(); bessByHour[bucket] = list; }
                list.Add(p.Value);
            }
        }

        var allBuckets = gridByHour.Keys
            .Union(solarByHour.Keys)
            .Union(bessByHour.Keys)
            .OrderBy(t => t)
            .ToList();

        var dataPoints = allBuckets.Select(bucket => new SourceDistributionDataPoint
        {
            Time = bucket,
            GridKW = gridByHour.TryGetValue(bucket, out var gList) && gList.Count > 0
                ? Math.Round(gList.Average(), 2) : null,
            SolarKW = solarByHour.TryGetValue(bucket, out var sList) && sList.Count > 0
                ? Math.Round(sList.Average(), 2) : null,
            BessKW = bessByHour.TryGetValue(bucket, out var bList) && bList.Count > 0
                ? Math.Round(bList.Average(), 2) : null
        }).ToList();

        return new SourceDistributionChartDto
        {
            Period = periodLabel,
            DataPoints = dataPoints
        };
    }
}
