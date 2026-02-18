using System.Text.Json;
using Backend.Application.Common.Interfaces;
using Backend.Application.Common.Models.Monitoring;
using Backend.Application.Common.Security;
using Backend.Application.Common.Services;

namespace Backend.Application.Pcs.Queries.GetPcsGeneral;

[Authorize(Roles = "Administrator,Tenant.Admin")]
[RequirePermission(Backend.Domain.Constants.Permissions.PcsView)]
public record GetPcsGeneralQuery : IRequest<List<DynamicSectionDto>>
{
    public Guid UnitId { get; init; }
}

public class GetPcsGeneralQueryHandler : IRequestHandler<GetPcsGeneralQuery, List<DynamicSectionDto>>
{
    private readonly IApplicationDbContext _context;

    public GetPcsGeneralQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<DynamicSectionDto>> Handle(GetPcsGeneralQuery request, CancellationToken cancellationToken)
    {
        var device = await _context.Devices
            .Include(d => d.DeviceType)
            .Include(d => d.DeviceModel)
            .Where(d => d.Id == request.UnitId && d.DeviceType != null && d.DeviceType.Name == "PCS")
            .FirstOrDefaultAsync(cancellationToken);

        Guard.Against.NotFound(request.UnitId, device);

        var latestData = await _context.DevicePcsData
            .Where(dd => dd.DeviceId == request.UnitId)
            .OrderByDescending(dd => dd.DeviceLocalTime)
            .FirstOrDefaultAsync(cancellationToken);

        var payload = PayloadDeserializer.GetElement(latestData?.Payload);
        var specs = PayloadDeserializer.GetElement(device.DeviceModel?.Specifications);

        return BuildGeneralSections(specs, payload);
    }

    private static List<DynamicSectionDto> BuildGeneralSections(JsonElement? specs, JsonElement? payload)
    {
        if (specs.HasValue && specs.Value.TryGetProperty("tabs", out var tabs) &&
            tabs.TryGetProperty("general", out var generalTab) &&
            generalTab.TryGetProperty("sections", out var sectionsElement) &&
            sectionsElement.ValueKind == JsonValueKind.Array)
        {
            return BuildFromSchema(sectionsElement, payload);
        }

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
                        value = PayloadDeserializer.ExtractValue(payload.Value, sourcePath);

                    fields.Add(new SectionFieldDto
                    {
                        Key = fKey, Label = fLabel, Value = value,
                        Unit = fUnit, Format = fFormat, SourcePath = sourcePath, Order = fieldOrder
                    });
                }
            }

            sections.Add(new DynamicSectionDto
            {
                Key = key, Title = title, Order = order, Fields = fields
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
                Key = "pcsStatus",
                Title = "PCS Status",
                Order = 1,
                Fields = new List<SectionFieldDto>
                {
                    Field("operatingMode", "Operating Mode", payload, "status.operatingMode", null, 1),
                    Field("efficiency", "Efficiency", payload, "efficiency", "%", 2),
                    Field("temperature", "Temperature", payload, "temperaturePcs", "°C", 3),
                }
            },
            new()
            {
                Key = "acMeasurements",
                Title = "AC Measurements",
                Order = 2,
                Fields = new List<SectionFieldDto>
                {
                    Field("totalActivePower", "Total Active Power", payload, "acPower.totalActivePower", "kW", 1),
                    Field("totalReactivePower", "Total Reactive Power", payload, "acPower.totalReactivePower", "kVAR", 2),
                    Field("powerFactor", "Power Factor", payload, "acPower.totalPowerFactor", null, 3),
                    Field("voltageRms", "Voltage RMS", payload, "acPower.voltageRms", "V", 4),
                    Field("currentRms", "Current RMS", payload, "acPower.currentRms", "A", 5),
                    Field("frequency", "Frequency", payload, "acPower.frequency", "Hz", 6),
                }
            },
            new()
            {
                Key = "dcMeasurements",
                Title = "DC Measurements",
                Order = 3,
                Fields = new List<SectionFieldDto>
                {
                    Field("dcVoltage", "DC Voltage", payload, "dcPower.voltage", "V", 1),
                    Field("dcCurrent", "DC Current", payload, "dcPower.current", "A", 2),
                    Field("dcPower", "DC Power", payload, "dcPower.power", "kW", 3),
                }
            },
            new()
            {
                Key = "powerDistribution",
                Title = "Power Distribution",
                Order = 4,
                Fields = new List<SectionFieldDto>
                {
                    Field("totalEnergyDelivered", "Total Energy Delivered", payload, "totalEnergyDelivered", "kWh", 1),
                    Field("totalEnergyAbsorbed", "Total Energy Absorbed", payload, "totalEnergyAbsorbed", "kWh", 2),
                    Field("dailyEnergy", "Daily Energy", payload, "acPower.totalEnergyDaily", "kWh", 3),
                }
            },
            new()
            {
                Key = "temperatures",
                Title = "Temperatures",
                Order = 5,
                Fields = new List<SectionFieldDto>
                {
                    Field("pcsTemp", "PCS Temperature", payload, "temperaturePcs", "°C", 1),
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
            Key = key, Label = label, Value = value,
            Unit = unit, SourcePath = sourcePath, Order = order
        };
    }
}
