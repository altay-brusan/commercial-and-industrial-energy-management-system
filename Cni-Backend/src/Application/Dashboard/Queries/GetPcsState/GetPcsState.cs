using Backend.Application.Common.Interfaces;
using Backend.Application.Common.Security;
using Backend.Application.Common.Services;
using Backend.Domain.Constants;

namespace Backend.Application.Dashboard.Queries.GetPcsState;

public record DashboardPcsStateDto
{
    public List<PcsUnitStateDto> Units { get; init; } = new();
}

public record PcsUnitStateDto
{
    public Guid DeviceId { get; init; }
    public string Name { get; init; } = string.Empty;
    public string? Mode { get; init; }
    public decimal? TemperatureC { get; init; }
    public decimal? CurrentLoadKW { get; init; }
    public decimal? EfficiencyPercent { get; init; }
    public decimal? AcVoltageV { get; init; }
    public decimal? DcVoltageV { get; init; }
}

[Authorize(Roles = "Administrator,Tenant.Admin")]
[RequirePermission(Backend.Domain.Constants.Permissions.DashboardView)]
public record GetPcsStateQuery : IRequest<DashboardPcsStateDto>
{
    public Guid FacilityId { get; init; }
}

public class GetPcsStateQueryHandler : IRequestHandler<GetPcsStateQuery, DashboardPcsStateDto>
{
    private readonly IApplicationDbContext _context;

    public GetPcsStateQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<DashboardPcsStateDto> Handle(GetPcsStateQuery request, CancellationToken ct)
    {
        var facilityId = request.FacilityId;

        var pcsDevices = await _context.Devices
            .Where(d => d.Rtu != null && d.Rtu.FacilityId == facilityId &&
                d.DeviceType != null &&
                (d.DeviceType.Name == DeviceTypeNames.Pcs ||
                 d.DeviceType.Name == DeviceTypeNames.Inverter))
            .Include(d => d.DeviceType)
            .ToListAsync(ct);

        if (pcsDevices.Count == 0)
            return new DashboardPcsStateDto();

        var deviceIds = pcsDevices.Select(d => d.Id).ToList();

        var latestPcs = await _context.DevicePcsData
            .Where(d => deviceIds.Contains(d.DeviceId))
            .GroupBy(d => d.DeviceId)
            .Select(g => g.OrderByDescending(d => d.DeviceLocalTime).First())
            .ToListAsync(ct);

        var telemetryByDevice = latestPcs.ToDictionary(
            e => e.DeviceId,
            e => PayloadDeserializer.GetElement(e.Payload));

        var units = pcsDevices.Select(device =>
        {
            telemetryByDevice.TryGetValue(device.Id, out var el);

            var temperature = DashboardHelpers.GetDecimal(el, "pcsIgbtTemp")
                              ?? DashboardHelpers.GetDecimal(el, "pcsAmbientTemp");
            var currentLoad = DashboardHelpers.GetDecimal(el, "pcsActivePower");
            var efficiency = DashboardHelpers.GetDecimal(el, "pcsEfficiency");
            var acVoltage = DashboardHelpers.GetDecimal(el, "pcsLineVoltageAB");
            var dcVoltage = DashboardHelpers.GetDecimal(el, "pcsTotalDcBusVoltage");

            string? mode = null;
            if (el.HasValue)
            {
                var modeVal = PayloadDeserializer.ExtractValue(el.Value, "pcsWorkingCondition");
                mode = modeVal?.ToString();
            }

            return new PcsUnitStateDto
            {
                DeviceId = device.Id,
                Name = device.Name,
                Mode = mode,
                TemperatureC = temperature,
                CurrentLoadKW = currentLoad,
                EfficiencyPercent = efficiency,
                AcVoltageV = acVoltage,
                DcVoltageV = dcVoltage
            };
        }).ToList();

        return new DashboardPcsStateDto { Units = units };
    }
}
