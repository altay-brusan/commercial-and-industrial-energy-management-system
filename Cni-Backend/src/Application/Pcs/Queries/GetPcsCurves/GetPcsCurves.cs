using System.Text.Json;
using Backend.Application.Common.Interfaces;
using Backend.Application.Common.Models.Monitoring;
using Backend.Application.Common.Security;
using Backend.Application.Common.Services;

namespace Backend.Application.Pcs.Queries.GetPcsCurves;

[Authorize(Roles = "Administrator,Tenant.Admin")]
[RequirePermission(Backend.Domain.Constants.Permissions.PcsView)]
public record GetPcsCurvesQuery : IRequest<ChartResponseDto>
{
    public Guid UnitId { get; init; }
    public string Period { get; init; } = MonitoringPeriods.Day;
    public string Interval { get; init; } = MonitoringIntervals.FifteenMin;
}

public class GetPcsCurvesQueryValidator : AbstractValidator<GetPcsCurvesQuery>
{
    public GetPcsCurvesQueryValidator()
    {
        RuleFor(x => x.UnitId).NotEmpty();
        RuleFor(x => x.Period).Must(p => MonitoringPeriods.All.Contains(p))
            .WithMessage($"Period must be one of: {string.Join(", ", MonitoringPeriods.All)}");
        RuleFor(x => x.Interval).Must(i => MonitoringIntervals.All.Contains(i))
            .WithMessage($"Interval must be one of: {string.Join(", ", MonitoringIntervals.All)}");
    }
}

public class GetPcsCurvesQueryHandler : IRequestHandler<GetPcsCurvesQuery, ChartResponseDto>
{
    private readonly IApplicationDbContext _context;

    public GetPcsCurvesQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<ChartResponseDto> Handle(GetPcsCurvesQuery request, CancellationToken cancellationToken)
    {
        var device = await _context.Devices
            .Include(d => d.DeviceType)
            .Include(d => d.SpecificationTemplate)
            .FirstOrDefaultAsync(d => d.Id == request.UnitId, cancellationToken);

        Guard.Against.NotFound(request.UnitId, device);

        var (start, end) = GetPeriodRange(request.Period);
        var intervalMinutes = GetIntervalMinutes(request.Interval);
        var timeFormat = GetTimeFormat(request.Period);

        var rawData = await _context.DevicePcsData
            .Where(dd => dd.DeviceId == request.UnitId && dd.DeviceLocalTime >= start && dd.DeviceLocalTime <= end)
            .OrderBy(dd => dd.DeviceLocalTime)
            .ToListAsync(cancellationToken);

        var sampled = SampleData(rawData, intervalMinutes);
        var specs = PayloadDeserializer.GetElement(device.SpecificationTemplate?.SpecificationsTemplate);
        var charts = BuildCharts(specs, sampled, timeFormat);

        return new ChartResponseDto { Charts = charts };
    }

    private static (DateTimeOffset start, DateTimeOffset end) GetPeriodRange(string period)
    {
        var now = DateTimeOffset.UtcNow;
        return period switch
        {
            MonitoringPeriods.Day => (now.AddDays(-1), now),
            MonitoringPeriods.Week => (now.AddDays(-7), now),
            MonitoringPeriods.Month => (now.AddMonths(-1), now),
            MonitoringPeriods.Year => (now.AddYears(-1), now),
            _ => (now.AddDays(-1), now)
        };
    }

    private static int GetIntervalMinutes(string interval) => interval switch
    {
        MonitoringIntervals.FiveMin => 5,
        MonitoringIntervals.FifteenMin => 15,
        MonitoringIntervals.OneHour => 60,
        MonitoringIntervals.OneDay => 1440,
        _ => 15
    };

    private static string GetTimeFormat(string period) => period switch
    {
        MonitoringPeriods.Day => "HH:mm",
        MonitoringPeriods.Week => "ddd HH:mm",
        MonitoringPeriods.Month => "dd MMM",
        MonitoringPeriods.Year => "MMM yy",
        _ => "HH:mm"
    };

    private static List<(DateTimeOffset Time, JsonElement? Payload)> SampleData(
        List<Domain.Entities.DeviceData.DevicePcsData> rawData, int intervalMinutes)
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

    private static List<ChartDefinitionDto> BuildCharts(
        JsonElement? specs, List<(DateTimeOffset Time, JsonElement? Payload)> data, string timeFormat)
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
            var xFormat = chart.TryGetProperty("xAxis", out var xa) && xa.TryGetProperty("format", out var xf)
                ? xf.GetString() ?? timeFormat : timeFormat;

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

            var chartData = BuildUnifiedData(data, sourcePaths, xFormat);

            charts.Add(new ChartDefinitionDto
            {
                Key = key, Title = title, Type = chartType,
                XAxis = new CurveXAxisDto { Key = "time", Label = "Time", Format = xFormat },
                Series = seriesDefs,
                Data = chartData
            });
        }

        return charts;
    }

    private static List<ChartDefinitionDto> BuildDefaultCharts(
        List<(DateTimeOffset Time, JsonElement? Payload)> data, string timeFormat)
    {
        return new List<ChartDefinitionDto>
        {
            BuildChart("acPower", "AC Power", "area", timeFormat, data,
                new CurveSeriesDto { Key = "activePower", Label = "Active Power", Unit = "kW", Color = "#3b82f6", YAxis = "left", Type = "area", SourcePath = "acPower.totalActivePower" },
                new CurveSeriesDto { Key = "reactivePower", Label = "Reactive Power", Unit = "kVAR", Color = "#f59e0b", YAxis = "right", Type = "line", SourcePath = "acPower.totalReactivePower" }),

            BuildChart("dcPower", "DC Power", "line", timeFormat, data,
                new CurveSeriesDto { Key = "dcPower", Label = "DC Power", Unit = "kW", Color = "#10b981", YAxis = "left", Type = "line", SourcePath = "dcPower.power" },
                new CurveSeriesDto { Key = "dcVoltage", Label = "DC Voltage", Unit = "V", Color = "#8b5cf6", YAxis = "right", Type = "line", SourcePath = "dcPower.voltage" }),

            BuildChart("frequency", "Grid Frequency", "line", timeFormat, data,
                new CurveSeriesDto { Key = "gridFreq", Label = "Frequency", Unit = "Hz", Color = "#06b6d4", Type = "line", SourcePath = "acPower.frequency" }),

            BuildChart("temperature", "Temperature", "area", timeFormat, data,
                new CurveSeriesDto { Key = "pcsTemp", Label = "PCS Temperature", Unit = "\u00b0C", Color = "#ef4444", Type = "area", SourcePath = "temperaturePcs" }),

            BuildChart("efficiency", "Efficiency", "line", timeFormat, data,
                new CurveSeriesDto { Key = "efficiency", Label = "Efficiency", Unit = "%", Color = "#22c55e", Type = "line", SourcePath = "efficiency" }),
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
            var row = new Dictionary<string, object?>
            {
                ["time"] = d.Time.ToString(timeFormat)
            };

            foreach (var (seriesKey, sourcePath) in sourcePaths)
            {
                row[seriesKey] = d.Payload.HasValue
                    ? ExtractNumber(d.Payload.Value, sourcePath)
                    : null;
            }

            return row;
        }).ToList();
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
}
