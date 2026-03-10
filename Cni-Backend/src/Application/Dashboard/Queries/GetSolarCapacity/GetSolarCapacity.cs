using Backend.Application.Common.Interfaces;
using Backend.Application.Common.Security;
using Backend.Application.Common.Services;
using Backend.Domain.Constants;

namespace Backend.Application.Dashboard.Queries.GetSolarCapacity;

public record DashboardSolarCapacityDto
{
    public decimal? CurrentPowerKW { get; init; }
    public decimal? MaxCapacityKW { get; init; }
    public decimal? UtilizationPercent { get; init; }
    public decimal? ProducedTodayKWh { get; init; }
    public decimal? PanelTemperatureC { get; init; }
}

[Authorize(Roles = "Administrator,Tenant.Admin")]
[RequirePermission(Backend.Domain.Constants.Permissions.DashboardView)]
public record GetSolarCapacityQuery : IRequest<DashboardSolarCapacityDto>
{
    public Guid FacilityId { get; init; }
}

public class GetSolarCapacityQueryHandler : IRequestHandler<GetSolarCapacityQuery, DashboardSolarCapacityDto>
{
    private readonly IApplicationDbContext _context;

    public GetSolarCapacityQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<DashboardSolarCapacityDto> Handle(GetSolarCapacityQuery request, CancellationToken ct)
    {
        var facilityId = request.FacilityId;

        var solarDevices = await _context.Devices
            .Where(d => d.Rtu != null && d.Rtu.FacilityId == facilityId &&
                d.DeviceType != null && d.DeviceType.Name == DeviceTypeNames.Solar)
            .Include(d => d.DeviceType)
            .ToListAsync(ct);

        if (solarDevices.Count == 0)
            return new DashboardSolarCapacityDto();

        var deviceIds = solarDevices.Select(d => d.Id).ToList();

        // Latest snapshot per device
        var latest = await _context.DeviceSolarData
            .Where(d => deviceIds.Contains(d.DeviceId))
            .GroupBy(d => d.DeviceId)
            .Select(g => g.OrderByDescending(d => d.DeviceLocalTime).First())
            .ToListAsync(ct);

        decimal currentPower = 0m;
        decimal maxCapacity = 0m;
        decimal panelTempSum = 0m;
        int panelTempCount = 0;

        foreach (var entry in latest)
        {
            var el = PayloadDeserializer.GetElement(entry.Payload);
            var p = DashboardHelpers.GetDecimal(el, "activePower");
            var cap = DashboardHelpers.GetDecimal(el, "ratedPower");
            var temp = DashboardHelpers.GetDecimal(el, "cabinetTemp");

            if (p.HasValue) currentPower += p.Value;
            if (cap.HasValue) maxCapacity += cap.Value;
            if (temp.HasValue) { panelTempSum += temp.Value; panelTempCount++; }
        }

        // Today's production: sum dailyEnergy from latest snapshots
        decimal producedToday = 0m;
        foreach (var entry in latest)
        {
            var el = PayloadDeserializer.GetElement(entry.Payload);
            var daily = DashboardHelpers.GetDecimal(el, "dailyYield");
            if (daily.HasValue) producedToday += daily.Value;
        }

        decimal? utilization = maxCapacity > 0m
            ? Math.Round(currentPower / maxCapacity * 100m, 1)
            : null;

        return new DashboardSolarCapacityDto
        {
            CurrentPowerKW = currentPower > 0m ? currentPower : null,
            MaxCapacityKW = maxCapacity > 0m ? maxCapacity : null,
            UtilizationPercent = utilization,
            ProducedTodayKWh = producedToday > 0m ? producedToday : null,
            PanelTemperatureC = panelTempCount > 0 ? panelTempSum / panelTempCount : null
        };
    }
}
