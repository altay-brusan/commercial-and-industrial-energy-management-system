using System.Text.Json;
using Backend.Application.Common.Interfaces;
using Backend.Application.Common.Models.Monitoring;
using Backend.Application.Common.Security;
using Backend.Application.Common.Services;
using Backend.Domain.Constants;

namespace Backend.Application.SolarInverters.Queries.GetSolarPerformance;

[Authorize(Roles = "Administrator,Tenant.Admin")]
[RequirePermission(Backend.Domain.Constants.Permissions.SolarView)]
public record GetSolarPerformanceQuery : IRequest<SolarPerformanceDto>
{
    public Guid FacilityId { get; init; }
    public string? Period { get; init; }
}

public class GetSolarPerformanceQueryHandler : IRequestHandler<GetSolarPerformanceQuery, SolarPerformanceDto>
{
    private readonly IApplicationDbContext _context;

    public GetSolarPerformanceQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<SolarPerformanceDto> Handle(GetSolarPerformanceQuery request, CancellationToken cancellationToken)
    {
        var solarDevices = await _context.Devices
            .Include(d => d.DeviceType)
            .Include(d => d.LogicalDevice)
            .Where(d => d.Rtu!.FacilityId == request.FacilityId &&
                ((d.DeviceType != null && (d.DeviceType.Name == DeviceTypeNames.Solar || d.DeviceType.Name == DeviceTypeNames.Inverter)) ||
                 (d.LogicalDevice != null && d.LogicalDevice.Category == "Solar")))
            .ToListAsync(cancellationToken);

        var deviceIds = solarDevices.Select(d => d.Id).ToList();

        if (deviceIds.Count == 0)
            return new SolarPerformanceDto();

        // Get latest telemetry per device
        var latestByDevice = await _context.DeviceSolarData
            .Where(dd => deviceIds.Contains(dd.DeviceId) && dd.Kind == "Solar.Power")
            .GroupBy(dd => dd.DeviceId)
            .Select(g => g.OrderByDescending(dd => dd.DeviceLocalTime).First())
            .ToListAsync(cancellationToken);

        // Calculate installed capacity from metadata
        double totalInstalledCapacityKw = 0;
        foreach (var device in solarDevices)
        {
            var meta = PayloadDeserializer.GetElement(device.Metadata);
            var ratedPower = ExtractDouble(meta, "ratedPower") ?? ExtractDouble(meta, "installedCapacityKw") ?? 0;
            totalInstalledCapacityKw += ratedPower;
        }

        // Per-inverter specific yield and efficiency
        var inverterMetrics = new List<SolarInverterPerformanceDto>();
        double totalPower = 0;
        double totalDailyYield = 0;
        var efficiencies = new List<double>();

        foreach (var entry in latestByDevice)
        {
            var device = solarDevices.FirstOrDefault(d => d.Id == entry.DeviceId);
            var payload = PayloadDeserializer.GetElement(entry.Payload);

            var power = ExtractDouble(payload, "activePower") ?? 0;
            var yield = ExtractDouble(payload, "dailyYield") ?? 0;
            var efficiency = ExtractDouble(payload, "efficiency");

            totalPower += power;
            totalDailyYield += yield;
            if (efficiency.HasValue) efficiencies.Add(efficiency.Value);

            var meta = PayloadDeserializer.GetElement(device?.Metadata);
            var ratedPower = ExtractDouble(meta, "ratedPower") ?? ExtractDouble(meta, "installedCapacityKw");
            var specificYield = ratedPower > 0 ? Math.Round(yield / ratedPower.Value, 2) : (double?)null;

            inverterMetrics.Add(new SolarInverterPerformanceDto
            {
                DeviceId = entry.DeviceId,
                Name = device?.Name ?? "Unknown",
                ActivePowerKw = Math.Round(power, 2),
                DailyYieldKwh = Math.Round(yield, 2),
                EfficiencyPct = efficiency.HasValue ? Math.Round(efficiency.Value, 1) : null,
                SpecificYield = specificYield,
                CapacityUtilizationPct = ratedPower > 0 ? Math.Round(power / ratedPower.Value * 100, 1) : null
            });
        }

        var capacityUtilization = totalInstalledCapacityKw > 0
            ? Math.Round(totalPower / totalInstalledCapacityKw * 100, 1)
            : (double?)null;

        return new SolarPerformanceDto
        {
            TotalInstalledCapacityKw = Math.Round(totalInstalledCapacityKw, 2),
            TotalActivePowerKw = Math.Round(totalPower, 2),
            TotalDailyYieldKwh = Math.Round(totalDailyYield, 2),
            CapacityUtilizationPct = capacityUtilization,
            AverageEfficiencyPct = efficiencies.Count > 0 ? Math.Round(efficiencies.Average(), 1) : null,
            PeakPowerKw = inverterMetrics.Count > 0 ? inverterMetrics.Max(m => m.ActivePowerKw) : 0,
            InverterMetrics = inverterMetrics
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

public record SolarPerformanceDto
{
    public double TotalInstalledCapacityKw { get; init; }
    public double TotalActivePowerKw { get; init; }
    public double TotalDailyYieldKwh { get; init; }
    public double? CapacityUtilizationPct { get; init; }
    public double? AverageEfficiencyPct { get; init; }
    public double PeakPowerKw { get; init; }
    public List<SolarInverterPerformanceDto> InverterMetrics { get; init; } = new();
}

public record SolarInverterPerformanceDto
{
    public Guid DeviceId { get; init; }
    public string Name { get; init; } = string.Empty;
    public double ActivePowerKw { get; init; }
    public double DailyYieldKwh { get; init; }
    public double? EfficiencyPct { get; init; }
    public double? SpecificYield { get; init; }
    public double? CapacityUtilizationPct { get; init; }
}
