using System.Text.Json;
using Backend.Application.Common.Interfaces;
using Backend.Application.Common.Models.Monitoring;
using Backend.Application.Common.Security;
using Backend.Application.Common.Services;
using Backend.Domain.Constants;

namespace Backend.Application.BatteryStorage.Queries.GetBessCurves;

// TODO: Review period/interval validation — the sampling interval should be validated
// against the selected period to prevent nonsensical combinations (e.g. 1d interval on 1h period).
// Consider deriving a default interval from the period when the client doesn't provide one,
// and moving chart series definitions + source paths to the ParameterMapTemplate schema
// so they are config-driven instead of hardcoded per device type.

[Authorize(Roles = "Administrator,Tenant.Admin")]
[RequirePermission(Backend.Domain.Constants.Permissions.BessView)]
public record GetBessCurvesQuery : IRequest<ChartResponseDto>
{
    public Guid UnitId { get; init; }
    public string Period { get; init; } = MonitoringPeriods.TwentyFourHours;
    public string? Interval { get; init; }
}

public class GetBessCurvesQueryValidator : AbstractValidator<GetBessCurvesQuery>
{
    public GetBessCurvesQueryValidator()
    {
        RuleFor(x => x.UnitId).NotEmpty();
        RuleFor(x => x.Period).Must(p => MonitoringPeriods.All.Contains(p))
            .WithMessage($"Period must be one of: {string.Join(", ", MonitoringPeriods.All)}");
        RuleFor(x => x.Interval).Must(i => i == null || MonitoringIntervals.All.Contains(i))
            .WithMessage($"Interval must be one of: {string.Join(", ", MonitoringIntervals.All)}");
    }
}

public class GetBessCurvesQueryHandler : IRequestHandler<GetBessCurvesQuery, ChartResponseDto>
{
    private readonly IApplicationDbContext _context;

    public GetBessCurvesQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<ChartResponseDto> Handle(GetBessCurvesQuery request, CancellationToken cancellationToken)
    {
        var device = await _context.Devices
            .Include(d => d.DeviceType)
            .Include(d => d.LogicalDevice)
            .Include(d => d.SpecificationTemplate)
            .FirstOrDefaultAsync(d => d.Id == request.UnitId, cancellationToken);

        Guard.Against.NotFound(request.UnitId, device);

        var (start, end) = GetPeriodRange(request.Period);
        var intervalMinutes = request.Interval != null
            ? GetIntervalMinutes(request.Interval)
            : GetDefaultIntervalMinutes(request.Period);
        var timeFormat = GetTimeFormat(request.Period);

        // Resolve battery data Kind from template (default: Battery.CellArray)
        var specs = PayloadDeserializer.GetElement(device.SpecificationTemplate?.SpecificationsTemplate);
        var batteryDataKind = "Battery.CellArray";
        if (specs.HasValue && specs.Value.TryGetProperty("batteryDataKind", out var kindProp)
            && kindProp.ValueKind == JsonValueKind.String)
        {
            batteryDataKind = kindProp.GetString() ?? batteryDataKind;
        }

        // Load Battery table time-series for root device
        var rawData = await _context.DeviceBatteryData
            .Where(dd => dd.DeviceId == request.UnitId
                && dd.Kind == batteryDataKind
                && dd.DeviceLocalTime >= start && dd.DeviceLocalTime <= end)
            .OrderBy(dd => dd.DeviceLocalTime)
            .ToListAsync(cancellationToken);

        var sampled = SampleData(rawData, intervalMinutes);

        // Also load PCS data from child PCS devices
        var pcsChildIds = await _context.Devices
            .Include(d => d.DeviceType)
            .Include(d => d.LogicalDevice)
            .Where(d => d.ParentDeviceId == request.UnitId &&
                ((d.DeviceType != null && d.DeviceType.Name == DeviceTypeNames.Pcs) ||
                 (d.LogicalDevice != null && d.LogicalDevice.Category == "PCS")))
            .Select(d => d.Id)
            .ToListAsync(cancellationToken);

        List<(DateTimeOffset Time, JsonElement? Payload)> pcsSampled = new();
        if (pcsChildIds.Count > 0)
        {
            var pcsRawData = await _context.DevicePcsData
                .Where(dd => pcsChildIds.Contains(dd.DeviceId) && dd.DeviceLocalTime >= start && dd.DeviceLocalTime <= end)
                .OrderBy(dd => dd.DeviceLocalTime)
                .ToListAsync(cancellationToken);

            pcsSampled = SamplePcsData(pcsRawData, intervalMinutes);
        }

        var charts = BuildCharts(specs, sampled, timeFormat, pcsSampled);

        return new ChartResponseDto { Charts = charts };
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

    /// <summary>
    /// Default sampling interval when the client does not provide one.
    /// </summary>
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

    private static List<(DateTimeOffset Time, JsonElement? Payload)> SampleData(
        List<Domain.Entities.DeviceData.DeviceBatteryData> rawData, int intervalMinutes)
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

    private static List<(DateTimeOffset Time, JsonElement? Payload)> SamplePcsData(
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
        JsonElement? specs,
        List<(DateTimeOffset Time, JsonElement? Payload)> data,
        string timeFormat,
        List<(DateTimeOffset Time, JsonElement? Payload)>? pcsData = null)
    {
        if (specs.HasValue && specs.Value.TryGetProperty("curveCharts", out var chartsElement) &&
            chartsElement.ValueKind == JsonValueKind.Array)
        {
            return BuildFromSchema(chartsElement, data, timeFormat);
        }

        return BuildDefaultCharts(data, timeFormat, pcsData);
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

    // TODO: Remove this entire hardcoded fallback and make curves fully config-driven.
    //
    // PROBLEM:
    // BuildDefaultCharts uses source paths and computed fields that are specific to the
    // Tesla Megapack Battery.Energy payload shape. For example:
    //   - "socPercent", "totalVoltageV", "instantCurrentA", "maxCellTempC" are Tesla field names
    //   - "@computed:power" hardcodes the formula: totalVoltageV * instantCurrentA / 1000
    // A different manufacturer (e.g. BYD, Sungrow) will send Battery.Energy with
    // completely different field names (e.g. "batteryVoltage", "stateOfCharge", "dcPowerKw"),
    // causing all chart series to return null because the paths won't match.
    //
    // SOLUTION:
    // We already have the ParameterResolver + FormulaEvaluator + ParameterMapParser system
    // (see Application/Common/Services/ParameterMapper/) that resolves parameters dynamically
    // per device model. Each model's DeviceSpecificationTemplate.ParameterMapTemplate defines:
    //   - Source parameters: { "type": "source", "sourcePath": "socPercent" }
    //   - Computed parameters: { "type": "formula", "formula": "multiply(totalVoltageV, instantCurrentA, 0.001)" }
    //
    // To fix this:
    // 1. Add a "curveCharts" section to ParameterMapTemplate (or SpecificationsTemplate) for
    //    each device model, defining chart keys, titles, series, and which parameter keys to use.
    // 2. In the handler, iterate over time-series rows, call ParameterResolver.ResolveAsync()
    //    per row to compute each series value using the model-specific parameter definitions.
    // 3. Remove BuildDefaultCharts entirely — BuildFromSchema (the schema-driven path above)
    //    should become the only path, reading chart definitions from the template.
    // 4. Remove the "@computed:power" hack from ExtractNumber — power computation should be
    //    a formula definition in the ParameterMapTemplate like all other computed parameters.
    //
    // This makes the curves endpoint fully dynamic: adding a new device model only requires
    // seeding its ParameterMapTemplate with the right field mappings — zero C# code changes.
    private static List<ChartDefinitionDto> BuildDefaultCharts(
        List<(DateTimeOffset Time, JsonElement? Payload)> data,
        string timeFormat,
        List<(DateTimeOffset Time, JsonElement? Payload)>? pcsData = null)
    {
        var charts = new List<ChartDefinitionDto>
        {
            BuildChart("powerSoc", "Power & State of Charge", "area", timeFormat, data,
                new CurveSeriesDto { Key = "power", Label = "Power", Unit = "kW", Color = "#8b5cf6", YAxis = "left", Type = "area", SourcePath = "@computed:power" },
                new CurveSeriesDto { Key = "soc", Label = "SoC", Unit = "%", Color = "#22c55e", YAxis = "right", Type = "line", SourcePath = "socPercent" }),

            BuildChart("voltageCurrent", "Voltage & Current", "line", timeFormat, data,
                new CurveSeriesDto { Key = "voltage", Label = "Voltage", Unit = "V", Color = "#3b82f6", YAxis = "left", Type = "line", SourcePath = "totalVoltageV" },
                new CurveSeriesDto { Key = "current", Label = "Current", Unit = "A", Color = "#f59e0b", YAxis = "right", Type = "line", SourcePath = "instantCurrentA" }),

            BuildChart("temperature", "Temperature", "area", timeFormat, data,
                new CurveSeriesDto { Key = "tempMax", Label = "T.Max", Unit = "\u00b0C", Color = "#ef4444", Type = "area", SourcePath = "maxCellTempC" },
                new CurveSeriesDto { Key = "tempAvg", Label = "T.Avg", Unit = "\u00b0C", Color = "#f59e0b", Type = "area", SourcePath = "ambientTempC" },
                new CurveSeriesDto { Key = "tempMin", Label = "T.Min", Unit = "\u00b0C", Color = "#3b82f6", Type = "area", SourcePath = "minCellTempC" }),
        };

        // Add PCS chart if child PCS data is available
        if (pcsData is { Count: > 0 })
        {
            charts.Add(BuildChart("pcsAcPower", "PCS AC Power", "line", timeFormat, pcsData,
                new CurveSeriesDto { Key = "acActivePower", Label = "Active Power", Unit = "kW", Color = "#06b6d4", YAxis = "left", Type = "line", SourcePath = "acPower.totalActivePower" },
                new CurveSeriesDto { Key = "efficiency", Label = "Efficiency", Unit = "%", Color = "#a855f7", YAxis = "right", Type = "line", SourcePath = "efficiency" }));
        }

        return charts;
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
        // Computed fields: voltage * current / 1000 = power in kW
        if (path == "@computed:power")
        {
            var voltage = PayloadDeserializer.ExtractValue(payload, "totalVoltageV");
            var current = PayloadDeserializer.ExtractValue(payload, "instantCurrentA");
            if (voltage is double v && current is double c)
                return Math.Round(v * c / 1000.0, 1);
            if (voltage is int vi && current is int ci)
                return Math.Round((double)vi * ci / 1000.0, 1);
            // Mixed types
            var vd = voltage switch { double d => d, int i => (double)i, long l => (double)l, _ => (double?)null };
            var cd = current switch { double d => d, int i => (double)i, long l => (double)l, _ => (double?)null };
            if (vd.HasValue && cd.HasValue)
                return Math.Round(vd.Value * cd.Value / 1000.0, 1);
            return null;
        }

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
