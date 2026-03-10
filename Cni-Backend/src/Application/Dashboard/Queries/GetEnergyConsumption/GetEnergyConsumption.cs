using Backend.Application.Common.Interfaces;
using Backend.Application.Common.Security;
using Backend.Application.Common.Services;
using Backend.Domain.Constants;

namespace Backend.Application.Dashboard.Queries.GetEnergyConsumption;

public record EnergyConsumptionChartDto
{
    public string Period { get; init; } = string.Empty;
    public List<EnergyConsumptionDataPoint> DataPoints { get; init; } = new();
}

public record EnergyConsumptionDataPoint
{
    public DateTimeOffset Time { get; init; }
    public decimal? ConsumptionKW { get; init; }
    public decimal? ProductionKW { get; init; }
}

[Authorize(Roles = "Administrator,Tenant.Admin")]
[RequirePermission(Backend.Domain.Constants.Permissions.DashboardView)]
public record GetEnergyConsumptionQuery : IRequest<EnergyConsumptionChartDto>
{
    public Guid FacilityId { get; init; }
    public string? Period { get; init; }
}

public class GetEnergyConsumptionQueryHandler : IRequestHandler<GetEnergyConsumptionQuery, EnergyConsumptionChartDto>
{
    private readonly IApplicationDbContext _context;

    public GetEnergyConsumptionQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<EnergyConsumptionChartDto> Handle(GetEnergyConsumptionQuery request, CancellationToken ct)
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

        // Consumption data (grid import power per hour)
        var consumptionByHour = new Dictionary<DateTimeOffset, List<decimal>>();

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
                if (!p.HasValue) continue;

                var bucket = DashboardHelpers.HourBucket(entry.DeviceLocalTime);
                if (!consumptionByHour.TryGetValue(bucket, out var list))
                {
                    list = new List<decimal>();
                    consumptionByHour[bucket] = list;
                }
                list.Add(p.Value);
            }
        }

        // Production data (solar power per hour)
        var productionByHour = new Dictionary<DateTimeOffset, List<decimal>>();

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
                if (!p.HasValue) continue;

                var bucket = DashboardHelpers.HourBucket(entry.DeviceLocalTime);
                if (!productionByHour.TryGetValue(bucket, out var list))
                {
                    list = new List<decimal>();
                    productionByHour[bucket] = list;
                }
                list.Add(p.Value);
            }
        }

        // Merge all hour buckets
        var allBuckets = consumptionByHour.Keys.Union(productionByHour.Keys).OrderBy(t => t).ToList();

        var dataPoints = allBuckets.Select(bucket => new EnergyConsumptionDataPoint
        {
            Time = bucket,
            ConsumptionKW = consumptionByHour.TryGetValue(bucket, out var cList) && cList.Count > 0
                ? Math.Round(cList.Average(), 2)
                : null,
            ProductionKW = productionByHour.TryGetValue(bucket, out var pList) && pList.Count > 0
                ? Math.Round(pList.Average(), 2)
                : null
        }).ToList();

        return new EnergyConsumptionChartDto
        {
            Period = periodLabel,
            DataPoints = dataPoints
        };
    }
}
