using System.Text.Json;
using Backend.Application.Common.Interfaces;
using Backend.Application.Common.Models.Monitoring;
using Backend.Application.Common.Security;
using Backend.Application.Common.Services;
using Backend.Domain.Constants;

namespace Backend.Application.Pcs.Queries.GetPcsCurves;

[Authorize(Roles = "Administrator,Tenant.Admin")]
[RequirePermission(Backend.Domain.Constants.Permissions.PcsView)]
public record GetPcsCurvesQuery : IRequest<ChartResponseDto>
{
    public Guid UnitId { get; init; }
    public string Period { get; init; } = MonitoringPeriods.TwentyFourHours;
    public string? Interval { get; init; }
}

public class GetPcsCurvesQueryValidator : AbstractValidator<GetPcsCurvesQuery>
{
    public GetPcsCurvesQueryValidator()
    {
        RuleFor(x => x.UnitId).NotEmpty();
        RuleFor(x => x.Period).Must(p => MonitoringPeriods.All.Contains(p))
            .WithMessage($"Period must be one of: {string.Join(", ", MonitoringPeriods.All)}");
        RuleFor(x => x.Interval).Must(i => i == null || MonitoringIntervals.All.Contains(i))
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
        _ = await _context.Devices
            .Include(d => d.LogicalDevice)
            .Where(d => d.Id == request.UnitId &&
                ((d.DeviceType != null && d.DeviceType.Name == DeviceTypeNames.Pcs) ||
                 (d.LogicalDevice != null && d.LogicalDevice.Category == "PCS")))
            .FirstOrDefaultAsync(cancellationToken)
            ?? throw new NotFoundException(request.UnitId.ToString(), "Device");

        var (start, end) = GetPeriodRange(request.Period);
        var intervalMinutes = request.Interval != null
            ? GetIntervalMinutes(request.Interval)
            : GetDefaultIntervalMinutes(request.Period);
        var timeFormat = GetTimeFormat(request.Period);

        var rawData = await _context.DevicePcsData
            .Where(dd => dd.DeviceId == request.UnitId && dd.DeviceLocalTime >= start && dd.DeviceLocalTime <= end)
            .OrderBy(dd => dd.DeviceLocalTime)
            .ToListAsync(cancellationToken);

        var buckets = BucketByInterval(rawData, intervalMinutes);

        return new ChartResponseDto { Charts = BuildCharts(buckets, timeFormat) };
    }

    // ── Time bucketing ───────────────────────────────────────────────────────

    /// <summary>
    /// Groups raw PCS data rows into time buckets, merging payloads from all Kinds
    /// within each bucket so a single data point has access to power, DC, AC, temp values.
    /// </summary>
    private static List<(DateTimeOffset Time, List<JsonElement> Payloads)> BucketByInterval(
        List<Domain.Entities.DeviceData.DevicePcsData> rawData, int intervalMinutes)
    {
        if (rawData.Count == 0) return new();

        var result = new List<(DateTimeOffset, List<JsonElement>)>();
        var bucketStart = rawData[0].DeviceLocalTime;
        var currentPayloads = new List<JsonElement>();

        foreach (var row in rawData)
        {
            if ((row.DeviceLocalTime - bucketStart).TotalMinutes >= intervalMinutes && currentPayloads.Count > 0)
            {
                result.Add((bucketStart, currentPayloads));
                currentPayloads = new List<JsonElement>();
                bucketStart = row.DeviceLocalTime;
            }

            var el = PayloadDeserializer.GetElement(row.Payload);
            if (el.HasValue)
                currentPayloads.Add(el.Value);
        }

        if (currentPayloads.Count > 0)
            result.Add((bucketStart, currentPayloads));

        return result;
    }

    // ── Chart builders ───────────────────────────────────────────────────────

    private static List<ChartDefinitionDto> BuildCharts(
        List<(DateTimeOffset Time, List<JsonElement> Payloads)> buckets, string timeFormat)
    {
        return new List<ChartDefinitionDto>
        {
            BuildPowerChart(buckets, timeFormat),
            BuildDcChart(buckets, timeFormat),
            BuildPhaseCurrentsChart(buckets, timeFormat),
            BuildFrequencyChart(buckets, timeFormat),
            BuildTemperaturesChart(buckets, timeFormat),
            BuildEfficiencyChart(buckets, timeFormat),
        };
    }

    private static ChartDefinitionDto BuildPowerChart(
        List<(DateTimeOffset Time, List<JsonElement> Payloads)> buckets, string timeFormat)
    {
        return new ChartDefinitionDto
        {
            Key = "powerProfile",
            Title = "Active & Reactive Power",
            Type = "area",
            XAxis = new CurveXAxisDto { Key = "time", Label = "Time", Format = timeFormat },
            Series = new List<CurveSeriesDto>
            {
                new() { Key = "activePowerKw", Label = "Active Power", Unit = "kW", Color = "#8b5cf6", YAxis = "left", Type = "area" },
                new() { Key = "reactivePowerKvar", Label = "Reactive Power", Unit = "kVar", Color = "#f97316", YAxis = "right", Type = "line" },
            },
            Data = buckets.Select(b => new Dictionary<string, object?>
            {
                ["time"] = b.Time.ToString(timeFormat),
                ["activePowerKw"] = Extract(b.Payloads, "pcsActivePower"),
                ["reactivePowerKvar"] = Extract(b.Payloads, "pcsReactivePower"),
            }).ToList()
        };
    }

    private static ChartDefinitionDto BuildDcChart(
        List<(DateTimeOffset Time, List<JsonElement> Payloads)> buckets, string timeFormat)
    {
        return new ChartDefinitionDto
        {
            Key = "dcProfile",
            Title = "DC Voltage & Current",
            Type = "line",
            XAxis = new CurveXAxisDto { Key = "time", Label = "Time", Format = timeFormat },
            Series = new List<CurveSeriesDto>
            {
                new() { Key = "dcBusV", Label = "DC Bus Voltage", Unit = "V", Color = "#22c55e", YAxis = "left", Type = "line" },
                new() { Key = "dcCurrentA", Label = "DC Current", Unit = "A", Color = "#f97316", YAxis = "right", Type = "line" },
            },
            Data = buckets.Select(b => new Dictionary<string, object?>
            {
                ["time"] = b.Time.ToString(timeFormat),
                ["dcBusV"] = Extract(b.Payloads, "pcsTotalDcBusVoltage"),
                ["dcCurrentA"] = Extract(b.Payloads, "pcsTotalDcCurrent"),
            }).ToList()
        };
    }

    private static ChartDefinitionDto BuildPhaseCurrentsChart(
        List<(DateTimeOffset Time, List<JsonElement> Payloads)> buckets, string timeFormat)
    {
        return new ChartDefinitionDto
        {
            Key = "phaseCurrents",
            Title = "AC Phase Currents",
            Type = "line",
            XAxis = new CurveXAxisDto { Key = "time", Label = "Time", Format = timeFormat },
            Series = new List<CurveSeriesDto>
            {
                new() { Key = "phaseA", Label = "Phase A", Unit = "A", Color = "#ef4444", Type = "line" },
                new() { Key = "phaseB", Label = "Phase B", Unit = "A", Color = "#22c55e", Type = "line" },
                new() { Key = "phaseC", Label = "Phase C", Unit = "A", Color = "#3b82f6", Type = "line" },
            },
            Data = buckets.Select(b => new Dictionary<string, object?>
            {
                ["time"] = b.Time.ToString(timeFormat),
                ["phaseA"] = Extract(b.Payloads, "pcsPhaseCurrentA"),
                ["phaseB"] = Extract(b.Payloads, "pcsPhaseCurrentB"),
                ["phaseC"] = Extract(b.Payloads, "pcsPhaseCurrentC"),
            }).ToList()
        };
    }

    private static ChartDefinitionDto BuildFrequencyChart(
        List<(DateTimeOffset Time, List<JsonElement> Payloads)> buckets, string timeFormat)
    {
        return new ChartDefinitionDto
        {
            Key = "gridFrequency",
            Title = "Grid Frequency",
            Type = "line",
            XAxis = new CurveXAxisDto { Key = "time", Label = "Time", Format = timeFormat },
            Series = new List<CurveSeriesDto>
            {
                new() { Key = "freqHz", Label = "Frequency", Unit = "Hz", Color = "#8b5cf6", Type = "line" },
            },
            ReferenceLines = new List<ChartReferenceLineDto>
            {
                new() { Y = 50, Label = "Nominal", Color = "#22c55e", StrokeDasharray = "5 5" },
                new() { Y = 50.5, Label = "High Limit", Color = "#f97316", StrokeDasharray = "3 3" },
                new() { Y = 49.5, Label = "Low Limit", Color = "#f97316", StrokeDasharray = "3 3" },
            },
            Data = buckets.Select(b => new Dictionary<string, object?>
            {
                ["time"] = b.Time.ToString(timeFormat),
                ["freqHz"] = Extract(b.Payloads, "pcsGridFreq"),
            }).ToList()
        };
    }

    private static ChartDefinitionDto BuildTemperaturesChart(
        List<(DateTimeOffset Time, List<JsonElement> Payloads)> buckets, string timeFormat)
    {
        return new ChartDefinitionDto
        {
            Key = "temperatures",
            Title = "Temperature Trends",
            Type = "area",
            XAxis = new CurveXAxisDto { Key = "time", Label = "Time", Format = timeFormat },
            Series = new List<CurveSeriesDto>
            {
                new() { Key = "igbt", Label = "IGBT", Unit = "\u00b0C", Color = "#ef4444", Type = "area" },
                new() { Key = "inductance", Label = "Inductance", Unit = "\u00b0C", Color = "#f97316", Type = "area" },
                new() { Key = "ambient", Label = "Ambient", Unit = "\u00b0C", Color = "#22c55e", Type = "area" },
            },
            Data = buckets.Select(b => new Dictionary<string, object?>
            {
                ["time"] = b.Time.ToString(timeFormat),
                ["igbt"] = Extract(b.Payloads, "pcsIgbtTemp"),
                ["inductance"] = Extract(b.Payloads, "pcsInductanceTemp"),
                ["ambient"] = Extract(b.Payloads, "pcsAmbientTemp"),
            }).ToList()
        };
    }

    private static ChartDefinitionDto BuildEfficiencyChart(
        List<(DateTimeOffset Time, List<JsonElement> Payloads)> buckets, string timeFormat)
    {
        return new ChartDefinitionDto
        {
            Key = "efficiencyTrend",
            Title = "Efficiency",
            Type = "line",
            Optional = true,
            XAxis = new CurveXAxisDto { Key = "time", Label = "Time", Format = timeFormat },
            Series = new List<CurveSeriesDto>
            {
                new() { Key = "efficiency", Label = "Efficiency", Unit = "%", Color = "#22c55e", Type = "line" },
            },
            ReferenceLines = new List<ChartReferenceLineDto>
            {
                new() { Y = 94, Label = "Min Threshold", Color = "#f97316", StrokeDasharray = "3 3" },
            },
            Data = buckets.Select(b => new Dictionary<string, object?>
            {
                ["time"] = b.Time.ToString(timeFormat),
                ["efficiency"] = Extract(b.Payloads, "pcsEfficiency"),
            }).ToList()
        };
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    /// <summary>
    /// Searches all payloads in a time bucket for the first non-null numeric value at the given path.
    /// </summary>
    private static object? Extract(List<JsonElement> payloads, string path)
    {
        foreach (var payload in payloads)
        {
            var value = PayloadDeserializer.ExtractValue(payload, path);
            switch (value)
            {
                case double d: return Math.Round(d, 1);
                case int i: return i;
                case long l: return l;
            }
        }
        return null;
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
