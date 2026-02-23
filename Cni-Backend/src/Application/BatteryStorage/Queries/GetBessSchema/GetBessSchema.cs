using System.Text.Json;
using Backend.Application.Common.Interfaces;
using Backend.Application.Common.Models.Monitoring;
using Backend.Application.Common.Security;
using Backend.Application.Common.Services;
using Backend.Domain.Constants;

namespace Backend.Application.BatteryStorage.Queries.GetBessSchema;

[Authorize(Roles = "Administrator,Tenant.Admin")]
[RequirePermission(Backend.Domain.Constants.Permissions.BessView)]
public record GetBessSchemaQuery : IRequest<SchemaResponseDto>;

public class GetBessSchemaQueryHandler : IRequestHandler<GetBessSchemaQuery, SchemaResponseDto>
{
    private readonly IApplicationDbContext _context;

    public GetBessSchemaQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<SchemaResponseDto> Handle(GetBessSchemaQuery request, CancellationToken cancellationToken)
    {
        // Load all Battery-type specification templates
        var templates = await _context.DeviceSpecificationTemplates
            .Include(t => t.DeviceType)
            .Where(t => t.DeviceType != null && t.DeviceType.Name == DeviceTypeNames.EnergyStorage)
            .ToListAsync(cancellationToken);

        var productTypes = templates.Select(t =>
        {
            var specs = PayloadDeserializer.GetElement(t.SpecificationsTemplate);
            var hierarchy = PayloadDeserializer.GetElement(t.HierarchyTemplate);

            return new ProductTypeSchemaDto
            {
                Key = BuildKey(t.Manufacturer, t.Model),
                Label = BuildLabel(t.Manufacturer, t.Model),
                Hierarchy = ExtractHierarchyLevels(hierarchy),
                SupportedTabs = ExtractStringList(specs, "supportedTabs") ?? DefaultSupportedTabs(),
                SummarySections = ExtractSectionKeys(specs, "tabs.summary.sections") ?? DefaultSummarySections(specs),
                HealthSections = ExtractSectionKeys(specs, "tabs.health.sections") ?? DefaultHealthSections(),
                FaultSources = ExtractStringList(specs, "faultSources") ?? DefaultFaultSources(specs),
                CurveCharts = ExtractStringList(specs, "curveCharts") ?? DefaultCurveCharts(),
                Features = ExtractFeatures(specs),
                ConnectedDevices = ExtractConnectedDevices(hierarchy),
            };
        }).ToList();

        return new SchemaResponseDto { ProductTypes = productTypes };
    }

    private static string BuildKey(string? manufacturer, string model)
    {
        var parts = new List<string>();
        if (!string.IsNullOrWhiteSpace(model))
            parts.Add(model);
        var key = string.Join("-", parts).ToLowerInvariant()
            .Replace(" ", "-").Replace(".", "").Replace("_", "-");
        return string.IsNullOrWhiteSpace(key) ? "unknown" : key;
    }

    private static string BuildLabel(string? manufacturer, string model)
    {
        if (!string.IsNullOrWhiteSpace(manufacturer))
            return $"{manufacturer} {model}".Trim();
        return model;
    }

    private static List<string> ExtractHierarchyLevels(JsonElement? hierarchy)
    {
        if (!hierarchy.HasValue) return new List<string> { "rack", "pack", "module", "cell" };

        // Walk the hierarchy template tree to extract level keys
        var levels = new List<string>();
        CollectHierarchyKeys(hierarchy.Value, levels);
        if (levels.Count == 0)
            return new List<string> { "rack", "pack", "module", "cell" };
        return levels;
    }

    private static void CollectHierarchyKeys(JsonElement element, List<string> levels)
    {
        if (element.TryGetProperty("children", out var children) && children.ValueKind == JsonValueKind.Array)
        {
            foreach (var child in children.EnumerateArray())
            {
                var key = child.TryGetProperty("key", out var k) ? k.GetString() : null;
                if (key != null && key is not "esc" and not "hvac")
                {
                    levels.Add(key);
                    CollectHierarchyKeys(child, levels);
                    break; // follow the main branch (rack > pack > module)
                }
            }
        }
    }

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

    private static List<string>? ExtractSectionKeys(JsonElement? specs, string dotPath)
    {
        if (!specs.HasValue) return null;

        var current = specs.Value;
        foreach (var part in dotPath.Split('.'))
        {
            if (!current.TryGetProperty(part, out var next)) return null;
            current = next;
        }

        if (current.ValueKind != JsonValueKind.Array) return null;

        var keys = new List<string>();
        foreach (var section in current.EnumerateArray())
        {
            if (section.TryGetProperty("key", out var k) && k.ValueKind == JsonValueKind.String)
                keys.Add(k.GetString()!);
        }
        return keys.Count > 0 ? keys : null;
    }

    private static Dictionary<string, object?> ExtractFeatures(JsonElement? specs)
    {
        var features = new Dictionary<string, object?>();

        if (specs.HasValue && specs.Value.TryGetProperty("features", out var featEl) &&
            featEl.ValueKind == JsonValueKind.Object)
        {
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
        }

        if (features.Count == 0)
        {
            // Derive from specs structure
            var hasTms = specs.HasValue && HasNestedProperty(specs.Value, "tabs.summary.sections", "thermalManagement");
            var hasHvcu = specs.HasValue && HasNestedProperty(specs.Value, "tabs.summary.sections", "hvcuControl");
            features["hasTms"] = hasTms;
            features["hasHvcu"] = hasHvcu;
            features["hasCellBalancing"] = false;
            features["hasChillerFaults"] = false;
        }

        return features;
    }

    private static Dictionary<string, object?> ExtractConnectedDevices(JsonElement? hierarchy)
    {
        var result = new Dictionary<string, object?>();
        if (!hierarchy.HasValue) return result;

        int escCount = 0, hvacCount = 0;
        if (hierarchy.Value.TryGetProperty("children", out var children) && children.ValueKind == JsonValueKind.Array)
        {
            foreach (var child in children.EnumerateArray())
            {
                var key = child.TryGetProperty("key", out var k) ? k.GetString() : null;
                var count = child.TryGetProperty("count", out var c) && c.TryGetInt32(out var n) ? n : 1;
                if (key == "esc") escCount += count;
                else if (key == "hvac") hvacCount += count;
            }
        }

        result["escCount"] = escCount;
        result["hvacCount"] = hvacCount;
        return result;
    }

    private static bool HasNestedProperty(JsonElement root, string arrayPath, string sectionKey)
    {
        var current = root;
        foreach (var part in arrayPath.Split('.'))
        {
            if (!current.TryGetProperty(part, out var next)) return false;
            current = next;
        }

        if (current.ValueKind != JsonValueKind.Array) return false;
        foreach (var item in current.EnumerateArray())
        {
            if (item.TryGetProperty("key", out var k) && k.GetString() == sectionKey)
                return true;
        }
        return false;
    }

    private static List<string> DefaultSupportedTabs() =>
        new() { "summary", "health", "hierarchy", "monitoring", "fault", "alarms", "settings" };

    private static List<string> DefaultSummarySections(JsonElement? specs)
    {
        var sections = new List<string>
        {
            "nameplate", "liveStatus", "energyLimits", "cellExtremes", "contactorsSafety"
        };

        // Check if TMS/HVCU likely exist
        if (specs.HasValue)
        {
            if (HasNestedProperty(specs.Value, "tabs.summary.sections", "thermalManagement"))
                sections.Add("thermalManagement");
            if (HasNestedProperty(specs.Value, "tabs.summary.sections", "hvcuControl"))
                sections.Add("hvcuControl");
        }

        sections.Add("healthPreview");
        sections.Add("connectedDevices");
        return sections;
    }

    private static List<string> DefaultHealthSections() =>
        new() { "healthOverview", "sohTrend", "degradationMetrics", "energyThroughput",
                 "thermalStress", "operationalHistory", "maintenanceWarranty" };

    private static List<string> DefaultFaultSources(JsonElement? specs)
    {
        var sources = new List<string> { "battery" };
        if (specs.HasValue && HasNestedProperty(specs.Value, "tabs.summary.sections", "thermalManagement"))
            sources.Add("chiller");
        return sources;
    }

    private static List<string> DefaultCurveCharts() =>
        new() { "powerSoc", "voltageCurrent", "temperature" };
}
