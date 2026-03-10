using Backend.Application.Common.Interfaces;
using Backend.Application.Common.Security;
using Backend.Application.Common.Services;
using Backend.Domain.Constants;

namespace Backend.Application.Dashboard.Queries.GetEnergyUsage;

public record EnergyUsageDto
{
    public decimal? TotalEnergyKWh { get; init; }
    public decimal? ActivePowerKW { get; init; }
    public decimal? ReactivePowerKVAR { get; init; }
    public List<EnergySourceBreakdownDto> Sources { get; init; } = new();
}

public record EnergySourceBreakdownDto
{
    public string Name { get; init; } = string.Empty;
    public decimal? EnergyKWh { get; init; }
    public decimal? Percentage { get; init; }
}

[Authorize(Roles = "Administrator,Tenant.Admin")]
[RequirePermission(Backend.Domain.Constants.Permissions.DashboardView)]
public record GetEnergyUsageQuery : IRequest<EnergyUsageDto>
{
    public Guid FacilityId { get; init; }
}

public class GetEnergyUsageQueryHandler : IRequestHandler<GetEnergyUsageQuery, EnergyUsageDto>
{
    private readonly IApplicationDbContext _context;

    public GetEnergyUsageQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<EnergyUsageDto> Handle(GetEnergyUsageQuery request, CancellationToken ct)
    {
        var facilityId = request.FacilityId;

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

        decimal? totalEnergy = null;
        decimal? activePower = null;
        decimal? reactivePower = null;
        decimal? gridEnergy = null;
        decimal? solarEnergy = null;
        decimal? bessEnergy = null;

        // SmartMeter → grid consumption
        if (meterIds.Count > 0)
        {
            var latest = await _context.DeviceSmartMeterData
                .Where(d => meterIds.Contains(d.DeviceId))
                .GroupBy(d => d.DeviceId)
                .Select(g => g.OrderByDescending(d => d.DeviceLocalTime).First())
                .ToListAsync(ct);

            foreach (var entry in latest)
            {
                var el = PayloadDeserializer.GetElement(entry.Payload);
                var energy = DashboardHelpers.GetDecimal(el, "importToday");
                var ap = DashboardHelpers.GetDecimal(el, "activePower");
                var rp = DashboardHelpers.GetDecimal(el, "reactivePower");

                if (energy.HasValue) gridEnergy = (gridEnergy ?? 0m) + energy.Value;
                if (ap.HasValue) activePower = (activePower ?? 0m) + ap.Value;
                if (rp.HasValue) reactivePower = (reactivePower ?? 0m) + rp.Value;
            }
        }

        // Solar → renewable production
        if (solarIds.Count > 0)
        {
            var latest = await _context.DeviceSolarData
                .Where(d => solarIds.Contains(d.DeviceId))
                .GroupBy(d => d.DeviceId)
                .Select(g => g.OrderByDescending(d => d.DeviceLocalTime).First())
                .ToListAsync(ct);

            foreach (var entry in latest)
            {
                var el = PayloadDeserializer.GetElement(entry.Payload);
                var energy = DashboardHelpers.GetDecimal(el, "dailyYield");
                if (energy.HasValue) solarEnergy = (solarEnergy ?? 0m) + energy.Value;
            }
        }

        // Battery → BESS contribution
        if (batteryIds.Count > 0)
        {
            var latest = await _context.DeviceBatteryData
                .Where(d => batteryIds.Contains(d.DeviceId))
                .GroupBy(d => d.DeviceId)
                .Select(g => g.OrderByDescending(d => d.DeviceLocalTime).First())
                .ToListAsync(ct);

            foreach (var entry in latest)
            {
                var el = PayloadDeserializer.GetElement(entry.Payload);
                var energy = DashboardHelpers.GetDecimal(el, "remainingEnergy");
                if (energy.HasValue) bessEnergy = (bessEnergy ?? 0m) + energy.Value;
            }
        }

        totalEnergy = (gridEnergy ?? 0m) + (solarEnergy ?? 0m) + (bessEnergy ?? 0m);
        if (totalEnergy == 0m) totalEnergy = null;

        var sources = new List<EnergySourceBreakdownDto>();
        decimal total = totalEnergy ?? 1m;

        if (gridEnergy.HasValue)
            sources.Add(new EnergySourceBreakdownDto
            {
                Name = "Grid",
                EnergyKWh = gridEnergy,
                Percentage = total > 0m ? Math.Round(gridEnergy.Value / total * 100m, 1) : null
            });

        if (solarEnergy.HasValue)
            sources.Add(new EnergySourceBreakdownDto
            {
                Name = "Solar",
                EnergyKWh = solarEnergy,
                Percentage = total > 0m ? Math.Round(solarEnergy.Value / total * 100m, 1) : null
            });

        if (bessEnergy.HasValue)
            sources.Add(new EnergySourceBreakdownDto
            {
                Name = "BESS",
                EnergyKWh = bessEnergy,
                Percentage = total > 0m ? Math.Round(bessEnergy.Value / total * 100m, 1) : null
            });

        return new EnergyUsageDto
        {
            TotalEnergyKWh = totalEnergy,
            ActivePowerKW = activePower,
            ReactivePowerKVAR = reactivePower,
            Sources = sources
        };
    }
}
