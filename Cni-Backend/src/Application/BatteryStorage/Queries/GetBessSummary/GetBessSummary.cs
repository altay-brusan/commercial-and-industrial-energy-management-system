using System.Text.Json;
using Backend.Application.Common.Interfaces;
using Backend.Application.Common.Models.Monitoring;
using Backend.Application.Common.Security;
using Backend.Application.Common.Services;

namespace Backend.Application.BatteryStorage.Queries.GetBessSummary;

[Authorize(Roles = "Administrator,Tenant.Admin")]
[RequirePermission(Backend.Domain.Constants.Permissions.BessView)]
public record GetBessSummaryQuery : IRequest<List<DynamicSectionDto>>
{
    public Guid UnitId { get; init; }
}

public class GetBessSummaryQueryHandler : IRequestHandler<GetBessSummaryQuery, List<DynamicSectionDto>>
{
    private readonly IApplicationDbContext _context;

    public GetBessSummaryQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<DynamicSectionDto>> Handle(GetBessSummaryQuery request, CancellationToken cancellationToken)
    {
        var device = await _context.Devices
            .Include(d => d.DeviceType)
            .Include(d => d.DeviceModel)
            .Where(d => d.Id == request.UnitId && d.DeviceType != null && d.DeviceType.Name == "Battery")
            .FirstOrDefaultAsync(cancellationToken);

        Guard.Against.NotFound(request.UnitId, device);

        var latestData = await _context.DeviceBessData
            .Where(dd => dd.DeviceId == request.UnitId)
            .OrderByDescending(dd => dd.DeviceLocalTime)
            .FirstOrDefaultAsync(cancellationToken);

        var payload = PayloadDeserializer.GetElement(latestData?.Payload);
        var specs = PayloadDeserializer.GetElement(device.DeviceModel?.Specifications);

        return BuildSummarySections(specs, payload);
    }

    private static List<DynamicSectionDto> BuildSummarySections(JsonElement? specs, JsonElement? payload)
    {
        // Try to get sections from DeviceModel.Specifications
        if (specs.HasValue && specs.Value.TryGetProperty("tabs", out var tabs) &&
            tabs.TryGetProperty("summary", out var summaryTab) &&
            summaryTab.TryGetProperty("sections", out var sectionsElement) &&
            sectionsElement.ValueKind == JsonValueKind.Array)
        {
            return BuildFromSchema(sectionsElement, payload);
        }

        // Default sections when no schema is available
        return BuildDefaultSections(payload);
    }

    private static List<DynamicSectionDto> BuildFromSchema(JsonElement sectionsElement, JsonElement? payload)
    {
        var sections = new List<DynamicSectionDto>();
        var order = 0;

        foreach (var section in sectionsElement.EnumerateArray())
        {
            order++;
            var key = section.TryGetProperty("key", out var k) ? k.GetString() ?? "" : "";
            var title = section.TryGetProperty("title", out var t) ? t.GetString() ?? key : key;

            var fields = new List<SectionFieldDto>();
            if (section.TryGetProperty("fields", out var fieldsElement) &&
                fieldsElement.ValueKind == JsonValueKind.Array)
            {
                var fieldOrder = 0;
                foreach (var field in fieldsElement.EnumerateArray())
                {
                    fieldOrder++;
                    var fKey = field.TryGetProperty("key", out var fk) ? fk.GetString() ?? "" : "";
                    var fLabel = field.TryGetProperty("label", out var fl) ? fl.GetString() ?? fKey : fKey;
                    var fUnit = field.TryGetProperty("unit", out var fu) ? fu.GetString() : null;
                    var fFormat = field.TryGetProperty("format", out var ff) ? ff.GetString() : null;
                    var sourcePath = field.TryGetProperty("sourcePath", out var sp) ? sp.GetString() : null;

                    object? value = null;
                    if (payload.HasValue && !string.IsNullOrEmpty(sourcePath))
                    {
                        value = PayloadDeserializer.ExtractValue(payload.Value, sourcePath);
                    }

                    fields.Add(new SectionFieldDto
                    {
                        Key = fKey,
                        Label = fLabel,
                        Value = value,
                        Unit = fUnit,
                        Format = fFormat,
                        SourcePath = sourcePath,
                        Order = fieldOrder
                    });
                }
            }

            sections.Add(new DynamicSectionDto
            {
                Key = key,
                Title = title,
                Order = order,
                Fields = fields
            });
        }

        return sections;
    }

    private static List<DynamicSectionDto> BuildDefaultSections(JsonElement? payload)
    {
        return new List<DynamicSectionDto>
        {
            new()
            {
                Key = "liveStatus",
                Title = "Live Status",
                Order = 1,
                Fields = new List<SectionFieldDto>
                {
                    Field("soc", "State of Charge", payload, "stateOfCharge", "%", 1),
                    Field("soh", "State of Health", payload, "stateOfHealth", "%", 2),
                    Field("chargeStatus", "Charge Status", payload, "chargeStatus", null, 3),
                    Field("batteryTemp", "Battery Temperature", payload, "batteryTemperature", "°C", 4),
                }
            },
            new()
            {
                Key = "dcPower",
                Title = "DC Power",
                Order = 2,
                Fields = new List<SectionFieldDto>
                {
                    Field("voltage", "Voltage", payload, "dcPower.voltage", "V", 1),
                    Field("current", "Current", payload, "dcPower.current", "A", 2),
                    Field("power", "Power", payload, "dcPower.power", "kW", 3),
                }
            },
            new()
            {
                Key = "energyLimits",
                Title = "Energy & Limits",
                Order = 3,
                Fields = new List<SectionFieldDto>
                {
                    Field("totalEnergyStored", "Total Energy Stored", payload, "totalEnergyStored", "kWh", 1),
                    Field("chargeLimit", "Charge Limit", payload, "chargeLimit", "kW", 2),
                    Field("dischargeLimit", "Discharge Limit", payload, "dischargeLimit", "kW", 3),
                }
            }
        };
    }

    private static SectionFieldDto Field(string key, string label, JsonElement? payload, string sourcePath, string? unit, int order)
    {
        object? value = null;
        if (payload.HasValue)
            value = PayloadDeserializer.ExtractValue(payload.Value, sourcePath);

        return new SectionFieldDto
        {
            Key = key,
            Label = label,
            Value = value,
            Unit = unit,
            SourcePath = sourcePath,
            Order = order
        };
    }
}
