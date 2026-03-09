using System.Text.Json;
using Backend.Application.Common.Interfaces;
using Backend.Application.Common.Models.Monitoring;
using Backend.Application.Common.Security;
using Backend.Application.Common.Services;
using Backend.Application.Common.Services.ParameterMapper;
using Backend.Domain.Constants;
using Microsoft.Extensions.Caching.Memory;

namespace Backend.Application.Pcs.Queries.GetPcsUnits;

[Authorize(Roles = "Administrator,Tenant.Admin")]
[RequirePermission(Backend.Domain.Constants.Permissions.PcsView)]
public record GetPcsUnitsQuery : IRequest<PcsUnitsResultDto>
{
    public Guid? FacilityId { get; init; }
}

public class GetPcsUnitsQueryHandler : IRequestHandler<GetPcsUnitsQuery, PcsUnitsResultDto>
{
    private readonly IApplicationDbContext _context;
    private readonly IDeviceStatusService _statusService;
    private readonly IMemoryCache _cache;

    public GetPcsUnitsQueryHandler(IApplicationDbContext context, IDeviceStatusService statusService, IMemoryCache cache)
    {
        _context = context;
        _statusService = statusService;
        _cache = cache;
    }

    public async Task<PcsUnitsResultDto> Handle(GetPcsUnitsQuery request, CancellationToken cancellationToken)
    {
        var pcsQuery = _context.Devices
            .Include(d => d.DeviceType)
            .Include(d => d.LogicalDevice)
            .Include(d => d.SpecificationTemplate)
            .Include(d => d.Rtu!).ThenInclude(r => r.Facility)
            .Where(d =>
                (d.DeviceType != null && d.DeviceType.Name == DeviceTypeNames.Pcs) ||
                (d.LogicalDevice != null && d.LogicalDevice.Category == "PCS"));

        if (request.FacilityId.HasValue)
            pcsQuery = pcsQuery.Where(d => d.Rtu!.FacilityId == request.FacilityId.Value);

        var pcsDevices = await pcsQuery.ToListAsync(cancellationToken);

        var deviceIds = pcsDevices.Select(d => d.Id).ToList();

        // Load latest telemetry per device per Kind from PCS table
        var pcsEntries = await _context.DevicePcsData
            .Where(dd => deviceIds.Contains(dd.DeviceId))
            .GroupBy(dd => new { dd.DeviceId, dd.Kind })
            .Select(g => g.OrderByDescending(dd => dd.DeviceLocalTime).First())
            .ToListAsync(cancellationToken);

        var telemetryByDevice = new Dictionary<Guid, Dictionary<string, JsonElement?>>();
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

        // Resolve linked BESS units via parent device chain
        var parentIds = pcsDevices
            .Where(d => d.ParentDeviceId.HasValue)
            .Select(d => d.ParentDeviceId!.Value)
            .Distinct()
            .ToList();

        var parentDevices = parentIds.Count > 0
            ? await _context.Devices
                .Include(d => d.DeviceType)
                .Where(d => parentIds.Contains(d.Id))
                .ToDictionaryAsync(d => d.Id, cancellationToken)
            : new Dictionary<Guid, Domain.Entities.Device>();

        // For PCS without a direct BESS parent, find EnergyStorage devices in same facility as fallback
        var facilityIds = pcsDevices.Select(d => d.Rtu!.FacilityId).Distinct().ToList();
        var bessDevicesByFacility = await _context.Devices
            .Include(d => d.DeviceType)
            .Include(d => d.LogicalDevice)
            .Where(d => facilityIds.Contains(d.Rtu!.FacilityId)
                && ((d.DeviceType != null && d.DeviceType.Name == DeviceTypeNames.EnergyStorage) ||
                    (d.LogicalDevice != null && d.LogicalDevice.Category == "BESS")))
            .Select(d => new { d.Id, d.Name, FacilityId = d.Rtu!.FacilityId })
            .ToListAsync(cancellationToken);

        var bessByFacility = bessDevicesByFacility
            .GroupBy(d => d.FacilityId)
            .ToDictionary(g => g.Key, g => g.First());

        // Batch-resolve device statuses from active alarms
        var statuses = await _statusService.GetStatusBatchAsync(deviceIds, cancellationToken);

        // Load active alarm counts per PCS device
        var alarmCounts = await _context.DeviceAlarms
            .Where(a => deviceIds.Contains(a.DeviceId) && a.IsActive)
            .GroupBy(a => a.DeviceId)
            .Select(g => new { DeviceId = g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.DeviceId, x => x.Count, cancellationToken);

        var units = pcsDevices.Select(device =>
        {
            var payloadsByKind = telemetryByDevice.GetValueOrDefault(device.Id)
                ?? new Dictionary<string, JsonElement?>();

            var metadata = PayloadDeserializer.GetElement(device.Metadata);

            // Build ParameterResolver from template's ParameterMapTemplate (if present)
            var parameterMap = ParameterMapParser.ParseCached(device.SpecificationTemplate?.ParameterMapTemplate, _cache);
            var resolver = parameterMap.Count > 0 && payloadsByKind.Count > 0
                ? new ParameterResolver(parameterMap, payloadsByKind, metadata)
                : null;

            // Fallback payloads for hardcoded extraction
            var powerPayload = payloadsByKind.GetValueOrDefault("Pcs.PowerDistribution");
            var statusPayload = payloadsByKind.GetValueOrDefault("Pcs.Status");
            var tempsPayload = payloadsByKind.GetValueOrDefault("Pcs.Status");
            var dcPayload = payloadsByKind.GetValueOrDefault("Pcs.DcMeasurement");

            // Resolve linked BESS: prefer parent device if it's EnergyStorage, else fallback to facility
            PcsLinkedBessUnitDto? linkedBess = null;
            if (device.ParentDeviceId.HasValue &&
                parentDevices.TryGetValue(device.ParentDeviceId.Value, out var parent))
            {
                if (parent.DeviceType?.Name == DeviceTypeNames.EnergyStorage ||
                    parent.LogicalDevice?.Category == "BESS")
                {
                    linkedBess = new PcsLinkedBessUnitDto { Id = parent.Id, Name = parent.Name };
                }
            }
            linkedBess ??= bessByFacility.TryGetValue(device.Rtu!.FacilityId, out var fallback)
                ? new PcsLinkedBessUnitDto { Id = fallback.Id, Name = fallback.Name }
                : null;

            var activePower = resolver?.ResolveNumber(ParameterKeys.PcsActivePower)
                ?? ExtractDouble(powerPayload, "pcs_outActPwrKw");

            var reactivePower = resolver?.ResolveNumber(ParameterKeys.PcsReactivePower)
                ?? ExtractDouble(powerPayload, "pcs_outReaPwrKvar");

            var dcPower = resolver?.ResolveNumber(ParameterKeys.PcsDcPower)
                ?? ExtractDouble(dcPayload, "pcs_dcPwrKw");

            // Compute efficiency: |AC power| / |DC power| * 100, capped at 100
            var efficiency = resolver?.ResolveNumber(ParameterKeys.PcsEfficiency);
            if (!efficiency.HasValue && activePower.HasValue && dcPower.HasValue && Math.Abs(dcPower.Value) > 0.1)
            {
                var raw = Math.Abs(activePower.Value) / Math.Abs(dcPower.Value) * 100.0;
                efficiency = Math.Round(Math.Min(raw, 100.0), 1);
            }

            var gridModeStr = resolver?.ResolveString(ParameterKeys.PcsGridMode);
            if (gridModeStr == null)
            {
                gridModeStr = MapGridMode(ExtractInt(statusPayload, "pcs_gridMode"));
            }

            return new PcsUnitListItemDto
            {
                Id = device.Id,
                Name = device.Name,
                SerialNumber = device.SerialNumber,
                ModelName = device.Model,
                Manufacturer = device.Manufacturer,
                FacilityName = device.Rtu?.Facility?.Name,
                FacilityId = device.Rtu!.FacilityId,
                Status = StatusBadgeDto.FromStatus(statuses.GetValueOrDefault(device.Id, "normal")),
                LinkedBessUnit = linkedBess,
                GridMode = gridModeStr,
                ActivePowerKw = activePower,
                ReactivePowerKvar = reactivePower,
                GridFreqHz = resolver?.ResolveNumber(ParameterKeys.PcsGridFreq)
                    ?? ExtractDouble(statusPayload, "pcs_gridFreqHz"),
                IgbtTempC = resolver?.ResolveNumber(ParameterKeys.PcsIgbtTemp)
                    ?? ExtractDouble(tempsPayload, "pcs_igbtTempC"),
                EfficiencyPct = efficiency,
                PowerFactor = resolver?.ResolveNumber(ParameterKeys.PcsPowerFactor)
                    ?? ExtractDouble(dcPayload, "pcs_acPwrFactor"),
                ActiveFaultCount = alarmCounts.GetValueOrDefault(device.Id, 0),
                LastSeenUtc = device.LastSeenUtc
            };
        }).ToList();

        var powerValues = units.Where(u => u.ActivePowerKw.HasValue).Select(u => u.ActivePowerKw!.Value).ToList();
        var reactiveValues = units.Where(u => u.ReactivePowerKvar.HasValue).Select(u => u.ReactivePowerKvar!.Value).ToList();
        var effValues = units.Where(u => u.EfficiencyPct.HasValue).Select(u => u.EfficiencyPct!.Value).ToList();

        return new PcsUnitsResultDto
        {
            Units = units,
            Aggregates = new PcsAggregatesDto
            {
                TotalUnits = units.Count,
                OnlineCount = units.Count(u => u.Status.Value == "online"),
                OfflineCount = units.Count(u => u.Status.Value == "offline"),
                FaultCount = units.Count(u => u.Status.Value == "fault"),
                StandbyCount = units.Count(u => u.Status.Value == "standby"),
                TotalActivePowerKw = powerValues.Count > 0 ? Math.Round(powerValues.Sum(), 2) : null,
                TotalReactivePowerKvar = reactiveValues.Count > 0 ? Math.Round(reactiveValues.Sum(), 2) : null,
                TotalActiveFaults = units.Sum(u => u.ActiveFaultCount),
                AverageEfficiencyPct = effValues.Count > 0 ? Math.Round(effValues.Average(), 1) : null
            }
        };
    }

    private static string? MapGridMode(int? code) => code switch
    {
        1 => "Grid Following",
        2 => "Grid Forming",
        3 => "Droop Control",
        4 => "Off-Grid",
        _ => null
    };

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

    private static int? ExtractInt(JsonElement? element, string path)
    {
        if (!element.HasValue) return null;
        var value = PayloadDeserializer.ExtractValue(element.Value, path);
        return value switch
        {
            int i => i,
            long l => (int)l,
            double d => (int)d,
            _ => null
        };
    }
}
