using System.Text.Json;
using Backend.Application.Common.Interfaces;
using Backend.Application.Common.Models.Monitoring;
using Backend.Application.Common.Security;
using Backend.Application.Common.Services;
using Backend.Domain.Constants;

namespace Backend.Application.SolarInverters.Queries.GetSolarCurves;

[Authorize(Roles = "Administrator,Tenant.Admin")]
[RequirePermission(Backend.Domain.Constants.Permissions.SolarView)]
public record GetSolarCurvesQuery : IRequest<ChartResponseDto>
{
    public Guid UnitId { get; init; }
    public string Period { get; init; } = MonitoringPeriods.TwentyFourHours;
    public string? Interval { get; init; }
}

public class GetSolarCurvesQueryValidator : AbstractValidator<GetSolarCurvesQuery>
{
    public GetSolarCurvesQueryValidator()
    {
        RuleFor(x => x.UnitId).NotEmpty();
        RuleFor(x => x.Period).Must(p => MonitoringPeriods.All.Contains(p))
            .WithMessage($"Period must be one of: {string.Join(", ", MonitoringPeriods.All)}");
        RuleFor(x => x.Interval).Must(i => i == null || MonitoringIntervals.All.Contains(i))
            .WithMessage($"Interval must be one of: {string.Join(", ", MonitoringIntervals.All)}");
    }
}

public class GetSolarCurvesQueryHandler : IRequestHandler<GetSolarCurvesQuery, ChartResponseDto>
{
    private readonly IApplicationDbContext _context;

    public GetSolarCurvesQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<ChartResponseDto> Handle(GetSolarCurvesQuery request, CancellationToken cancellationToken)
    {
        var device = await _context.Devices
            .Include(d => d.SpecificationTemplate)
            .FirstOrDefaultAsync(d => d.Id == request.UnitId, cancellationToken);

        Guard.Against.NotFound(request.UnitId, device);

        var (start, end) = GetPeriodRange(request.Period);
        var intervalMinutes = request.Interval != null
            ? GetIntervalMinutes(request.Interval)
            : GetDefaultIntervalMinutes(request.Period);
        var timeFormat = GetTimeFormat(request.Period);

        // Load Solar.Power time-series
        var rawData = await _context.DeviceSolarData
            .Where(dd => dd.DeviceId == request.UnitId
                && dd.Kind == "Solar.Power"
                && dd.DeviceLocalTime >= start && dd.DeviceLocalTime <= end)
            .OrderBy(dd => dd.DeviceLocalTime)
            .ToListAsync(cancellationToken);

        var sampled = SampleData(rawData, intervalMinutes);

        // Check if schema-driven charts exist in template
        var specs = PayloadDeserializer.GetElement(device.SpecificationTemplate?.SpecificationsTemplate);
        var charts = BuildCharts(specs, sampled, timeFormat);

        return new ChartResponseDto { Charts = charts };
    }

    private static List<ChartDefinitionDto> BuildCharts(
        JsonElement? specs,
        List<(DateTimeOffset Time, JsonElement? Payload)> data,
        string timeFormat)
    {
        if (specs.HasValue && specs.Value.TryGetProperty("curveCharts", out var chartsElement) &&
            chartsElement.ValueKind == JsonValueKind.Array)
        {
            return BuildFromSchema(chartsElement, data, timeFormat);
        }

        return BuildDefaultCharts(data, timeFormat);
    }

    private static List<ChartDefinitionDto> BuildFromSchema(
        JsonElement chartsElement, List<(DateTimeOffset Time, JsonElement? Payload)> data, string timeFormat)
    {
        var charts = new List<ChartDefinitionDto>();

        foreach (var chart in chartsElement.EnumerateArray())
        {
            var key = chart.TryGetProperty("key", out var k) ? k.GetString() ?? "" : "";
            var title = chart.TryGetProperty("title", out var t) ? t.GetString() ?? key : key;
            var chartType = chart.TryGetProperty("type", out var ct) ? ct.GetString() ?? "line" : "line";

            var seriesDefs = new List<CurveSeriesDto>();
            var sourcePaths = new List<(string Key, string SourcePath)>();

            if (chart.TryGetProperty("series", out var seriesElement) && seriesElement.ValueKind == JsonValueKind.Array)
            {
                foreach (var s in seriesElement.EnumerateArray())
                {
                    var sKey = s.TryGetProperty("key", out var sk) ? sk.GetString() ?? "" : "";
                    var sLabel = s.TryGetProperty("label", out var sl) ? sl.GetString() ?? sKey : sKey;
                    var sUnit = s.TryGetProperty("unit", out var su) ? su.GetString() : null;
                    var sColor = s.TryGetProperty("color", out var sc) ? sc.GetString() : null;
                    var sYAxis = s.TryGetProperty("yAxis", out var sy) ? sy.GetString() : null;
                    var sType = s.TryGetProperty("type", out var st) ? st.GetString() : null;
                    var sourcePath = s.TryGetProperty("sourcePath", out var sp) ? sp.GetString() : null;

                    seriesDefs.Add(new CurveSeriesDto
                    {
                        Key = sKey, Label = sLabel, Unit = sUnit, Color = sColor,
                        YAxis = sYAxis, Type = sType, SourcePath = sourcePath
                    });

                    if (sourcePath != null)
                        sourcePaths.Add((sKey, sourcePath));
                }
            }

            charts.Add(new ChartDefinitionDto
            {
                Key = key, Title = title, Type = chartType,
                XAxis = new CurveXAxisDto { Key = "time", Label = "Time", Format = timeFormat },
                Series = seriesDefs,
                Data = BuildUnifiedData(data, sourcePaths, timeFormat)
            });
        }

        return charts;
    }

    private static List<ChartDefinitionDto> BuildDefaultCharts(
        List<(DateTimeOffset Time, JsonElement? Payload)> data, string timeFormat)
    {
        return new List<ChartDefinitionDto>
        {
            BuildChart("power", "Power Output", "area", timeFormat, data,
                new CurveSeriesDto { Key = "activePower", Label = "Active Power", Unit = "kW", Color = "#f59e0b", YAxis = "left", Type = "area", SourcePath = "activePower" },
                new CurveSeriesDto { Key = "reactivePower", Label = "Reactive Power", Unit = "kVar", Color = "#8b5cf6", YAxis = "right", Type = "line", SourcePath = "reactivePower" }),

            BuildChart("voltage", "Voltage", "line", timeFormat, data,
                new CurveSeriesDto { Key = "voltageAB", Label = "V_AB", Unit = "V", Color = "#ef4444", Type = "line", SourcePath = "voltageAB" },
                new CurveSeriesDto { Key = "voltageBC", Label = "V_BC", Unit = "V", Color = "#22c55e", Type = "line", SourcePath = "voltageBC" },
                new CurveSeriesDto { Key = "voltageCA", Label = "V_CA", Unit = "V", Color = "#3b82f6", Type = "line", SourcePath = "voltageCA" }),

            BuildChart("current", "Current", "line", timeFormat, data,
                new CurveSeriesDto { Key = "currentA", Label = "I_A", Unit = "A", Color = "#ef4444", Type = "line", SourcePath = "currentA" },
                new CurveSeriesDto { Key = "currentB", Label = "I_B", Unit = "A", Color = "#22c55e", Type = "line", SourcePath = "currentB" },
                new CurveSeriesDto { Key = "currentC", Label = "I_C", Unit = "A", Color = "#3b82f6", Type = "line", SourcePath = "currentC" }),

            BuildChart("dcInput", "DC Input (MPPT)", "area", timeFormat, data,
                new CurveSeriesDto { Key = "dcPower", Label = "DC Power", Unit = "kW", Color = "#06b6d4", Type = "area", SourcePath = "dcPower" },
                new CurveSeriesDto { Key = "dcVoltage", Label = "DC Voltage", Unit = "V", Color = "#f59e0b", YAxis = "right", Type = "line", SourcePath = "dcVoltage" }),
        };
    }

    private static ChartDefinitionDto BuildChart(
        string key, string title, string type, string timeFormat,
        List<(DateTimeOffset Time, JsonElement? Payload)> data,
        params CurveSeriesDto[] seriesDefs)
    {
        var sourcePaths = seriesDefs
            .Where(s => s.SourcePath != null)
            .Select(s => (s.Key, s.SourcePath!))
            .ToList();

        return new ChartDefinitionDto
        {
            Key = key, Title = title, Type = type,
            XAxis = new CurveXAxisDto { Key = "time", Label = "Time", Format = timeFormat },
            Series = seriesDefs.ToList(),
            Data = BuildUnifiedData(data, sourcePaths, timeFormat)
        };
    }

    private static List<Dictionary<string, object?>> BuildUnifiedData(
        List<(DateTimeOffset Time, JsonElement? Payload)> data,
        List<(string Key, string SourcePath)> sourcePaths,
        string timeFormat)
    {
        return data.Select(d =>
        {
            var row = new Dictionary<string, object?> { ["time"] = d.Time.ToString(timeFormat) };

            foreach (var (seriesKey, sourcePath) in sourcePaths)
            {
                row[seriesKey] = d.Payload.HasValue
                    ? ExtractNumber(d.Payload.Value, sourcePath)
                    : null;
            }

            return row;
        }).ToList();
    }

    private static List<(DateTimeOffset Time, JsonElement? Payload)> SampleData(
        List<Domain.Entities.DeviceData.DeviceSolarData> rawData, int intervalMinutes)
    {
        if (rawData.Count == 0) return new();

        var result = new List<(DateTimeOffset, JsonElement?)>();
        var lastTimestamp = DateTimeOffset.MinValue;

        foreach (var row in rawData)
        {
            if ((row.DeviceLocalTime - lastTimestamp).TotalMinutes >= intervalMinutes)
            {
                result.Add((row.DeviceLocalTime, PayloadDeserializer.GetElement(row.Payload)));
                lastTimestamp = row.DeviceLocalTime;
            }
        }

        return result;
    }

    private static object? ExtractNumber(JsonElement payload, string path)
    {
        var value = PayloadDeserializer.ExtractValue(payload, path);
        return value switch
        {
            double d => Math.Round(d, 1),
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
            MonitoringPeriods.OneHour => (now.AddHours(-1), now),
            MonitoringPeriods.SixHours => (now.AddHours(-6), now),
            MonitoringPeriods.TwelveHours => (now.AddHours(-12), now),
            MonitoringPeriods.TwentyFourHours => (now.AddHours(-24), now),
            MonitoringPeriods.FortyEightHours => (now.AddHours(-48), now),
            MonitoringPeriods.SevenDays => (now.AddDays(-7), now),
            MonitoringPeriods.ThirtyDays => (now.AddDays(-30), now),
            _ => (now.AddHours(-24), now)
        };
    }

    private static int GetDefaultIntervalMinutes(string period) => period switch
    {
        MonitoringPeriods.OneHour => 1,
        MonitoringPeriods.SixHours => 5,
        MonitoringPeriods.TwelveHours => 15,
        MonitoringPeriods.TwentyFourHours => 15,
        MonitoringPeriods.FortyEightHours => 30,
        MonitoringPeriods.SevenDays => 60,
        MonitoringPeriods.ThirtyDays => 1440,
        _ => 15
    };

    private static int GetIntervalMinutes(string interval) => interval switch
    {
        MonitoringIntervals.OneMin => 1,
        MonitoringIntervals.FiveMin => 5,
        MonitoringIntervals.FifteenMin => 15,
        MonitoringIntervals.ThirtyMin => 30,
        MonitoringIntervals.OneHour => 60,
        MonitoringIntervals.OneDay => 1440,
        _ => 15
    };

    private static string GetTimeFormat(string period) => period switch
    {
        MonitoringPeriods.OneHour => "HH:mm",
        MonitoringPeriods.SixHours => "HH:mm",
        MonitoringPeriods.TwelveHours => "HH:mm",
        MonitoringPeriods.TwentyFourHours => "HH:mm",
        MonitoringPeriods.FortyEightHours => "dd/MM HH:mm",
        MonitoringPeriods.SevenDays => "dd/MM HH:mm",
        MonitoringPeriods.ThirtyDays => "dd MMM",
        _ => "HH:mm"
    };
}
