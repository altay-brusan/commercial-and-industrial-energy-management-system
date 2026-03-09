using System.Text.Json;
using Backend.Application.Common.Interfaces;
using Backend.Application.Common.Models.Monitoring;
using Backend.Application.Common.Security;
using Backend.Application.Common.Services;
using Backend.Application.Common.Services.ParameterMapper;
using Backend.Domain.Constants;
using Microsoft.Extensions.Caching.Memory;

namespace Backend.Application.BatteryStorage.Queries.GetBessUnits;

[Authorize(Roles = "Administrator,Tenant.Admin")]
[RequirePermission(Backend.Domain.Constants.Permissions.BessView)]
public record GetBessUnitsQuery : IRequest<BessUnitsResultDto>
{
    public Guid? FacilityId { get; init; }
    public string? Search { get; init; }
    public string? SortBy { get; init; }
    public string? SortDirection { get; init; }
}

public class GetBessUnitsQueryHandler : IRequestHandler<GetBessUnitsQuery, BessUnitsResultDto>
{
    private readonly IApplicationDbContext _context;
    private readonly IMemoryCache _cache;
    private readonly IDeviceStatusService _statusService;

    public GetBessUnitsQueryHandler(IApplicationDbContext context, IMemoryCache cache, IDeviceStatusService statusService)
    {
        _context = context;
        _cache = cache;
        _statusService = statusService;
    }

    public async Task<BessUnitsResultDto> Handle(GetBessUnitsQuery request, CancellationToken cancellationToken)
    {
        var query = _context.Devices
            .Include(d => d.DeviceType)
            .Include(d => d.LogicalDevice)
            .Include(d => d.SpecificationTemplate)
            .Include(d => d.Rtu!).ThenInclude(r => r.Facility)
            .Where(d =>
                (d.DeviceType != null && d.DeviceType.Name == DeviceTypeNames.EnergyStorage) ||
                (d.LogicalDevice != null && d.LogicalDevice.Category == "BESS"));

        if (request.FacilityId.HasValue)
            query = query.Where(d => d.Rtu!.FacilityId == request.FacilityId.Value);

        var batteryDevices = await query.ToListAsync(cancellationToken);

        var deviceIds = batteryDevices.Select(d => d.Id).ToList();

        // Load latest telemetry per device per Kind from all relevant tables
        var batteryEntries = await _context.DeviceBatteryData
            .Where(dd => deviceIds.Contains(dd.DeviceId))
            .GroupBy(dd => new { dd.DeviceId, dd.Kind })
            .Select(g => g.OrderByDescending(dd => dd.DeviceLocalTime).First())
            .ToListAsync(cancellationToken);

        var pcsEntries = await _context.DevicePcsData
            .Where(dd => deviceIds.Contains(dd.DeviceId))
            .GroupBy(dd => new { dd.DeviceId, dd.Kind })
            .Select(g => g.OrderByDescending(dd => dd.DeviceLocalTime).First())
            .ToListAsync(cancellationToken);

        // Build per-device payloadsByKind dictionary
        var telemetryByDevice = new Dictionary<Guid, Dictionary<string, JsonElement?>>();
        foreach (var entry in batteryEntries)
        {
            if (!telemetryByDevice.TryGetValue(entry.DeviceId, out var kindMap))
            {
                kindMap = new Dictionary<string, JsonElement?>();
                telemetryByDevice[entry.DeviceId] = kindMap;
            }
            if (entry.Kind != null)
                kindMap[entry.Kind] = PayloadDeserializer.GetElement(entry.Payload);
        }
        foreach (var entry in pcsEntries)
        {
            if (!telemetryByDevice.TryGetValue(entry.DeviceId, out var kindMap))
            {
                kindMap = new Dictionary<string, JsonElement?>();
                telemetryByDevice[entry.DeviceId] = kindMap;
            }
            if (entry.Kind != null)
                kindMap[entry.Kind] = PayloadDeserializer.GetElement(entry.Payload);
        }

        // Load active alarm counts per device (includes descendant devices)
        var allDescendantIds = await _context.Devices
            .Where(d => d.ParentDeviceId.HasValue && deviceIds.Contains(d.ParentDeviceId.Value))
            .Select(d => new { d.Id, d.ParentDeviceId })
            .ToListAsync(cancellationToken);

        // Build a map of root device → all descendant IDs (walk the tree)
        var allDevicesInRtus = await _context.Devices
            .Where(d => deviceIds.Contains(d.Id) || d.ParentDeviceId.HasValue)
            .Select(d => new { d.Id, d.ParentDeviceId, d.RtuId })
            .ToListAsync(cancellationToken);

        // Group all devices by RTU, then map root → descendants
        var rootRtuMap = batteryDevices.ToDictionary(d => d.Id, d => d.RtuId);
        var devicesByRtu = allDevicesInRtus.GroupBy(d => d.RtuId).ToDictionary(g => g.Key, g => g.Select(d => d.Id).ToList());

        var alarmDeviceIds = devicesByRtu.Values.SelectMany(ids => ids).Distinct().ToList();
        var alarmCounts = await _context.DeviceAlarms
            .Where(a => alarmDeviceIds.Contains(a.DeviceId) && a.IsActive)
            .GroupBy(a => a.DeviceId)
            .Select(g => new { DeviceId = g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.DeviceId, x => x.Count, cancellationToken);

        // Calculate alarm count per root device (sum across all devices in same RTU)
        var rootAlarmCounts = new Dictionary<Guid, int>();
        foreach (var device in batteryDevices)
        {
            var rtuDeviceIds = devicesByRtu.GetValueOrDefault(device.RtuId, new List<Guid>());
            rootAlarmCounts[device.Id] = rtuDeviceIds.Sum(id => alarmCounts.GetValueOrDefault(id, 0));
        }

        // Batch-resolve device statuses from active alarms
        var statuses = await _statusService.GetStatusBatchAsync(deviceIds, cancellationToken);

        var units = batteryDevices.Select(device =>
        {
            var payloadsByKind = telemetryByDevice.GetValueOrDefault(device.Id)
                ?? new Dictionary<string, JsonElement?>();

            // Fallback payload for direct extraction (Battery.Energy)
            var payload = payloadsByKind.GetValueOrDefault("Battery.Energy");

            // Build resolver from template's ParameterMapTemplate (if present)
            var parameterMap = ParameterMapParser.ParseCached(device.SpecificationTemplate?.ParameterMapTemplate, _cache);
            var metadata = PayloadDeserializer.GetElement(device.Metadata);
            var resolver = parameterMap.Count > 0 && payloadsByKind.Count > 0
                ? new ParameterResolver(parameterMap, payloadsByKind, metadata)
                : null;

            var stateStr = resolver?.ResolveString(ParameterKeys.OperatingMode)
                ?? (payload.HasValue ? PayloadDeserializer.ExtractValue(payload.Value, "state") as string : null);

            return new BessUnitListItemDto
            {
                Id = device.Id,
                Name = device.Name,
                Status = StatusBadgeDto.FromStatus(statuses.GetValueOrDefault(device.Id, "normal")),
                OperatingMode = StatusBadgeDto.FromOperatingMode(stateStr),
                Soc = resolver?.ResolveNumber(ParameterKeys.Soc)
                    ?? ExtractDouble(payload, "socPercent", "stateOfCharge", "soc"),
                Soh = resolver?.ResolveNumber(ParameterKeys.Soh)
                    ?? ExtractDouble(payload, "sohPercent", "stateOfHealth", "soh"),
                Power = resolver?.ResolveNumber(ParameterKeys.Power)
                    ?? (ExtractDouble(payload, "instantCurrentA") is double current
                            && ExtractDouble(payload, "totalVoltageV") is double voltage
                        ? Math.Round(voltage * current / 1000.0, 1)
                        : ExtractDouble(payload, "dcPower.power", "power")),
                CapacityKwh = resolver?.ResolveNumber(ParameterKeys.AvailableEnergy)
                    ?? ExtractDouble(payload, "availableEnergyKwh", "capacityKwh", "ratedEnergyKwh"),
                ActiveAlarmCount = rootAlarmCounts.GetValueOrDefault(device.Id, 0),
                Brand = device.Manufacturer,
                Model = device.Model,
                SerialNumber = device.SerialNumber,
                FacilityName = device.Rtu?.Facility?.Name,
                FacilityId = device.Rtu!.FacilityId,
                TemperatureC = resolver?.ResolveNumber(ParameterKeys.AmbientTemp)
                    ?? ExtractDouble(payload, "ambientTempC", "batteryTemperature"),
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
            ("soc", "asc") => units.OrderBy(u => u.Soc).ToList(),
            ("soc", "desc") => units.OrderByDescending(u => u.Soc).ToList(),
            ("power", "asc") => units.OrderBy(u => u.Power).ToList(),
            ("power", "desc") => units.OrderByDescending(u => u.Power).ToList(),
            ("status", "asc") => units.OrderBy(u => u.Status.Value).ToList(),
            ("status", "desc") => units.OrderByDescending(u => u.Status.Value).ToList(),
            _ => units.OrderBy(u => u.Name).ToList()
        };

        var socValues = units.Where(u => u.Soc.HasValue).Select(u => u.Soc!.Value).ToList();
        var capacityValues = units.Where(u => u.CapacityKwh.HasValue).Select(u => u.CapacityKwh!.Value).ToList();

        return new BessUnitsResultDto
        {
            Units = units,
            Aggregates = new BessAggregatesDto
            {
                TotalUnits = units.Count,
                TotalCapacityKwh = capacityValues.Count > 0 ? Math.Round(capacityValues.Sum(), 2) : null,
                AverageSoc = socValues.Count > 0 ? Math.Round(socValues.Average(), 1) : null,
                TotalActiveAlarms = units.Sum(u => u.ActiveAlarmCount),
                OnlineCount = units.Count(u => u.Status.Value == "online"),
                OfflineCount = units.Count(u => u.Status.Value == "offline"),
                FaultCount = units.Count(u => u.Status.Value == "fault"),
                StandbyCount = units.Count(u => u.Status.Value == "standby")
            }
        };
    }

    private static double? ExtractDouble(System.Text.Json.JsonElement? element, params string[] paths)
    {
        if (!element.HasValue) return null;
        foreach (var path in paths)
        {
            var value = PayloadDeserializer.ExtractValue(element.Value, path);
            var result = value switch
            {
                double d => d,
                int i => i,
                long l => l,
                _ => (double?)null
            };
            if (result.HasValue) return result;
        }
        return null;
    }
}
