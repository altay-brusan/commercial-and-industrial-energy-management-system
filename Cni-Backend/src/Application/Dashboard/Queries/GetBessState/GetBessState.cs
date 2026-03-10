using Backend.Application.Common.Interfaces;
using Backend.Application.Common.Security;
using Backend.Application.Common.Services;
using Backend.Domain.Constants;

namespace Backend.Application.Dashboard.Queries.GetBessState;

public record DashboardBessStateDto
{
    public List<BessUnitStateDto> Units { get; init; } = new();
}

public record BessUnitStateDto
{
    public Guid DeviceId { get; init; }
    public string Name { get; init; } = string.Empty;
    public decimal? SocPercent { get; init; }
    public string? Status { get; init; }
    public string? ChargeLevel { get; init; }
    public decimal? CapacityKWh { get; init; }
    public decimal? HealthPercent { get; init; }
}

[Authorize(Roles = "Administrator,Tenant.Admin")]
[RequirePermission(Backend.Domain.Constants.Permissions.DashboardView)]
public record GetBessStateQuery : IRequest<DashboardBessStateDto>
{
    public Guid FacilityId { get; init; }
}

public class GetBessStateQueryHandler : IRequestHandler<GetBessStateQuery, DashboardBessStateDto>
{
    private readonly IApplicationDbContext _context;

    public GetBessStateQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<DashboardBessStateDto> Handle(GetBessStateQuery request, CancellationToken ct)
    {
        var facilityId = request.FacilityId;

        var bessDevices = await _context.Devices
            .Where(d => d.Rtu != null && d.Rtu.FacilityId == facilityId &&
                d.DeviceType != null &&
                (d.DeviceType.Name == DeviceTypeNames.EnergyStorage ||
                 d.DeviceType.Name == DeviceTypeNames.EnergyStorageBattery))
            .Include(d => d.DeviceType)
            .ToListAsync(ct);

        if (bessDevices.Count == 0)
            return new DashboardBessStateDto();

        var deviceIds = bessDevices.Select(d => d.Id).ToList();

        var latestBattery = await _context.DeviceBatteryData
            .Where(d => deviceIds.Contains(d.DeviceId))
            .GroupBy(d => d.DeviceId)
            .Select(g => g.OrderByDescending(d => d.DeviceLocalTime).First())
            .ToListAsync(ct);

        var telemetryByDevice = latestBattery.ToDictionary(
            e => e.DeviceId,
            e => PayloadDeserializer.GetElement(e.Payload));

        var units = bessDevices.Select(device =>
        {
            telemetryByDevice.TryGetValue(device.Id, out var el);

            var soc = DashboardHelpers.GetDecimal(el, "soc");
            var soh = DashboardHelpers.GetDecimal(el, "soh");
            var capacity = DashboardHelpers.GetDecimal(el, "nominalCapacity");

            // Derive status from active alarms count (simple: no telemetry = Unknown)
            string? status = el.HasValue ? "Online" : "Unknown";

            return new BessUnitStateDto
            {
                DeviceId = device.Id,
                Name = device.Name,
                SocPercent = soc,
                Status = status,
                ChargeLevel = DashboardHelpers.GetChargeLevel(soc),
                CapacityKWh = capacity,
                HealthPercent = soh
            };
        }).ToList();

        return new DashboardBessStateDto { Units = units };
    }
}
