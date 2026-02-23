using System.Text.Json;
using Backend.Application.Common.Interfaces;
using Backend.Application.Common.Models.Monitoring;
using Backend.Application.Common.Security;
using Backend.Application.Common.Services;

namespace Backend.Application.Pcs.Queries.GetPcsSchema;

[Authorize(Roles = "Administrator,Tenant.Admin")]
[RequirePermission(Backend.Domain.Constants.Permissions.PcsView)]
public record GetPcsSchemaQuery : IRequest<SchemaResponseDto>;

public class GetPcsSchemaQueryHandler : IRequestHandler<GetPcsSchemaQuery, SchemaResponseDto>
{
    private readonly IApplicationDbContext _context;

    public GetPcsSchemaQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<SchemaResponseDto> Handle(GetPcsSchemaQuery request, CancellationToken cancellationToken)
    {
        var templates = await _context.DeviceSpecificationTemplates
            .Include(t => t.DeviceType)
            .Where(t => t.DeviceType != null && t.DeviceType.Name == "PCS")
            .ToListAsync(cancellationToken);

        var productTypes = templates.Select(t =>
        {
            var specs = PayloadDeserializer.GetElement(t.SpecificationsTemplate);

            return new ProductTypeSchemaDto
            {
                Key = BuildKey(t.Model),
                Label = BuildLabel(t.Manufacturer, t.Model),
                SupportedTabs = ExtractStringList(specs, "supportedTabs")
                    ?? new List<string> { "general", "curves", "faultFlags" },
                CurveCharts = ExtractStringList(specs, "curveCharts")
                    ?? new List<string> { "acPower", "dcPower", "frequency", "temperature", "efficiency" },
                Features = ExtractFeatures(specs),
            };
        }).ToList();

        return new SchemaResponseDto { ProductTypes = productTypes };
    }

    private static string BuildKey(string model) =>
        model.ToLowerInvariant().Replace(" ", "-").Replace(".", "").Replace("_", "-");

    private static string BuildLabel(string? manufacturer, string model) =>
        !string.IsNullOrWhiteSpace(manufacturer) ? $"{manufacturer} {model}".Trim() : model;

    private static List<string>? ExtractStringList(JsonElement? specs, string dotPath)
    {
        if (!specs.HasValue) return null;
        var current = specs.Value;
        foreach (var part in dotPath.Split('.'))
        {
            if (!current.TryGetProperty(part, out var next)) return null;
            current = next;
        }
        if (current.ValueKind != JsonValueKind.Array) return null;

        var result = new List<string>();
        foreach (var item in current.EnumerateArray())
        {
            if (item.ValueKind == JsonValueKind.String)
                result.Add(item.GetString()!);
            else if (item.TryGetProperty("key", out var k) && k.ValueKind == JsonValueKind.String)
                result.Add(k.GetString()!);
        }
        return result.Count > 0 ? result : null;
    }

    private static Dictionary<string, object?> ExtractFeatures(JsonElement? specs)
    {
        var features = new Dictionary<string, object?>();
        if (!specs.HasValue || !specs.Value.TryGetProperty("features", out var featEl) ||
            featEl.ValueKind != JsonValueKind.Object)
            return features;

        foreach (var prop in featEl.EnumerateObject())
        {
            features[prop.Name] = prop.Value.ValueKind switch
            {
                JsonValueKind.True => true,
                JsonValueKind.False => false,
                JsonValueKind.Number => prop.Value.TryGetInt32(out var i) ? i : prop.Value.GetDouble(),
                JsonValueKind.String => prop.Value.GetString(),
                _ => null
            };
        }
        return features;
    }
}
