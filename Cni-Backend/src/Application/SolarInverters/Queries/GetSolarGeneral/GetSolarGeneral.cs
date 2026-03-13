using System.Text.Json;
using Backend.Application.Common.Interfaces;
using Backend.Application.Common.Models.Monitoring;
using Backend.Application.Common.Security;
using Backend.Application.Common.Services;
using Backend.Application.Common.Services.ParameterMapper;
using Backend.Domain.Constants;
using Microsoft.Extensions.Caching.Memory;

namespace Backend.Application.SolarInverters.Queries.GetSolarGeneral;

[Authorize(Roles = "Administrator,Tenant.Admin")]
[RequirePermission(Backend.Domain.Constants.Permissions.SolarView)]
public record GetSolarGeneralQuery : IRequest<SummaryResponseDto>
{
    public Guid UnitId { get; init; }
}

public class GetSolarGeneralQueryHandler : IRequestHandler<GetSolarGeneralQuery, SummaryResponseDto>
{
    private readonly IApplicationDbContext _context;
    private readonly IMemoryCache _cache;

    public GetSolarGeneralQueryHandler(IApplicationDbContext context, IMemoryCache cache)
    {
        _context = context;
        _cache = cache;
    }

    public async Task<SummaryResponseDto> Handle(GetSolarGeneralQuery request, CancellationToken cancellationToken)
    {
        var device = await _context.Devices
            .Include(d => d.DeviceType)
            .Include(d => d.LogicalDevice)
            .Include(d => d.SpecificationTemplate)
            .Include(d => d.Rtu!).ThenInclude(r => r.Facility)
            .Where(d => d.Id == request.UnitId &&
                ((d.DeviceType != null && (d.DeviceType.Name == DeviceTypeNames.Solar || d.DeviceType.Name == DeviceTypeNames.Inverter)) ||
                 (d.LogicalDevice != null && d.LogicalDevice.Category == "Solar")))
            .FirstOrDefaultAsync(cancellationToken);

        Guard.Against.NotFound(request.UnitId, device);

        // Load latest telemetry per Kind from Solar table
        var solarEntries = await _context.DeviceSolarData
            .Where(dd => dd.DeviceId == request.UnitId)
            .GroupBy(dd => dd.Kind)
            .Select(g => g.OrderByDescending(dd => dd.DeviceLocalTime).First())
            .ToListAsync(cancellationToken);

        var payloadsByKind = new Dictionary<string, JsonElement?>();
        foreach (var entry in solarEntries)
        {
            if (entry.Kind != null)
                payloadsByKind[entry.Kind] = PayloadDeserializer.GetElement(entry.Payload);
        }

        var metadata = PayloadDeserializer.GetElement(device.Metadata);

        var parameterMap = ParameterMapParser.ParseCached(device.SpecificationTemplate?.ParameterMapTemplate, _cache);
        var resolver = parameterMap.Count > 0 && payloadsByKind.Count > 0
            ? new ParameterResolver(parameterMap, payloadsByKind, metadata)
            : null;

        var powerPayload = payloadsByKind.GetValueOrDefault("Solar.Power");
        var mpptPayload = payloadsByKind.GetValueOrDefault("Solar.Mppt");

        var sections = new List<DynamicSectionDto>();

        // Overview section
        sections.Add(BuildOverviewSection(device, resolver, powerPayload));

        // AC Output section
        sections.Add(BuildAcOutputSection(resolver, powerPayload));

        // MPPT Info section
        sections.Add(BuildMpptSection(resolver, mpptPayload, powerPayload));

        // Device Info section
        sections.Add(BuildDeviceInfoSection(device, metadata));

        return new SummaryResponseDto { Sections = sections };
    }

    private static DynamicSectionDto BuildOverviewSection(
        Domain.Entities.Device device, ParameterResolver? resolver, JsonElement? powerPayload)
    {
        return new DynamicSectionDto
        {
            Key = "overview",
            Title = "Overview",
            Icon = "sun",
            Columns = 3,
            Fields = new List<SectionFieldDto>
            {
                NumField("activePower", "Active Power", "kW",
                    resolver?.ResolveNumber("activePower") ?? ExtractDouble(powerPayload, "activePower")),
                NumField("reactivePower", "Reactive Power", "kVar",
                    resolver?.ResolveNumber("reactivePower") ?? ExtractDouble(powerPayload, "reactivePower")),
                NumField("dailyYield", "Daily Yield", "kWh",
                    resolver?.ResolveNumber("dailyYield") ?? ExtractDouble(powerPayload, "dailyYield")),
                NumField("totalYield", "Total Yield", "kWh",
                    resolver?.ResolveNumber("totalYield") ?? ExtractDouble(powerPayload, "totalYield")),
                NumField("efficiency", "Efficiency", "%",
                    resolver?.ResolveNumber("efficiency") ?? ExtractDouble(powerPayload, "efficiency")),
                NumField("powerFactor", "Power Factor", null,
                    resolver?.ResolveNumber("powerFactor") ?? ExtractDouble(powerPayload, "powerFactor")),
            }
        };
    }

    private static DynamicSectionDto BuildAcOutputSection(
        ParameterResolver? resolver, JsonElement? powerPayload)
    {
        return new DynamicSectionDto
        {
            Key = "acOutput",
            Title = "AC Output",
            Icon = "zap",
            Columns = 3,
            Fields = new List<SectionFieldDto>
            {
                NumField("voltageAB", "Voltage AB", "V",
                    resolver?.ResolveNumber("voltageAB") ?? ExtractDouble(powerPayload, "voltageAB")),
                NumField("voltageBC", "Voltage BC", "V",
                    resolver?.ResolveNumber("voltageBC") ?? ExtractDouble(powerPayload, "voltageBC")),
                NumField("voltageCA", "Voltage CA", "V",
                    resolver?.ResolveNumber("voltageCA") ?? ExtractDouble(powerPayload, "voltageCA")),
                NumField("currentA", "Current Phase A", "A",
                    resolver?.ResolveNumber("currentA") ?? ExtractDouble(powerPayload, "currentA")),
                NumField("currentB", "Current Phase B", "A",
                    resolver?.ResolveNumber("currentB") ?? ExtractDouble(powerPayload, "currentB")),
                NumField("currentC", "Current Phase C", "A",
                    resolver?.ResolveNumber("currentC") ?? ExtractDouble(powerPayload, "currentC")),
                NumField("frequency", "Grid Frequency", "Hz",
                    resolver?.ResolveNumber("frequency") ?? ExtractDouble(powerPayload, "frequency")),
            }
        };
    }

    private static DynamicSectionDto BuildMpptSection(
        ParameterResolver? resolver, JsonElement? mpptPayload, JsonElement? powerPayload)
    {
        var columns = new List<TableColumnDto>
        {
            new() { Key = "mppt", Label = "MPPT", Type = "string" },
            new() { Key = "voltage", Label = "Voltage (V)", Type = "number" },
            new() { Key = "current", Label = "Current (A)", Type = "number" },
            new() { Key = "power", Label = "Power (kW)", Type = "number" }
        };

        var rows = new List<Dictionary<string, object?>>();

        // Try to extract MPPT array from payload
        var source = mpptPayload ?? powerPayload;
        if (source.HasValue)
        {
            // Look for mpptTrackers array or mppt1Voltage/mppt2Voltage pattern
            if (source.Value.TryGetProperty("mpptTrackers", out var trackers) &&
                trackers.ValueKind == JsonValueKind.Array)
            {
                var idx = 0;
                foreach (var tracker in trackers.EnumerateArray())
                {
                    idx++;
                    rows.Add(new Dictionary<string, object?>
                    {
                        ["mppt"] = $"MPPT {idx}",
                        ["voltage"] = ExtractDouble(tracker, "voltage"),
                        ["current"] = ExtractDouble(tracker, "current"),
                        ["power"] = ExtractDouble(tracker, "power")
                    });
                }
            }
            else
            {
                // Try numbered pattern: mppt1Voltage, mppt1Current, etc.
                for (var i = 1; i <= 12; i++)
                {
                    var v = ExtractDouble(source, $"mppt{i}Voltage");
                    var c = ExtractDouble(source, $"mppt{i}Current");
                    if (v == null && c == null) break;

                    rows.Add(new Dictionary<string, object?>
                    {
                        ["mppt"] = $"MPPT {i}",
                        ["voltage"] = v,
                        ["current"] = c,
                        ["power"] = v.HasValue && c.HasValue ? Math.Round(v.Value * c.Value / 1000.0, 2) : null
                    });
                }
            }
        }

        return new DynamicSectionDto
        {
            Key = "mpptInfo",
            Title = "MPPT Information",
            Icon = "grid",
            Variant = "table",
            Optional = rows.Count == 0,
            TableData = new TableDataDto { Columns = columns, Rows = rows }
        };
    }

    private static DynamicSectionDto BuildDeviceInfoSection(
        Domain.Entities.Device device, JsonElement? metadata)
    {
        return new DynamicSectionDto
        {
            Key = "deviceInfo",
            Title = "Device Information",
            Icon = "info",
            Columns = 2,
            Fields = new List<SectionFieldDto>
            {
                StringField("name", "Device Name", device.Name),
                StringField("serial", "Serial Number", device.SerialNumber),
                StringField("model", "Model", device.Model),
                StringField("manufacturer", "Manufacturer", device.Manufacturer),
                StringField("firmware", "Firmware Version",
                    ExtractString(metadata, "firmwareVersion") ?? ExtractString(metadata, "softwareVersion")),
                StringField("protocol", "Protocol",
                    ExtractString(metadata, "protocol") ?? ExtractString(metadata, "communicationProtocol")),
                StringField("installedCapacity", "Installed Capacity",
                    ExtractString(metadata, "ratedPower") ?? ExtractString(metadata, "installedCapacityKw")),
            }
        };
    }

    private static SectionFieldDto NumField(string key, string label, string? unit, double? value) =>
        new() { Key = key, Label = label, Type = "number", Unit = unit, Value = value };

    private static SectionFieldDto StringField(string key, string label, string? value) =>
        new() { Key = key, Label = label, Type = "string", Value = value };

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

    private static double? ExtractDouble(JsonElement element, string path)
    {
        var value = PayloadDeserializer.ExtractValue(element, path);
        return value switch
        {
            double d => d,
            int i => i,
            long l => l,
            _ => null
        };
    }

    private static string? ExtractString(JsonElement? element, string path)
    {
        if (!element.HasValue) return null;
        var value = PayloadDeserializer.ExtractValue(element.Value, path);
        return value?.ToString();
    }
}
