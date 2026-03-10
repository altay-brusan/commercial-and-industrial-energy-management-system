using Backend.Application.Common.Interfaces;
using Backend.Application.Common.Security;
using Backend.Application.Common.Services;
using Backend.Domain.Constants;

namespace Backend.Application.Dashboard.Queries.GetCostSavings;

public record CostSavingsChartDto
{
    public string Period { get; init; } = string.Empty;
    public decimal? TotalCost { get; init; }
    public decimal? TotalSavings { get; init; }
    public string? Currency { get; init; }
    public List<CostSavingsDataPoint> DataPoints { get; init; } = new();
}

public record CostSavingsDataPoint
{
    public DateTimeOffset Time { get; init; }
    public decimal? CostAmount { get; init; }
    public decimal? SavingsAmount { get; init; }
}

[Authorize(Roles = "Administrator,Tenant.Admin")]
[RequirePermission(Backend.Domain.Constants.Permissions.DashboardView)]
public record GetCostSavingsQuery : IRequest<CostSavingsChartDto>
{
    public Guid FacilityId { get; init; }
    public string? Period { get; init; }
}

public class GetCostSavingsQueryHandler : IRequestHandler<GetCostSavingsQuery, CostSavingsChartDto>
{
    private readonly IApplicationDbContext _context;

    public GetCostSavingsQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<CostSavingsChartDto> Handle(GetCostSavingsQuery request, CancellationToken ct)
    {
        var facilityId = request.FacilityId;
        var (start, end) = DashboardHelpers.GetDateRange(request.Period);
        var periodLabel = request.Period?.ToLowerInvariant() ?? "today";

        // Load tariff
        var facilityTariff = await _context.FacilityTariffs
            .Where(ft => ft.FacilityId == facilityId && ft.IsActive)
            .Include(ft => ft.Tariff)
            .FirstOrDefaultAsync(ct);

        decimal ratePerKWh;
        string currency;
        if (facilityTariff?.Tariff != null)
        {
            ratePerKWh = DashboardHelpers.ComputeRatePerKWh(facilityTariff.Tariff);
            currency = facilityTariff.Tariff.Currency;
        }
        else
        {
            ratePerKWh = DashboardHelpers.DefaultRatePerKWh;
            currency = DashboardHelpers.DefaultCurrency;
        }

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

        // Grid consumption per hour
        var gridByHour = new Dictionary<DateTimeOffset, List<decimal>>();

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
                if (!p.HasValue || p.Value <= 0m) continue;
                var bucket = DashboardHelpers.HourBucket(entry.DeviceLocalTime);
                if (!gridByHour.TryGetValue(bucket, out var list)) { list = new List<decimal>(); gridByHour[bucket] = list; }
                list.Add(p.Value);
            }
        }

        // Solar production per hour → cost savings
        var solarByHour = new Dictionary<DateTimeOffset, List<decimal>>();

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
                if (!p.HasValue || p.Value <= 0m) continue;
                var bucket = DashboardHelpers.HourBucket(entry.DeviceLocalTime);
                if (!solarByHour.TryGetValue(bucket, out var list)) { list = new List<decimal>(); solarByHour[bucket] = list; }
                list.Add(p.Value);
            }
        }

        var allBuckets = gridByHour.Keys.Union(solarByHour.Keys).OrderBy(t => t).ToList();

        decimal totalCost = 0m;
        decimal totalSavings = 0m;

        var dataPoints = allBuckets.Select(bucket =>
        {
            // Average kW for the hour ≈ kWh consumed/produced in that hour
            decimal? gridKWh = gridByHour.TryGetValue(bucket, out var gList) && gList.Count > 0
                ? Math.Round(gList.Average(), 4) : null;
            decimal? solarKWh = solarByHour.TryGetValue(bucket, out var sList) && sList.Count > 0
                ? Math.Round(sList.Average(), 4) : null;

            decimal? costAmount = gridKWh.HasValue
                ? Math.Round(gridKWh.Value * ratePerKWh, 4) : null;
            decimal? savingsAmount = solarKWh.HasValue
                ? Math.Round(solarKWh.Value * ratePerKWh, 4) : null;

            if (costAmount.HasValue) totalCost += costAmount.Value;
            if (savingsAmount.HasValue) totalSavings += savingsAmount.Value;

            return new CostSavingsDataPoint
            {
                Time = bucket,
                CostAmount = costAmount,
                SavingsAmount = savingsAmount
            };
        }).ToList();

        return new CostSavingsChartDto
        {
            Period = periodLabel,
            TotalCost = totalCost > 0m ? Math.Round(totalCost, 2) : null,
            TotalSavings = totalSavings > 0m ? Math.Round(totalSavings, 2) : null,
            Currency = currency,
            DataPoints = dataPoints
        };
    }
}
