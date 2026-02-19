using System.Text.Json;
using Backend.Application.Common.Interfaces;
using Backend.Application.Common.Models.Monitoring;
using Backend.Application.Common.Security;
using Backend.Application.Common.Services;

namespace Backend.Application.BatteryStorage.Queries.GetBessCurves;

[Authorize(Roles = "Administrator,Tenant.Admin")]
[RequirePermission(Backend.Domain.Constants.Permissions.BessView)]
public record GetBessCurvesQuery : IRequest<ChartResponseDto>
{
    public Guid UnitId { get; init; }
    public string Period { get; init; } = MonitoringPeriods.Day;
    public string Interval { get; init; } = MonitoringIntervals.FifteenMin;
}

public class GetBessCurvesQueryValidator : AbstractValidator<GetBessCurvesQuery>
{
    public GetBessCurvesQueryValidator()
    {
        RuleFor(x => x.UnitId).NotEmpty();
        RuleFor(x => x.Period).Must(p => MonitoringPeriods.All.Contains(p))
            .WithMessage($"Period must be one of: {string.Join(", ", MonitoringPeriods.All)}");
        RuleFor(x => x.Interval).Must(i => MonitoringIntervals.All.Contains(i))
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
            .Include(d => d.DeviceModel)
            .Where(d => d.Id == request.UnitId && d.DeviceType != null && d.DeviceType.Name == "Battery")
            .FirstOrDefaultAsync(cancellationToken);

        Guard.Against.NotFound(request.UnitId, device);

        var (start, end) = GetPeriodRange(request.Period);
        var intervalMinutes = GetIntervalMinutes(request.Interval);

        var rawData = await _context.DeviceBessData
            .Where(dd => dd.DeviceId == request.UnitId && dd.DeviceLocalTime >= start && dd.DeviceLocalTime <= end)
            .OrderBy(dd => dd.DeviceLocalTime)
            .ToListAsync(cancellationToken);

        var sampled = SampleData(rawData, intervalMinutes);
        var specs = PayloadDeserializer.GetElement(device.DeviceModel?.Specifications);
        var charts = BuildCharts(specs, sampled);

        return new ChartResponseDto
        {
            Charts = charts,
            Period = request.Period,
            Interval = request.Interval
        };
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

    private static List<(DateTimeOffset Time, JsonElement? Payload)> SampleData(
        List<Domain.Entities.DeviceData.DeviceBessData> rawData, int intervalMinutes)
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
        JsonElement? specs, List<(DateTimeOffset Time, JsonElement? Payload)> data)
    {
        if (specs.HasValue && specs.Value.TryGetProperty("curveCharts", out var chartsElement) &&
            chartsElement.ValueKind == JsonValueKind.Array)
        {
            return BuildFromSchema(chartsElement, data);
        }

        return BuildDefaultCharts(data);
    }

    private static List<ChartDefinitionDto> BuildFromSchema(
        JsonElement chartsElement, List<(DateTimeOffset Time, JsonElement? Payload)> data)
    {
        var charts = new List<ChartDefinitionDto>();
        var labels = data.Select(d => d.Time.ToString("yyyy-MM-ddTHH:mm:ssZ")).ToList();

        foreach (var chart in chartsElement.EnumerateArray())
        {
            var key = chart.TryGetProperty("key", out var k) ? k.GetString() ?? "" : "";
            var title = chart.TryGetProperty("title", out var t) ? t.GetString() ?? key : key;
            var chartType = chart.TryGetProperty("chartType", out var ct) ? ct.GetString() ?? "line" : "line";

            var series = new List<ChartSeriesDto>();
            if (chart.TryGetProperty("series", out var seriesElement) && seriesElement.ValueKind == JsonValueKind.Array)
            {
                foreach (var s in seriesElement.EnumerateArray())
                {
                    var sKey = s.TryGetProperty("key", out var sk) ? sk.GetString() ?? "" : "";
                    var sLabel = s.TryGetProperty("label", out var sl) ? sl.GetString() ?? sKey : sKey;
                    var sUnit = s.TryGetProperty("unit", out var su) ? su.GetString() : null;
                    var sColor = s.TryGetProperty("color", out var sc) ? sc.GetString() : null;
                    var sourcePath = s.TryGetProperty("sourcePath", out var sp) ? sp.GetString() : null;

                    var values = data.Select(d =>
                        d.Payload.HasValue && sourcePath != null
                            ? PayloadDeserializer.ExtractValue(d.Payload.Value, sourcePath)
                            : null).ToList();

                    series.Add(new ChartSeriesDto
                    {
                        Key = sKey, Label = sLabel, Unit = sUnit, Color = sColor,
                        SourcePath = sourcePath, Data = values
                    });
                }
            }

            charts.Add(new ChartDefinitionDto
            {
                Key = key, Title = title, ChartType = chartType,
                XAxis = new XAxisDto { Type = "time", Labels = labels },
                Series = series
            });
        }

        return charts;
    }

    private static List<ChartDefinitionDto> BuildDefaultCharts(
        List<(DateTimeOffset Time, JsonElement? Payload)> data)
    {
        var labels = data.Select(d => d.Time.ToString("yyyy-MM-ddTHH:mm:ssZ")).ToList();

        return new List<ChartDefinitionDto>
        {
            new()
            {
                Key = "power", Title = "DC Power", ChartType = "line",
                XAxis = new XAxisDto { Type = "time", Labels = labels },
                Series = new List<ChartSeriesDto>
                {
                    ExtractSeries("dcPower", "DC Power", "kW", "#3b82f6", "dcPower.power", data),
                }
            },
            new()
            {
                Key = "soc", Title = "State of Charge", ChartType = "line",
                XAxis = new XAxisDto { Type = "time", Labels = labels },
                Series = new List<ChartSeriesDto>
                {
                    ExtractSeries("soc", "SoC", "%", "#10b981", "stateOfCharge", data),
                }
            },
            new()
            {
                Key = "temperature", Title = "Temperature", ChartType = "line",
                XAxis = new XAxisDto { Type = "time", Labels = labels },
                Series = new List<ChartSeriesDto>
                {
                    ExtractSeries("batteryTemp", "Battery Temp", "°C", "#f59e0b", "batteryTemperature", data),
                }
            },
            new()
            {
                Key = "voltage", Title = "DC Voltage", ChartType = "line",
                XAxis = new XAxisDto { Type = "time", Labels = labels },
                Series = new List<ChartSeriesDto>
                {
                    ExtractSeries("dcVoltage", "DC Voltage", "V", "#8b5cf6", "dcPower.voltage", data),
                }
            }
        };
    }

    private static ChartSeriesDto ExtractSeries(
        string key, string label, string unit, string color, string sourcePath,
        List<(DateTimeOffset Time, JsonElement? Payload)> data)
    {
        var values = data.Select(d =>
            d.Payload.HasValue ? PayloadDeserializer.ExtractValue(d.Payload.Value, sourcePath) : null).ToList();

        return new ChartSeriesDto
        {
            Key = key, Label = label, Unit = unit, Color = color,
            SourcePath = sourcePath, Data = values
        };
    }
}
