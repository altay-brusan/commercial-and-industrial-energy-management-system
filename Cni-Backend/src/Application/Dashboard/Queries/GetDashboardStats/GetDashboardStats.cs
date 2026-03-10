using Backend.Application.Common.Interfaces;
using Backend.Application.Common.Security;
using Backend.Application.Common.Services;
using Backend.Domain.Constants;

namespace Backend.Application.Dashboard.Queries.GetDashboardStats;

public record DashboardStatsDto
{
    public decimal? TotalEnergyKWh { get; init; }
    public decimal? CurrentPowerKW { get; init; }
    public decimal? DailyCostValue { get; init; }
    public string? Currency { get; init; }
    public decimal? BessSocPercent { get; init; }
}

[Authorize(Roles = "Administrator,Tenant.Admin")]
[RequirePermission(Backend.Domain.Constants.Permissions.DashboardView)]
public record GetDashboardStatsQuery : IRequest<DashboardStatsDto>
{
    public Guid FacilityId { get; init; }
}

public class GetDashboardStatsQueryHandler : IRequestHandler<GetDashboardStatsQuery, DashboardStatsDto>
{
    private readonly IApplicationDbContext _context;

    public GetDashboardStatsQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<DashboardStatsDto> Handle(GetDashboardStatsQuery request, CancellationToken ct)
    {
        var facilityId = request.FacilityId;

        // Resolve all device IDs for this facility
        var devices = await _context.Devices
            .Where(d => d.Rtu != null && d.Rtu.FacilityId == facilityId)
            .Include(d => d.DeviceType)
            .ToListAsync(ct);

        var meterIds = devices
            .Where(d => d.DeviceType != null && d.DeviceType.Name == DeviceTypeNames.SmartMeter)
            .Select(d => d.Id)
            .ToList();

        var batteryIds = devices
            .Where(d => d.DeviceType != null &&
                (d.DeviceType.Name == DeviceTypeNames.EnergyStorage ||
                 d.DeviceType.Name == DeviceTypeNames.EnergyStorageBattery))
            .Select(d => d.Id)
            .ToList();

        // Latest SmartMeter telemetry per device
        decimal? totalEnergyKWh = null;
        decimal? currentPowerKW = null;

        if (meterIds.Count > 0)
        {
            var latestMeter = await _context.DeviceSmartMeterData
                .Where(d => meterIds.Contains(d.DeviceId))
                .GroupBy(d => d.DeviceId)
                .Select(g => g.OrderByDescending(d => d.DeviceLocalTime).First())
                .ToListAsync(ct);

            foreach (var entry in latestMeter)
            {
                var el = PayloadDeserializer.GetElement(entry.Payload);
                var energy = DashboardHelpers.GetDecimal(el, "importToday");
                var power = DashboardHelpers.GetDecimal(el, "activePower");

                totalEnergyKWh = (totalEnergyKWh ?? 0m) + (energy ?? 0m);
                currentPowerKW = (currentPowerKW ?? 0m) + (power ?? 0m);
            }

            if (totalEnergyKWh == 0m) totalEnergyKWh = null;
            if (currentPowerKW == 0m) currentPowerKW = null;
        }

        // Latest Battery telemetry for SOC
        decimal? bessSoc = null;

        if (batteryIds.Count > 0)
        {
            var latestBattery = await _context.DeviceBatteryData
                .Where(d => batteryIds.Contains(d.DeviceId))
                .GroupBy(d => d.DeviceId)
                .Select(g => g.OrderByDescending(d => d.DeviceLocalTime).First())
                .ToListAsync(ct);

            decimal socSum = 0m;
            int socCount = 0;
            foreach (var entry in latestBattery)
            {
                var el = PayloadDeserializer.GetElement(entry.Payload);
                var soc = DashboardHelpers.GetDecimal(el, "soc");
                if (soc != null)
                {
                    socSum += soc.Value;
                    socCount++;
                }
            }

            if (socCount > 0)
                bessSoc = socSum / socCount;
        }

        // Daily cost: today's energy × tariff rate
        var todayStart = new DateTimeOffset(DateTimeOffset.UtcNow.Date, TimeSpan.Zero);
        decimal? dailyCost = null;
        string? currency = null;

        var facilityTariff = await _context.FacilityTariffs
            .Where(ft => ft.FacilityId == facilityId && ft.IsActive)
            .Include(ft => ft.Tariff)
            .FirstOrDefaultAsync(ct);

        decimal ratePerKWh;
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

        if (meterIds.Count > 0)
        {
            var todayMeter = await _context.DeviceSmartMeterData
                .Where(d => meterIds.Contains(d.DeviceId) && d.DeviceLocalTime >= todayStart)
                .OrderBy(d => d.DeviceLocalTime)
                .ToListAsync(ct);

            decimal todayEnergy = 0m;
            foreach (var entry in todayMeter)
            {
                var el = PayloadDeserializer.GetElement(entry.Payload);
                var energy = DashboardHelpers.GetDecimal(el, "importToday");
                if (energy.HasValue)
                    todayEnergy += energy.Value;
            }

            if (todayEnergy > 0m)
                dailyCost = todayEnergy * ratePerKWh;
        }

        return new DashboardStatsDto
        {
            TotalEnergyKWh = totalEnergyKWh,
            CurrentPowerKW = currentPowerKW,
            DailyCostValue = dailyCost,
            Currency = currency,
            BessSocPercent = bessSoc
        };
    }
}
