using Backend.Application.Common.Interfaces;
using Backend.Application.Common.Security;
using Backend.Application.Common.Services;
using Backend.Domain.Constants;

namespace Backend.Application.Dashboard.Queries.GetCo2Emissions;

public record Co2EmissionsDto
{
    public string Period { get; init; } = string.Empty;
    public decimal? TotalEmissionsKg { get; init; }
    public decimal? TotalSavedKg { get; init; }
    public decimal EmissionFactorKgPerKWh { get; init; } = DashboardHelpers.Co2FactorKgPerKWh;
    public List<Co2DataPoint> DataPoints { get; init; } = new();
}

public record Co2DataPoint
{
    public DateTimeOffset Time { get; init; }
    public decimal? EmissionsKg { get; init; }
    public decimal? SavedKg { get; init; }
}

[Authorize(Roles = "Administrator,Tenant.Admin")]
[RequirePermission(Backend.Domain.Constants.Permissions.DashboardView)]
public record GetCo2EmissionsQuery : IRequest<Co2EmissionsDto>
{
    public Guid FacilityId { get; init; }
    public string? Period { get; init; }
}

public class GetCo2EmissionsQueryHandler : IRequestHandler<GetCo2EmissionsQuery, Co2EmissionsDto>
{
    private readonly IApplicationDbContext _context;

    public GetCo2EmissionsQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Co2EmissionsDto> Handle(GetCo2EmissionsQuery request, CancellationToken ct)
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

        // Grid power per hour → emissions
        var gridByHour = new Dictionary<DateTimeOffset, List<decimal>>();

        if (meterIds.Count > 0)
        {
            var meterData = await _context.DeviceSmartMeterData
                .Where(d => meterIds.Contains(d.DeviceId) && d.DeviceLocalTime >= start && d.DeviceLocalTime <= end)
                .OrderBy(d => d.DeviceLocalTime)
                .ToListAsync(ct);

            foreach (var entry in meterData)
            {
                var el = PayloadDeserializer.GetElement(entry.Payload);
                var p = DashboardHelpers.GetDecimal(el, "activePower");
                if (!p.HasValue || p.Value <= 0m) continue;

                var bucket = DashboardHelpers.HourBucket(entry.DeviceLocalTime);
                if (!gridByHour.TryGetValue(bucket, out var list))
                {
                    list = new List<decimal>();
                    gridByHour[bucket] = list;
                }
                list.Add(p.Value);
            }
        }

        // Solar power per hour → saved emissions
        var solarByHour = new Dictionary<DateTimeOffset, List<decimal>>();

        if (solarIds.Count > 0)
        {
            var solarData = await _context.DeviceSolarData
                .Where(d => solarIds.Contains(d.DeviceId) && d.DeviceLocalTime >= start && d.DeviceLocalTime <= end)
                .OrderBy(d => d.DeviceLocalTime)
                .ToListAsync(ct);

            foreach (var entry in solarData)
            {
                var el = PayloadDeserializer.GetElement(entry.Payload);
                var p = DashboardHelpers.GetDecimal(el, "activePower");
                if (!p.HasValue || p.Value <= 0m) continue;

                var bucket = DashboardHelpers.HourBucket(entry.DeviceLocalTime);
                if (!solarByHour.TryGetValue(bucket, out var list))
                {
                    list = new List<decimal>();
                    solarByHour[bucket] = list;
                }
                list.Add(p.Value);
            }
        }

        var allBuckets = gridByHour.Keys.Union(solarByHour.Keys).OrderBy(t => t).ToList();

        decimal totalEmissions = 0m;
        decimal totalSaved = 0m;

        var dataPoints = allBuckets.Select(bucket =>
        {
            // Average power (kW) for the hour ≈ energy (kWh) for 1-hour bucket
            decimal? gridKWh = gridByHour.TryGetValue(bucket, out var gList) && gList.Count > 0
                ? Math.Round(gList.Average(), 4)
                : null;

            decimal? solarKWh = solarByHour.TryGetValue(bucket, out var sList) && sList.Count > 0
                ? Math.Round(sList.Average(), 4)
                : null;

            decimal? emissionsKg = gridKWh.HasValue
                ? Math.Round(gridKWh.Value * DashboardHelpers.Co2FactorKgPerKWh, 4)
                : null;

            decimal? savedKg = solarKWh.HasValue
                ? Math.Round(solarKWh.Value * DashboardHelpers.Co2FactorKgPerKWh, 4)
                : null;

            if (emissionsKg.HasValue) totalEmissions += emissionsKg.Value;
            if (savedKg.HasValue) totalSaved += savedKg.Value;

            return new Co2DataPoint
            {
                Time = bucket,
                EmissionsKg = emissionsKg,
                SavedKg = savedKg
            };
        }).ToList();

        return new Co2EmissionsDto
        {
            Period = periodLabel,
            TotalEmissionsKg = totalEmissions > 0m ? Math.Round(totalEmissions, 2) : null,
            TotalSavedKg = totalSaved > 0m ? Math.Round(totalSaved, 2) : null,
            DataPoints = dataPoints
        };
    }
}
