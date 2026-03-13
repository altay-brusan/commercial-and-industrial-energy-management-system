using System.Text.Json;
using Backend.Application.Common.Interfaces;
using Backend.Application.Common.Models.Monitoring;
using Backend.Application.Common.Security;
using Backend.Application.Common.Services;
using Backend.Domain.Constants;

namespace Backend.Application.SolarInverters.Queries.GetSolarHistory;

[Authorize(Roles = "Administrator,Tenant.Admin")]
[RequirePermission(Backend.Domain.Constants.Permissions.SolarView)]
public record GetSolarHistoryQuery : IRequest<SolarHistoryResponseDto>
{
    public Guid UnitId { get; init; }
    public string Period { get; init; } = MonitoringPeriods.TwentyFourHours;
    public string Interval { get; init; } = MonitoringIntervals.FiveMin;
}

public class GetSolarHistoryQueryHandler : IRequestHandler<GetSolarHistoryQuery, SolarHistoryResponseDto>
{
    private readonly IApplicationDbContext _context;

    public GetSolarHistoryQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<SolarHistoryResponseDto> Handle(GetSolarHistoryQuery request, CancellationToken cancellationToken)
    {
        var device = await _context.Devices
            .Include(d => d.SpecificationTemplate)
            .FirstOrDefaultAsync(d => d.Id == request.UnitId, cancellationToken);

        Guard.Against.NotFound(request.UnitId, device);

        var (start, end) = GetPeriodRange(request.Period);
        var intervalMinutes = GetIntervalMinutes(request.Interval);

        // Load all Solar data kinds for the device
        var rawData = await _context.DeviceSolarData
            .Where(dd => dd.DeviceId == request.UnitId
                && dd.DeviceLocalTime >= start && dd.DeviceLocalTime <= end)
            .OrderBy(dd => dd.DeviceLocalTime)
            .ToListAsync(cancellationToken);

        // Sample at the requested interval
        var sampled = new List<(DateTimeOffset Time, JsonElement? Payload)>();
        var lastTimestamp = DateTimeOffset.MinValue;

        foreach (var row in rawData)
        {
            if ((row.DeviceLocalTime - lastTimestamp).TotalMinutes >= intervalMinutes)
            {
                sampled.Add((row.DeviceLocalTime, PayloadDeserializer.GetElement(row.Payload)));
                lastTimestamp = row.DeviceLocalTime;
            }
        }

        var signals = GetSignalConfigs();

        var data = sampled.Select(d =>
        {
            var row = new Dictionary<string, object?> { ["time"] = d.Time.ToString("yyyy-MM-ddTHH:mm:ss") };

            foreach (var signal in signals)
            {
                row[signal.Key] = d.Payload.HasValue
                    ? ExtractNumber(d.Payload.Value, signal.SourcePath ?? signal.Key)
                    : null;
            }

            return row;
        }).ToList();

        return new SolarHistoryResponseDto
        {
            Signals = signals,
            DataPointCount = data.Count,
            Interval = request.Interval,
            Period = request.Period,
            Data = data
        };
    }

    private static List<SolarSignalConfigDto> GetSignalConfigs()
    {
        return new List<SolarSignalConfigDto>
        {
            // Power group
            new() { Key = "activePower", Label = "Active Power", Unit = "kW", Group = "Power", SourcePath = "activePower" },
            new() { Key = "reactivePower", Label = "Reactive Power", Unit = "kVar", Group = "Power", SourcePath = "reactivePower" },
            new() { Key = "dcPower", Label = "DC Power", Unit = "kW", Group = "Power", SourcePath = "dcPower" },
            new() { Key = "dailyYield", Label = "Daily Yield", Unit = "kWh", Group = "Power", SourcePath = "dailyYield" },

            // Voltage group
            new() { Key = "voltageAB", Label = "Voltage AB", Unit = "V", Group = "Voltage", SourcePath = "voltageAB" },
            new() { Key = "voltageBC", Label = "Voltage BC", Unit = "V", Group = "Voltage", SourcePath = "voltageBC" },
            new() { Key = "voltageCA", Label = "Voltage CA", Unit = "V", Group = "Voltage", SourcePath = "voltageCA" },

            // Current group
            new() { Key = "currentA", Label = "Current A", Unit = "A", Group = "Current", SourcePath = "currentA" },
            new() { Key = "currentB", Label = "Current B", Unit = "A", Group = "Current", SourcePath = "currentB" },
            new() { Key = "currentC", Label = "Current C", Unit = "A", Group = "Current", SourcePath = "currentC" },

            // MPPT group
            new() { Key = "mppt1Voltage", Label = "MPPT1 Voltage", Unit = "V", Group = "MPPT", SourcePath = "mppt1Voltage" },
            new() { Key = "mppt1Current", Label = "MPPT1 Current", Unit = "A", Group = "MPPT", SourcePath = "mppt1Current" },
            new() { Key = "mppt2Voltage", Label = "MPPT2 Voltage", Unit = "V", Group = "MPPT", SourcePath = "mppt2Voltage" },
            new() { Key = "mppt2Current", Label = "MPPT2 Current", Unit = "A", Group = "MPPT", SourcePath = "mppt2Current" },

            // Other
            new() { Key = "frequency", Label = "Grid Frequency", Unit = "Hz", Group = "Other", SourcePath = "frequency" },
            new() { Key = "efficiency", Label = "Efficiency", Unit = "%", Group = "Other", SourcePath = "efficiency" },
            new() { Key = "powerFactor", Label = "Power Factor", Unit = null, Group = "Other", SourcePath = "powerFactor" },
            new() { Key = "temperature", Label = "Internal Temperature", Unit = "\u00b0C", Group = "Other", SourcePath = "temperature" },
        };
    }

    private static object? ExtractNumber(JsonElement payload, string path)
    {
        var value = PayloadDeserializer.ExtractValue(payload, path);
        return value switch
        {
            double d => Math.Round(d, 2),
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

    private static int GetIntervalMinutes(string interval) => interval switch
    {
        MonitoringIntervals.OneMin => 1,
        MonitoringIntervals.FiveMin => 5,
        MonitoringIntervals.FifteenMin => 15,
        MonitoringIntervals.ThirtyMin => 30,
        MonitoringIntervals.OneHour => 60,
        MonitoringIntervals.OneDay => 1440,
        _ => 5
    };
}

public record SolarHistoryResponseDto
{
    public List<SolarSignalConfigDto> Signals { get; init; } = new();
    public int DataPointCount { get; init; }
    public string Interval { get; init; } = string.Empty;
    public string Period { get; init; } = string.Empty;
    public List<Dictionary<string, object?>> Data { get; init; } = new();
}

public record SolarSignalConfigDto
{
    public string Key { get; init; } = string.Empty;
    public string Label { get; init; } = string.Empty;
    public string? Unit { get; init; }
    public string Group { get; init; } = string.Empty;
    public string? SourcePath { get; init; }
}
