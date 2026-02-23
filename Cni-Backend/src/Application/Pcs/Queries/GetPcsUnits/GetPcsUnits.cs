using Backend.Application.Common.Interfaces;
using Backend.Application.Common.Models.Monitoring;
using Backend.Application.Common.Security;
using Backend.Domain.Constants;

namespace Backend.Application.Pcs.Queries.GetPcsUnits;

[Authorize(Roles = "Administrator,Tenant.Admin")]
[RequirePermission(Backend.Domain.Constants.Permissions.PcsView)]
public record GetPcsUnitsQuery : IRequest<PcsUnitsResultDto>
{
    public string? Search { get; init; }
    public string? SortBy { get; init; }
    public string? SortDirection { get; init; }
}

public class GetPcsUnitsQueryHandler : IRequestHandler<GetPcsUnitsQuery, PcsUnitsResultDto>
{
    private readonly IApplicationDbContext _context;

    public GetPcsUnitsQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<PcsUnitsResultDto> Handle(GetPcsUnitsQuery request, CancellationToken cancellationToken)
    {
        var pcsDevices = await _context.Devices
            .Include(d => d.DeviceType)
            .Include(d => d.Rtu!).ThenInclude(r => r.Facility)
            .Where(d => d.DeviceType != null && d.DeviceType.Name == "PCS")
            .ToListAsync(cancellationToken);

        var deviceIds = pcsDevices.Select(d => d.Id).ToList();
        var facilityIds = pcsDevices.Select(d => d.Rtu!.FacilityId).Distinct().ToList();

        var latestData = await _context.DevicePcsData
            .Where(dd => deviceIds.Contains(dd.DeviceId))
            .GroupBy(dd => dd.DeviceId)
            .Select(g => g.OrderByDescending(dd => dd.DeviceLocalTime).First())
            .ToListAsync(cancellationToken);

        var dataByDevice = latestData.ToDictionary(d => d.DeviceId);

        // Find linked BESS units (Battery devices in same facility)
        var batteryDevices = await _context.Devices
            .Include(d => d.DeviceType)
            .Where(d => facilityIds.Contains(d.Rtu!.FacilityId) && d.DeviceType != null && d.DeviceType.Name == DeviceTypeNames.EnergyStorage)
            .ToListAsync(cancellationToken);

        var batteryByFacility = batteryDevices
            .GroupBy(d => d.Rtu!.FacilityId)
            .ToDictionary(g => g.Key, g => g.First());

        var units = pcsDevices.Select(device =>
        {
            dataByDevice.TryGetValue(device.Id, out var data);
            var payload = data != null
                ? Common.Services.PayloadDeserializer.GetElement(data.Payload)
                : null;

            batteryByFacility.TryGetValue(device.Rtu!.FacilityId, out var linkedBess);

            return new PcsUnitListItemDto
            {
                Id = device.Id,
                Name = device.Name,
                SerialNumber = device.SerialNumber,
                ModelName = device.Model,
                Manufacturer = device.Manufacturer,
                FacilityName = device.Rtu?.Facility?.Name,
                FacilityId = device.Rtu!.FacilityId,
                Status = StatusBadgeDto.FromStatus(device.Status),
                GridMode = ExtractString(payload, "status.operatingMode"),
                ActivePowerKw = ExtractDouble(payload, "acPower.totalActivePower"),
                GridFreqHz = ExtractDouble(payload, "acPower.frequency"),
                IgbtTempC = ExtractDouble(payload, "temperaturePcs"),
                EfficiencyPct = ExtractDouble(payload, "efficiency"),
                PowerFactor = ExtractDouble(payload, "acPower.totalPowerFactor"),
                LinkedBessUnitId = linkedBess?.Id,
                LinkedBessUnitName = linkedBess?.Name,
                LastSeenUtc = device.LastSeenUtc
            };
        }).ToList();

        // Apply search filter
        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var search = request.Search.ToLowerInvariant();
            units = units.Where(u =>
                (u.Name?.ToLowerInvariant().Contains(search) == true) ||
                (u.SerialNumber?.ToLowerInvariant().Contains(search) == true))
                .ToList();
        }

        // Apply sorting
        units = (request.SortBy?.ToLowerInvariant(), request.SortDirection?.ToLowerInvariant()) switch
        {
            ("name", "desc") => units.OrderByDescending(u => u.Name).ToList(),
            ("power", "asc") => units.OrderBy(u => u.ActivePowerKw).ToList(),
            ("power", "desc") => units.OrderByDescending(u => u.ActivePowerKw).ToList(),
            ("efficiency", "asc") => units.OrderBy(u => u.EfficiencyPct).ToList(),
            ("efficiency", "desc") => units.OrderByDescending(u => u.EfficiencyPct).ToList(),
            ("status", "asc") => units.OrderBy(u => u.Status.Value).ToList(),
            ("status", "desc") => units.OrderByDescending(u => u.Status.Value).ToList(),
            _ => units.OrderBy(u => u.Name).ToList()
        };

        var powerValues = units.Where(u => u.ActivePowerKw.HasValue).Select(u => u.ActivePowerKw!.Value).ToList();
        var effValues = units.Where(u => u.EfficiencyPct.HasValue).Select(u => u.EfficiencyPct!.Value).ToList();
        var pfValues = units.Where(u => u.PowerFactor.HasValue).Select(u => u.PowerFactor!.Value).ToList();

        return new PcsUnitsResultDto
        {
            Units = units,
            Aggregates = new PcsAggregatesDto
            {
                TotalUnits = units.Count,
                OnlineCount = units.Count(u => u.Status.Value == "online"),
                OfflineCount = units.Count(u => u.Status.Value == "offline"),
                FaultCount = units.Count(u => u.Status.Value == "fault"),
                TotalActivePowerKw = powerValues.Count > 0 ? Math.Round(powerValues.Sum(), 2) : null,
                AvgEfficiencyPct = effValues.Count > 0 ? Math.Round(effValues.Average(), 1) : null,
                AvgPowerFactor = pfValues.Count > 0 ? Math.Round(pfValues.Average(), 3) : null
            }
        };
    }

    private static double? ExtractDouble(System.Text.Json.JsonElement? element, string path)
    {
        if (!element.HasValue) return null;
        var value = Common.Services.PayloadDeserializer.ExtractValue(element.Value, path);
        return value switch
        {
            double d => d,
            int i => i,
            long l => l,
            _ => null
        };
    }

    private static string? ExtractString(System.Text.Json.JsonElement? element, string path)
    {
        if (!element.HasValue) return null;
        return Common.Services.PayloadDeserializer.ExtractValue(element.Value, path) as string;
    }
}
