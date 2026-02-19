using System.Text.Json;
using Backend.Application.Common.Interfaces;
using Backend.Application.Common.Models.Monitoring;
using Backend.Application.Common.Security;
using Backend.Application.Common.Services;

namespace Backend.Application.BatteryStorage.Queries.GetBessHierarchy;

[Authorize(Roles = "Administrator,Tenant.Admin")]
[RequirePermission(Backend.Domain.Constants.Permissions.BessView)]
public record GetBessHierarchyQuery : IRequest<HierarchyResponseDto>
{
    public Guid UnitId { get; init; }
}

public class GetBessHierarchyQueryHandler : IRequestHandler<GetBessHierarchyQuery, HierarchyResponseDto>
{
    private readonly IApplicationDbContext _context;

    public GetBessHierarchyQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<HierarchyResponseDto> Handle(GetBessHierarchyQuery request, CancellationToken cancellationToken)
    {
        var device = await _context.Devices
            .Include(d => d.DeviceType)
            .Include(d => d.DeviceModel)
            .Where(d => d.Id == request.UnitId && d.DeviceType != null && d.DeviceType.Name == "Battery")
            .FirstOrDefaultAsync(cancellationToken);

        Guard.Against.NotFound(request.UnitId, device);

        var levels = GetHierarchyLevels(device.DeviceModel?.Specifications);

        var latestData = await _context.DeviceBessData
            .Where(dd => dd.DeviceId == request.UnitId)
            .OrderByDescending(dd => dd.DeviceLocalTime)
            .FirstOrDefaultAsync(cancellationToken);

        var rootItems = ExtractRootItems(latestData?.Payload, levels);

        return new HierarchyResponseDto
        {
            Levels = levels,
            RootItems = rootItems
        };
    }

    private static List<HierarchyLevelDefinitionDto> GetHierarchyLevels(string? specifications)
    {
        var defaultLevels = new List<HierarchyLevelDefinitionDto>
        {
            new() { Key = "rack", Label = "Rack", Depth = 0, HasChildren = true },
            new() { Key = "pack", Label = "Pack", Depth = 1, HasChildren = true },
            new() { Key = "module", Label = "Module", Depth = 2, HasChildren = true },
            new() { Key = "cell", Label = "Cell", Depth = 3, HasChildren = false }
        };

        if (string.IsNullOrWhiteSpace(specifications))
            return defaultLevels;

        var element = PayloadDeserializer.GetElement(specifications);
        if (!element.HasValue)
            return defaultLevels;

        if (element.Value.TryGetProperty("hierarchy", out var hierarchyElement) &&
            hierarchyElement.ValueKind == JsonValueKind.Array)
        {
            var depth = 0;
            var parsed = new List<HierarchyLevelDefinitionDto>();
            foreach (var level in hierarchyElement.EnumerateArray())
            {
                var key = level.TryGetProperty("key", out var k) ? k.GetString() ?? "" : "";
                var label = level.TryGetProperty("label", out var l) ? l.GetString() ?? key : key;
                var hasChildren = level.TryGetProperty("hasChildren", out var hc) && hc.GetBoolean();
                parsed.Add(new HierarchyLevelDefinitionDto { Key = key, Label = label, Depth = depth, HasChildren = hasChildren });
                depth++;
            }
            return parsed.Count > 0 ? parsed : defaultLevels;
        }

        return defaultLevels;
    }

    private static List<HierarchyItemDto> ExtractRootItems(string? payload, List<HierarchyLevelDefinitionDto> levels)
    {
        var items = new List<HierarchyItemDto>();
        if (string.IsNullOrWhiteSpace(payload) || levels.Count == 0)
            return items;

        var element = PayloadDeserializer.GetElement(payload);
        if (!element.HasValue)
            return items;

        var rootLevel = levels[0];
        var pluralKey = rootLevel.Key + "s"; // rack -> racks

        if (PayloadDeserializer.GetNestedElement(element.Value, pluralKey) is { } arrayElement &&
            arrayElement.ValueKind == JsonValueKind.Array)
        {
            var index = 0;
            foreach (var item in arrayElement.EnumerateArray())
            {
                var id = item.TryGetProperty("id", out var idProp)
                    ? idProp.GetRawText().Trim('"')
                    : $"{rootLevel.Key}_{index}";

                var name = item.TryGetProperty("name", out var nameProp)
                    ? nameProp.GetString() ?? $"{rootLevel.Label} {index + 1}"
                    : $"{rootLevel.Label} {index + 1}";

                var status = item.TryGetProperty("status", out var statusProp)
                    ? statusProp.GetString()
                    : null;

                var data = new Dictionary<string, object?>();
                foreach (var prop in item.EnumerateObject())
                {
                    if (prop.Name is not ("id" or "name" or "status") &&
                        prop.Value.ValueKind != JsonValueKind.Array &&
                        prop.Value.ValueKind != JsonValueKind.Object)
                    {
                        data[prop.Name] = PayloadDeserializer.ExtractValue(item, prop.Name);
                    }
                }

                var childKey = levels.Count > 1 ? levels[1].Key + "s" : null;
                var childCount = 0;
                if (childKey != null && item.TryGetProperty(childKey, out var children) &&
                    children.ValueKind == JsonValueKind.Array)
                {
                    childCount = children.GetArrayLength();
                }

                items.Add(new HierarchyItemDto
                {
                    Id = id,
                    Name = name,
                    Level = rootLevel.Key,
                    Status = StatusBadgeDto.FromStatus(status),
                    Data = data.Count > 0 ? data : null,
                    HasChildren = rootLevel.HasChildren && childCount > 0,
                    ChildCount = childCount
                });

                index++;
            }
        }

        return items;
    }
}
