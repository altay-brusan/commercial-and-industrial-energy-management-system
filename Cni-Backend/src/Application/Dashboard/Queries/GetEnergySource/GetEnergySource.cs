using Backend.Application.Common.Interfaces;
using Backend.Application.Common.Security;
using Backend.Application.Common.Services;
using Backend.Domain.Constants;

namespace Backend.Application.Dashboard.Queries.GetEnergySource;

public record EnergySourcePowerDto
{
    public decimal? TotalPowerKW { get; init; }
    public List<PowerSourceDto> Sources { get; init; } = new();
}

public record PowerSourceDto
{
    public string Name { get; init; } = string.Empty;
    public decimal? PowerKW { get; init; }
    public decimal? Percentage { get; init; }
}

[Authorize(Roles = "Administrator,Tenant.Admin")]
[RequirePermission(Backend.Domain.Constants.Permissions.DashboardView)]
public record GetEnergySourceQuery : IRequest<EnergySourcePowerDto>
{
    public Guid FacilityId { get; init; }
}

public class GetEnergySourceQueryHandler : IRequestHandler<GetEnergySourceQuery, EnergySourcePowerDto>
{
    private readonly IApplicationDbContext _context;

    public GetEnergySourceQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<EnergySourcePowerDto> Handle(GetEnergySourceQuery request, CancellationToken ct)
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

        var pcsIds = devices
            .Where(d => d.DeviceType != null &&
                (d.DeviceType.Name == DeviceTypeNames.Pcs ||
                 d.DeviceType.Name == DeviceTypeNames.Inverter))
            .Select(d => d.Id).ToList();

        decimal? gridPower = null;
        decimal? solarPower = null;
        decimal? bessPower = null;

        // Grid power from SmartMeter
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
                var p = DashboardHelpers.GetDecimal(el, "activePower");
                if (p.HasValue) gridPower = (gridPower ?? 0m) + p.Value;
            }
        }

        // Solar power
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
                var p = DashboardHelpers.GetDecimal(el, "activePower");
                if (p.HasValue) solarPower = (solarPower ?? 0m) + p.Value;
            }
        }

        // BESS power (from battery data or PCS)
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
                var p = DashboardHelpers.GetDecimal(el, "power");
                if (p.HasValue) bessPower = (bessPower ?? 0m) + p.Value;
            }
        }

        if (bessPower == null && pcsIds.Count > 0)
        {
            var latest = await _context.DevicePcsData
                .Where(d => pcsIds.Contains(d.DeviceId))
                .GroupBy(d => d.DeviceId)
                .Select(g => g.OrderByDescending(d => d.DeviceLocalTime).First())
                .ToListAsync(ct);

            foreach (var entry in latest)
            {
                var el = PayloadDeserializer.GetElement(entry.Payload);
                var p = DashboardHelpers.GetDecimal(el, "pcsActivePower");
                if (p.HasValue) bessPower = (bessPower ?? 0m) + p.Value;
            }
        }

        decimal totalPower = (gridPower ?? 0m) + (solarPower ?? 0m) + Math.Abs(bessPower ?? 0m);

        var sources = new List<PowerSourceDto>();

        if (gridPower.HasValue)
            sources.Add(new PowerSourceDto
            {
                Name = "Grid",
                PowerKW = gridPower,
                Percentage = totalPower > 0m ? Math.Round(Math.Abs(gridPower.Value) / totalPower * 100m, 1) : null
            });

        if (solarPower.HasValue)
            sources.Add(new PowerSourceDto
            {
                Name = "Solar",
                PowerKW = solarPower,
                Percentage = totalPower > 0m ? Math.Round(Math.Abs(solarPower.Value) / totalPower * 100m, 1) : null
            });

        if (bessPower.HasValue)
            sources.Add(new PowerSourceDto
            {
                Name = "BESS",
                PowerKW = bessPower,
                Percentage = totalPower > 0m ? Math.Round(Math.Abs(bessPower.Value) / totalPower * 100m, 1) : null
            });

        return new EnergySourcePowerDto
        {
            TotalPowerKW = totalPower > 0m ? totalPower : null,
            Sources = sources
        };
    }
}
