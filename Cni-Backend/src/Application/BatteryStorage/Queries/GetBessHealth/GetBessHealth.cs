using System.Text.Json;
using Backend.Application.Common.Interfaces;
using Backend.Application.Common.Models.Monitoring;
using Backend.Application.Common.Security;
using Backend.Application.Common.Services;

namespace Backend.Application.BatteryStorage.Queries.GetBessHealth;

[Authorize(Roles = "Administrator,Tenant.Admin")]
[RequirePermission(Backend.Domain.Constants.Permissions.BessView)]
public record GetBessHealthQuery : IRequest<List<DynamicSectionDto>>
{
    public Guid UnitId { get; init; }
}

public class GetBessHealthQueryHandler : IRequestHandler<GetBessHealthQuery, List<DynamicSectionDto>>
{
    private readonly IApplicationDbContext _context;

    public GetBessHealthQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<DynamicSectionDto>> Handle(GetBessHealthQuery request, CancellationToken cancellationToken)
    {
        var device = await _context.Devices
            .Include(d => d.DeviceType)
            .Where(d => d.Id == request.UnitId && d.DeviceType != null && d.DeviceType.Name == "Battery")
            .FirstOrDefaultAsync(cancellationToken);

        Guard.Against.NotFound(request.UnitId, device);

        var latestData = await _context.DeviceBatteryData
            .Where(dd => dd.DeviceId == request.UnitId)
            .OrderByDescending(dd => dd.DeviceLocalTime)
            .FirstOrDefaultAsync(cancellationToken);

        var payload = PayloadDeserializer.GetElement(latestData?.Payload);
        var specs = PayloadDeserializer.GetElement(device.Specifications);

        return BuildHealthSections(specs, payload);
    }

    private static List<DynamicSectionDto> BuildHealthSections(JsonElement? specs, JsonElement? payload)
    {
        if (specs.HasValue && specs.Value.TryGetProperty("tabs", out var tabs) &&
            tabs.TryGetProperty("health", out var healthTab) &&
            healthTab.TryGetProperty("sections", out var sectionsElement) &&
            sectionsElement.ValueKind == JsonValueKind.Array)
        {
            return BuildFromSchema(sectionsElement, payload);
        }

        return BuildDefaultSections(payload);
    }

    private static List<DynamicSectionDto> BuildFromSchema(JsonElement sectionsElement, JsonElement? payload)
    {
        var sections = new List<DynamicSectionDto>();

        foreach (var section in sectionsElement.EnumerateArray())
        {
            var key = section.TryGetProperty("key", out var k) ? k.GetString() ?? "" : "";
            var title = section.TryGetProperty("title", out var t) ? t.GetString() ?? key : key;
            var icon = section.TryGetProperty("icon", out var ic) ? ic.GetString() : null;
            var variant = section.TryGetProperty("variant", out var v) ? v.GetString() ?? "card" : "card";
            var columns = section.TryGetProperty("columns", out var c) && c.TryGetInt32(out var cols) ? cols : 3;

            var fields = new List<SectionFieldDto>();
            if (section.TryGetProperty("fields", out var fieldsElement) &&
                fieldsElement.ValueKind == JsonValueKind.Array)
            {
                foreach (var field in fieldsElement.EnumerateArray())
                {
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
                        Key = fKey,
                        Label = fLabel,
                        Value = value,
                        Unit = fUnit,
                        Format = fFormat,
                        SourcePath = sourcePath
                    });
                }
            }

            sections.Add(new DynamicSectionDto
            {
                Key = key,
                Title = title,
                Icon = icon,
                Variant = variant,
                Columns = columns,
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
                Key = "sohOverview",
                Title = "State of Health Overview",
                Icon = "heart-pulse",
                Fields = new List<SectionFieldDto>
                {
                    Field("soh", "State of Health", payload, "stateOfHealth", "%"),
                    Field("cycleCount", "Cycle Count", payload, "health.cycleCount", null),
                    Field("designCapacity", "Design Capacity", payload, "health.designCapacity", "kWh"),
                    Field("currentCapacity", "Current Capacity", payload, "health.currentCapacity", "kWh"),
                }
            },
            new()
            {
                Key = "degradation",
                Title = "Degradation",
                Icon = "trending-down",
                Fields = new List<SectionFieldDto>
                {
                    Field("capacityFade", "Capacity Fade", payload, "health.capacityFade", "%"),
                    Field("resistanceIncrease", "Resistance Increase", payload, "health.resistanceIncrease", "%"),
                }
            },
            new()
            {
                Key = "throughput",
                Title = "Energy Throughput",
                Icon = "zap",
                Fields = new List<SectionFieldDto>
                {
                    Field("totalCharged", "Total Energy Charged", payload, "health.totalCharged", "kWh"),
                    Field("totalDischarged", "Total Energy Discharged", payload, "health.totalDischarged", "kWh"),
                }
            }
        };
    }

    private static SectionFieldDto Field(string key, string label, JsonElement? payload, string sourcePath, string? unit)
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
            SourcePath = sourcePath
        };
    }
}
