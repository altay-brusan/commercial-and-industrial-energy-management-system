using System.Text.Json;
using Backend.Application.Common.Interfaces;
using Backend.Application.Common.Models.Monitoring;
using Backend.Application.Common.Security;
using Backend.Application.Common.Services;
using Backend.Domain.Constants;

namespace Backend.Application.SolarInverters.Queries.GetSolarEnergyAnalytics;

[Authorize(Roles = "Administrator,Tenant.Admin")]
[RequirePermission(Backend.Domain.Constants.Permissions.SolarView)]
public record GetSolarEnergyAnalyticsQuery : IRequest<SolarEnergyAnalyticsDto>
{
    public Guid FacilityId { get; init; }
    public string? Period { get; init; }
}

public class GetSolarEnergyAnalyticsQueryHandler : IRequestHandler<GetSolarEnergyAnalyticsQuery, SolarEnergyAnalyticsDto>
{
    private readonly IApplicationDbContext _context;

    public GetSolarEnergyAnalyticsQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<SolarEnergyAnalyticsDto> Handle(GetSolarEnergyAnalyticsQuery request, CancellationToken cancellationToken)
    {
        // Find all solar devices in facility
        var solarDevices = await _context.Devices
            .Include(d => d.DeviceType)
            .Include(d => d.LogicalDevice)
            .Where(d => d.Rtu!.FacilityId == request.FacilityId &&
                ((d.DeviceType != null && (d.DeviceType.Name == DeviceTypeNames.Solar || d.DeviceType.Name == DeviceTypeNames.Inverter)) ||
                 (d.LogicalDevice != null && d.LogicalDevice.Category == "Solar")))
            .ToListAsync(cancellationToken);

        var deviceIds = solarDevices.Select(d => d.Id).ToList();

        if (deviceIds.Count == 0)
            return new SolarEnergyAnalyticsDto();

        var (start, end) = GetPeriodRange(request.Period ?? "24h");

        // Load latest Solar.Power per device for stats
        var latestByDevice = await _context.DeviceSolarData
            .Where(dd => deviceIds.Contains(dd.DeviceId) && dd.Kind == "Solar.Power")
            .GroupBy(dd => dd.DeviceId)
            .Select(g => g.OrderByDescending(dd => dd.DeviceLocalTime).First())
            .ToListAsync(cancellationToken);

        double totalActivePowerKw = 0;
        double totalDailyYieldKwh = 0;
        double totalTotalYieldKwh = 0;

        foreach (var entry in latestByDevice)
        {
            var payload = PayloadDeserializer.GetElement(entry.Payload);
            totalActivePowerKw += ExtractDouble(payload, "activePower") ?? 0;
            totalDailyYieldKwh += ExtractDouble(payload, "dailyYield") ?? 0;
            totalTotalYieldKwh += ExtractDouble(payload, "totalYield") ?? 0;
        }

        // Plant power curve: aggregate Solar.Power over time
        var timeSeriesData = await _context.DeviceSolarData
            .Where(dd => deviceIds.Contains(dd.DeviceId)
                && dd.Kind == "Solar.Power"
                && dd.DeviceLocalTime >= start && dd.DeviceLocalTime <= end)
            .OrderBy(dd => dd.DeviceLocalTime)
            .ToListAsync(cancellationToken);

        var intervalMinutes = GetDefaultIntervalMinutes(request.Period ?? "24h");
        var timeFormat = GetTimeFormat(request.Period ?? "24h");

        // Group by time bucket and sum power across all devices
        var bucketedData = timeSeriesData
            .GroupBy(d => BucketTime(d.DeviceLocalTime, intervalMinutes))
            .OrderBy(g => g.Key)
            .Select(g =>
            {
                double power = 0;
                foreach (var entry in g)
                {
                    var p = PayloadDeserializer.GetElement(entry.Payload);
                    power += ExtractDouble(p, "activePower") ?? 0;
                }
                return new Dictionary<string, object?>
                {
                    ["time"] = g.Key.ToString(timeFormat),
                    ["power"] = Math.Round(power, 1)
                };
            })
            .ToList();

        // Per-inverter yield comparison
        var inverterYields = latestByDevice.Select(entry =>
        {
            var device = solarDevices.FirstOrDefault(d => d.Id == entry.DeviceId);
            var payload = PayloadDeserializer.GetElement(entry.Payload);
            return new Dictionary<string, object?>
            {
                ["name"] = device?.Name ?? "Unknown",
                ["dailyYield"] = ExtractDouble(payload, "dailyYield"),
                ["totalYield"] = ExtractDouble(payload, "totalYield")
            };
        }).ToList();

        return new SolarEnergyAnalyticsDto
        {
            Stats = new SolarAnalyticsStatsDto
            {
                TotalActivePowerKw = Math.Round(totalActivePowerKw, 2),
                TotalDailyYieldKwh = Math.Round(totalDailyYieldKwh, 2),
                TotalYieldKwh = Math.Round(totalTotalYieldKwh, 2),
                InverterCount = deviceIds.Count,
                OnlineCount = latestByDevice.Count
            },
            PlantPowerCurve = new ChartDefinitionDto
            {
                Key = "plantPower",
                Title = "Plant Power Curve",
                Type = "area",
                XAxis = new CurveXAxisDto { Key = "time", Label = "Time", Format = timeFormat },
                Series = new List<CurveSeriesDto>
                {
                    new() { Key = "power", Label = "Total Power", Unit = "kW", Color = "#f59e0b", Type = "area" }
                },
                Data = bucketedData
            },
            InverterYieldComparison = inverterYields
        };
    }

    private static DateTimeOffset BucketTime(DateTimeOffset time, int intervalMinutes)
    {
        var totalMinutes = (long)time.TimeOfDay.TotalMinutes;
        var bucket = totalMinutes / intervalMinutes * intervalMinutes;
        return time.Date.Add(TimeSpan.FromMinutes(bucket));
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

    private static (DateTimeOffset start, DateTimeOffset end) GetPeriodRange(string period)
    {
        var now = DateTimeOffset.UtcNow;
        return period switch
        {
            "1h" => (now.AddHours(-1), now),
            "6h" => (now.AddHours(-6), now),
            "12h" => (now.AddHours(-12), now),
            "24h" => (now.AddHours(-24), now),
            "48h" => (now.AddHours(-48), now),
            "7d" => (now.AddDays(-7), now),
            "30d" => (now.AddDays(-30), now),
            _ => (now.AddHours(-24), now)
        };
    }

    private static int GetDefaultIntervalMinutes(string period) => period switch
    {
        "1h" => 1, "6h" => 5, "12h" => 15, "24h" => 15,
        "48h" => 30, "7d" => 60, "30d" => 1440, _ => 15
    };

    private static string GetTimeFormat(string period) => period switch
    {
        "1h" or "6h" or "12h" or "24h" => "HH:mm",
        "48h" or "7d" => "dd/MM HH:mm",
        "30d" => "dd MMM",
        _ => "HH:mm"
    };
}

public record SolarEnergyAnalyticsDto
{
    public SolarAnalyticsStatsDto Stats { get; init; } = new();
    public ChartDefinitionDto? PlantPowerCurve { get; init; }
    public List<Dictionary<string, object?>> InverterYieldComparison { get; init; } = new();
}

public record SolarAnalyticsStatsDto
{
    public double TotalActivePowerKw { get; init; }
    public double TotalDailyYieldKwh { get; init; }
    public double TotalYieldKwh { get; init; }
    public int InverterCount { get; init; }
    public int OnlineCount { get; init; }
}
