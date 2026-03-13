using System.Text.Json;
using Backend.Application.Common.Interfaces;
using Backend.Application.Common.Models.Monitoring;
using Backend.Application.Common.Security;
using Backend.Application.Common.Services;
using Backend.Application.Common.Services.ParameterMapper;
using Backend.Domain.Constants;
using Microsoft.Extensions.Caching.Memory;

namespace Backend.Application.SolarInverters.Queries.GetSolarUnits;

[Authorize(Roles = "Administrator,Tenant.Admin")]
[RequirePermission(Backend.Domain.Constants.Permissions.SolarView)]
public record GetSolarUnitsQuery : IRequest<SolarUnitsResultDto>
{
    public Guid? FacilityId { get; init; }
    public string? Search { get; init; }
    public string? SortBy { get; init; }
    public string? SortDirection { get; init; }
}

public class GetSolarUnitsQueryHandler : IRequestHandler<GetSolarUnitsQuery, SolarUnitsResultDto>
{
    private readonly IApplicationDbContext _context;
    private readonly IDeviceStatusService _statusService;
    private readonly IMemoryCache _cache;

    public GetSolarUnitsQueryHandler(IApplicationDbContext context, IDeviceStatusService statusService, IMemoryCache cache)
    {
        _context = context;
        _statusService = statusService;
        _cache = cache;
    }

    public async Task<SolarUnitsResultDto> Handle(GetSolarUnitsQuery request, CancellationToken cancellationToken)
    {
        var query = _context.Devices
            .Include(d => d.DeviceType)
            .Include(d => d.LogicalDevice)
            .Include(d => d.SpecificationTemplate)
            .Include(d => d.Rtu!).ThenInclude(r => r.Facility)
            .Where(d =>
                (d.DeviceType != null && (d.DeviceType.Name == DeviceTypeNames.Solar || d.DeviceType.Name == DeviceTypeNames.Inverter)) ||
                (d.LogicalDevice != null && d.LogicalDevice.Category == "Solar"));

        if (request.FacilityId.HasValue)
            query = query.Where(d => d.Rtu!.FacilityId == request.FacilityId.Value);

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var search = request.Search.ToLowerInvariant();
            query = query.Where(d => d.Name.ToLower().Contains(search) ||
                (d.SerialNumber != null && d.SerialNumber.ToLower().Contains(search)));
        }

        var devices = await query.ToListAsync(cancellationToken);
        var deviceIds = devices.Select(d => d.Id).ToList();

        // Load latest telemetry per device per Kind from Solar table
        var solarEntries = await _context.DeviceSolarData
            .Where(dd => deviceIds.Contains(dd.DeviceId))
            .GroupBy(dd => new { dd.DeviceId, dd.Kind })
            .Select(g => g.OrderByDescending(dd => dd.DeviceLocalTime).First())
            .ToListAsync(cancellationToken);

        var telemetryByDevice = new Dictionary<Guid, Dictionary<string, JsonElement?>>();
        foreach (var entry in solarEntries)
        {
            if (!telemetryByDevice.TryGetValue(entry.DeviceId, out var kindMap))
            {
                kindMap = new Dictionary<string, JsonElement?>();
                telemetryByDevice[entry.DeviceId] = kindMap;
            }
            if (entry.Kind != null)
                kindMap[entry.Kind] = PayloadDeserializer.GetElement(entry.Payload);
        }

        var statuses = await _statusService.GetStatusBatchAsync(deviceIds, cancellationToken);

        var alarmCounts = await _context.DeviceAlarms
            .Where(a => deviceIds.Contains(a.DeviceId) && a.IsActive)
            .GroupBy(a => a.DeviceId)
            .Select(g => new { DeviceId = g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.DeviceId, x => x.Count, cancellationToken);

        var units = devices.Select(device =>
        {
            var payloadsByKind = telemetryByDevice.GetValueOrDefault(device.Id)
                ?? new Dictionary<string, JsonElement?>();

            var metadata = PayloadDeserializer.GetElement(device.Metadata);

            var parameterMap = ParameterMapParser.ParseCached(device.SpecificationTemplate?.ParameterMapTemplate, _cache);
            var resolver = parameterMap.Count > 0 && payloadsByKind.Count > 0
                ? new ParameterResolver(parameterMap, payloadsByKind, metadata)
                : null;

            var powerPayload = payloadsByKind.GetValueOrDefault("Solar.Power");

            var activePower = resolver?.ResolveNumber("activePower")
                ?? ExtractDouble(powerPayload, "activePower");

            var dailyYield = resolver?.ResolveNumber("dailyYield")
                ?? ExtractDouble(powerPayload, "dailyYield");

            var totalYield = resolver?.ResolveNumber("totalYield")
                ?? ExtractDouble(powerPayload, "totalYield");

            var efficiency = resolver?.ResolveNumber("efficiency")
                ?? ExtractDouble(powerPayload, "efficiency");

            var temperatureC = resolver?.ResolveNumber("temperature")
                ?? ExtractDouble(powerPayload, "temperature")
                ?? ExtractDouble(powerPayload, "internalTemperature");

            return new SolarUnitListItemDto
            {
                Id = device.Id,
                Name = device.Name,
                SerialNumber = device.SerialNumber,
                ModelName = device.Model,
                Manufacturer = device.Manufacturer,
                FacilityName = device.Rtu?.Facility?.Name,
                FacilityId = device.Rtu!.FacilityId,
                Status = StatusBadgeDto.FromStatus(statuses.GetValueOrDefault(device.Id, "normal")),
                ActivePowerKw = activePower,
                DailyYieldKwh = dailyYield,
                TotalYieldKwh = totalYield,
                EfficiencyPct = efficiency,
                TemperatureC = temperatureC,
                ActiveFaultCount = alarmCounts.GetValueOrDefault(device.Id, 0),
                LastSeenUtc = device.LastSeenUtc
            };
        }).ToList();

        // Apply sorting
        units = (request.SortBy?.ToLowerInvariant(), request.SortDirection?.ToLowerInvariant()) switch
        {
            ("name", "asc") => units.OrderBy(u => u.Name).ToList(),
            ("name", _) => units.OrderByDescending(u => u.Name).ToList(),
            ("power", "asc") => units.OrderBy(u => u.ActivePowerKw ?? 0).ToList(),
            ("power", _) => units.OrderByDescending(u => u.ActivePowerKw ?? 0).ToList(),
            ("yield", "asc") => units.OrderBy(u => u.DailyYieldKwh ?? 0).ToList(),
            ("yield", _) => units.OrderByDescending(u => u.DailyYieldKwh ?? 0).ToList(),
            _ => units
        };

        var powerValues = units.Where(u => u.ActivePowerKw.HasValue).Select(u => u.ActivePowerKw!.Value).ToList();
        var yieldValues = units.Where(u => u.DailyYieldKwh.HasValue).Select(u => u.DailyYieldKwh!.Value).ToList();
        var effValues = units.Where(u => u.EfficiencyPct.HasValue).Select(u => u.EfficiencyPct!.Value).ToList();

        return new SolarUnitsResultDto
        {
            Units = units,
            Aggregates = new SolarAggregatesDto
            {
                TotalUnits = units.Count,
                OnlineCount = units.Count(u => u.Status.Value == "online"),
                OfflineCount = units.Count(u => u.Status.Value == "offline"),
                FaultCount = units.Count(u => u.Status.Value == "fault"),
                TotalActivePowerKw = powerValues.Count > 0 ? Math.Round(powerValues.Sum(), 2) : null,
                TotalDailyYieldKwh = yieldValues.Count > 0 ? Math.Round(yieldValues.Sum(), 2) : null,
                AverageEfficiencyPct = effValues.Count > 0 ? Math.Round(effValues.Average(), 1) : null,
                TotalActiveFaults = units.Sum(u => u.ActiveFaultCount)
            }
        };
    }

    private static double? ExtractDouble(JsonElement? element, string path)
    {
        if (!element.HasValue) return null;
        var value = PayloadDeserializer.ExtractValue(element.Value, path);
        return value switch
        {
            double d => d,
            int i => i,
            long l => l,
            _ => null
        };
    }
}

public record SolarUnitsResultDto
{
    public List<SolarUnitListItemDto> Units { get; init; } = new();
    public SolarAggregatesDto Aggregates { get; init; } = new();
}

public record SolarUnitListItemDto
{
    public Guid Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string? SerialNumber { get; init; }
    public string? ModelName { get; init; }
    public string? Manufacturer { get; init; }
    public string? FacilityName { get; init; }
    public Guid FacilityId { get; init; }
    public StatusBadgeDto Status { get; init; } = new();
    public double? ActivePowerKw { get; init; }
    public double? DailyYieldKwh { get; init; }
    public double? TotalYieldKwh { get; init; }
    public double? EfficiencyPct { get; init; }
    public double? TemperatureC { get; init; }
    public int ActiveFaultCount { get; init; }
    public DateTimeOffset? LastSeenUtc { get; init; }
}

public record SolarAggregatesDto
{
    public int TotalUnits { get; init; }
    public int OnlineCount { get; init; }
    public int OfflineCount { get; init; }
    public int FaultCount { get; init; }
    public double? TotalActivePowerKw { get; init; }
    public double? TotalDailyYieldKwh { get; init; }
    public double? AverageEfficiencyPct { get; init; }
    public int TotalActiveFaults { get; init; }
}
