using Backend.Application.Common.Interfaces;
using Backend.Application.Common.Models.Monitoring;
using Backend.Application.Common.Security;

namespace Backend.Application.BatteryStorage.Queries.GetBessUnits;

[Authorize(Roles = "Administrator,Tenant.Admin")]
[RequirePermission(Backend.Domain.Constants.Permissions.BessView)]
public record GetBessUnitsQuery : IRequest<BessUnitsResultDto>
{
    public string? Search { get; init; }
    public string? SortBy { get; init; }
    public string? SortDirection { get; init; }
}

public class GetBessUnitsQueryHandler : IRequestHandler<GetBessUnitsQuery, BessUnitsResultDto>
{
    private readonly IApplicationDbContext _context;

    public GetBessUnitsQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<BessUnitsResultDto> Handle(GetBessUnitsQuery request, CancellationToken cancellationToken)
    {
        var batteryDevices = await _context.Devices
            .Include(d => d.DeviceType)
            .Include(d => d.Rtu!).ThenInclude(r => r.Facility)
            .Where(d => d.DeviceType != null && d.DeviceType.Name == "Battery")
            .ToListAsync(cancellationToken);

        var deviceIds = batteryDevices.Select(d => d.Id).ToList();

        var latestData = await _context.DeviceBatteryData
            .Where(dd => deviceIds.Contains(dd.DeviceId))
            .GroupBy(dd => dd.DeviceId)
            .Select(g => g.OrderByDescending(dd => dd.DeviceLocalTime).First())
            .ToListAsync(cancellationToken);

        var dataByDevice = latestData.ToDictionary(d => d.DeviceId);

        var units = batteryDevices.Select(device =>
        {
            dataByDevice.TryGetValue(device.Id, out var data);
            var payload = data != null
                ? Common.Services.PayloadDeserializer.GetElement(data.Payload)
                : null;

            return new BessUnitListItemDto
            {
                Id = device.Id,
                Name = device.Name,
                SerialNumber = device.SerialNumber,
                ModelName = device.Model,
                Manufacturer = device.Manufacturer,
                FacilityName = device.Rtu?.Facility?.Name,
                FacilityId = device.Rtu!.FacilityId,
                Status = StatusBadgeDto.FromStatus(device.Status),
                SocPercent = ExtractDouble(payload, "stateOfCharge"),
                SohPercent = ExtractDouble(payload, "stateOfHealth"),
                PowerKw = ExtractDouble(payload, "dcPower.power"),
                TemperatureC = ExtractDouble(payload, "batteryTemperature"),
                ChargeStatus = payload.HasValue
                    ? Common.Services.PayloadDeserializer.ExtractValue(payload.Value, "chargeStatus") as string
                    : null,
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
            ("soc", "asc") => units.OrderBy(u => u.SocPercent).ToList(),
            ("soc", "desc") => units.OrderByDescending(u => u.SocPercent).ToList(),
            ("power", "asc") => units.OrderBy(u => u.PowerKw).ToList(),
            ("power", "desc") => units.OrderByDescending(u => u.PowerKw).ToList(),
            ("status", "asc") => units.OrderBy(u => u.Status.Value).ToList(),
            ("status", "desc") => units.OrderByDescending(u => u.Status.Value).ToList(),
            _ => units.OrderBy(u => u.Name).ToList()
        };

        var socValues = units.Where(u => u.SocPercent.HasValue).Select(u => u.SocPercent!.Value).ToList();
        var sohValues = units.Where(u => u.SohPercent.HasValue).Select(u => u.SohPercent!.Value).ToList();
        var powerValues = units.Where(u => u.PowerKw.HasValue).Select(u => u.PowerKw!.Value).ToList();

        return new BessUnitsResultDto
        {
            Units = units,
            Aggregates = new BessAggregatesDto
            {
                TotalUnits = units.Count,
                OnlineCount = units.Count(u => u.Status.Value == "online"),
                OfflineCount = units.Count(u => u.Status.Value == "offline"),
                FaultCount = units.Count(u => u.Status.Value == "fault"),
                AvgSocPercent = socValues.Count > 0 ? Math.Round(socValues.Average(), 1) : null,
                AvgSohPercent = sohValues.Count > 0 ? Math.Round(sohValues.Average(), 1) : null,
                TotalPowerKw = powerValues.Count > 0 ? Math.Round(powerValues.Sum(), 2) : null
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
}
