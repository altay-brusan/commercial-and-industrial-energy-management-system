using System.Text.Json;
using Backend.Application.Common.Interfaces;
using Backend.Application.Common.Models.Monitoring;
using Backend.Application.Common.Security;
using Backend.Application.Common.Services;
using Backend.Domain.Constants;

namespace Backend.Application.BatteryStorage.Queries.GetBessHierarchyLevel;

[Authorize(Roles = "Administrator,Tenant.Admin")]
[RequirePermission(Backend.Domain.Constants.Permissions.BessView)]
public record GetBessHierarchyLevelQuery : IRequest<HierarchyLevelResponseDto>
{
    public Guid UnitId { get; init; }
    public string Level { get; init; } = string.Empty;
    public string? ParentId { get; init; }
}

public class GetBessHierarchyLevelQueryHandler : IRequestHandler<GetBessHierarchyLevelQuery, HierarchyLevelResponseDto>
{
    private readonly IApplicationDbContext _context;

    private static readonly string[] HierarchyLevels = { "rack", "pack", "module", "cell" };

    public GetBessHierarchyLevelQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<HierarchyLevelResponseDto> Handle(GetBessHierarchyLevelQuery request, CancellationToken cancellationToken)
    {
        var device = await _context.Devices
            .Include(d => d.DeviceType)
            .Where(d => d.Id == request.UnitId && d.DeviceType != null && d.DeviceType.Name == DeviceTypeNames.EnergyStorage)
            .FirstOrDefaultAsync(cancellationToken);

        Guard.Against.NotFound(request.UnitId, device);

        // Parse hierarchy level definitions to check for virtual levels
        var levels = GetHierarchyLevels(device.Specifications);
        var levelDef = levels.FirstOrDefault(l => l.Key == request.Level.ToLowerInvariant());

        // Check if parentId is a valid GUID (indicates entity-based hierarchy)
        if (Guid.TryParse(request.ParentId, out var parentDeviceId))
        {
            return await BuildFromDeviceEntities(parentDeviceId, request.Level, levelDef?.IsVirtual ?? false, cancellationToken);
        }

        // Fallback: extract from payload JSON (legacy behavior)
        return await BuildFromPayload(request.UnitId, request.Level, request.ParentId, cancellationToken);
    }

    private async Task<HierarchyLevelResponseDto> BuildFromDeviceEntities(
        Guid parentDeviceId, string level, bool isVirtualLevel, CancellationToken cancellationToken)
    {
        // Build breadcrumb by walking up the parent chain
        var breadcrumb = new List<BreadcrumbDto>();
        var current = await _context.Devices
            .Include(d => d.DeviceType)
            .FirstOrDefaultAsync(d => d.Id == parentDeviceId, cancellationToken);

        if (current != null)
        {
            var ancestors = new List<BreadcrumbDto>();
            var walker = current;
            while (walker != null && walker.ParentDeviceId.HasValue)
            {
                ancestors.Add(new BreadcrumbDto
                {
                    Id = walker.Id.ToString(),
                    Name = walker.Name,
                    Level = walker.DeviceType?.Name?.Replace(DeviceTypeNames.EnergyStoragePrefix, "").ToLowerInvariant() ?? "unknown"
                });

                walker = await _context.Devices
                    .Include(d => d.DeviceType)
                    .FirstOrDefaultAsync(d => d.Id == walker.ParentDeviceId, cancellationToken);
            }
            ancestors.Reverse();
            breadcrumb = ancestors;
        }

        // Load children of the parent
        var children = await _context.Devices
            .Include(d => d.DeviceType)
            .Where(d => d.ParentDeviceId == parentDeviceId)
            .OrderBy(d => d.Name)
            .ToListAsync(cancellationToken);

        var childIds = children.Select(d => d.Id).ToList();

        // Check which children have their own children
        var hasGrandchildren = await _context.Devices
            .Where(d => d.ParentDeviceId.HasValue && childIds.Contains(d.ParentDeviceId.Value))
            .Select(d => d.ParentDeviceId!.Value)
            .Distinct()
            .ToListAsync(cancellationToken);

        var grandchildSet = new HashSet<Guid>(hasGrandchildren);

        var childCounts = await _context.Devices
            .Where(d => d.ParentDeviceId.HasValue && childIds.Contains(d.ParentDeviceId.Value))
            .GroupBy(d => d.ParentDeviceId!.Value)
            .Select(g => new { ParentId = g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.ParentId, x => x.Count, cancellationToken);

        var items = children.Select(child => new HierarchyItemDto
        {
            Id = child.Id.ToString(),
            Name = child.Name,
            Level = child.DeviceType?.Name?.Replace(DeviceTypeNames.EnergyStoragePrefix, "").ToLowerInvariant() ?? level,
            Status = StatusBadgeDto.FromStatus(child.Status),
            Data = null,
            HasChildren = grandchildSet.Contains(child.Id),
            ChildCount = childCounts.GetValueOrDefault(child.Id, 0),
            IsVirtual = isVirtualLevel
        }).ToList();

        return new HierarchyLevelResponseDto
        {
            Level = level,
            ParentId = parentDeviceId.ToString(),
            Breadcrumb = breadcrumb,
            Items = items
        };
    }

    private async Task<HierarchyLevelResponseDto> BuildFromPayload(
        Guid unitId, string level, string? parentId, CancellationToken cancellationToken)
    {
        var latestData = await _context.DeviceBatteryData
            .Where(dd => dd.DeviceId == unitId)
            .OrderByDescending(dd => dd.DeviceLocalTime)
            .FirstOrDefaultAsync(cancellationToken);

        var payload = PayloadDeserializer.GetElement(latestData?.Payload);
        var items = new List<HierarchyItemDto>();
        var breadcrumb = new List<BreadcrumbDto>();

        if (payload.HasValue)
        {
            NavigateAndExtract(payload.Value, level, parentId, items, breadcrumb);
        }

        return new HierarchyLevelResponseDto
        {
            Level = level,
            ParentId = parentId,
            Breadcrumb = breadcrumb,
            Items = items
        };
    }

    private static void NavigateAndExtract(
        JsonElement root,
        string targetLevel,
        string? parentId,
        List<HierarchyItemDto> items,
        List<BreadcrumbDto> breadcrumb)
    {
        var levelIndex = Array.IndexOf(HierarchyLevels, targetLevel.ToLowerInvariant());
        if (levelIndex < 0) return;

        var hasChildren = levelIndex < HierarchyLevels.Length - 1;
        var nextLevelKey = hasChildren ? HierarchyLevels[levelIndex + 1] + "s" : null;

        JsonElement? container = root;

        for (int i = 0; i < levelIndex; i++)
        {
            var currentLevelPluralKey = HierarchyLevels[i] + "s";
            container = PayloadDeserializer.GetNestedElement(container.Value, currentLevelPluralKey);
            if (!container.HasValue || container.Value.ValueKind != JsonValueKind.Array) return;

            JsonElement? found = null;
            foreach (var item in container.Value.EnumerateArray())
            {
                var itemId = item.TryGetProperty("id", out var idProp)
                    ? idProp.GetRawText().Trim('"')
                    : null;

                if (i == levelIndex - 1 && itemId == parentId)
                {
                    found = item;
                    var name = item.TryGetProperty("name", out var n)
                        ? n.GetString() ?? itemId ?? ""
                        : itemId ?? "";
                    breadcrumb.Add(new BreadcrumbDto { Id = itemId ?? "", Name = name, Level = HierarchyLevels[i] });
                    break;
                }
                else if (i < levelIndex - 1)
                {
                    var childPluralKey = HierarchyLevels[i + 1] + "s";
                    if (item.TryGetProperty(childPluralKey, out var children) &&
                        children.ValueKind == JsonValueKind.Array &&
                        ContainsId(children, parentId, HierarchyLevels, i + 1, levelIndex - 1))
                    {
                        found = item;
                        var name = item.TryGetProperty("name", out var n)
                            ? n.GetString() ?? itemId ?? ""
                            : itemId ?? "";
                        breadcrumb.Add(new BreadcrumbDto { Id = itemId ?? "", Name = name, Level = HierarchyLevels[i] });
                        break;
                    }
                }
            }

            if (!found.HasValue) return;
            container = found;
        }

        var targetPluralKey = HierarchyLevels[levelIndex] + "s";
        var targetArray = PayloadDeserializer.GetNestedElement(container.Value, targetPluralKey);
        if (!targetArray.HasValue || targetArray.Value.ValueKind != JsonValueKind.Array) return;

        var index = 0;
        foreach (var item in targetArray.Value.EnumerateArray())
        {
            var id = item.TryGetProperty("id", out var idProp)
                ? idProp.GetRawText().Trim('"')
                : $"{targetLevel}_{index}";

            var name = item.TryGetProperty("name", out var nameProp)
                ? nameProp.GetString() ?? $"{targetLevel} {index + 1}"
                : $"{targetLevel} {index + 1}";

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

            var childCount = 0;
            if (nextLevelKey != null && item.TryGetProperty(nextLevelKey, out var childrenArr) &&
                childrenArr.ValueKind == JsonValueKind.Array)
            {
                childCount = childrenArr.GetArrayLength();
            }

            items.Add(new HierarchyItemDto
            {
                Id = id,
                Name = name,
                Level = targetLevel,
                Status = StatusBadgeDto.FromStatus(status),
                Data = data.Count > 0 ? data : null,
                HasChildren = hasChildren && childCount > 0,
                ChildCount = childCount
            });

            index++;
        }
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
                var isDataLevel = !level.TryGetProperty("dataLevel", out var dl) || dl.GetBoolean();
                var isVirtual = level.TryGetProperty("virtual", out var vr) && vr.GetBoolean();
                var inheritsFrom = level.TryGetProperty("inheritsFrom", out var inf) ? inf.GetString() : null;

                parsed.Add(new HierarchyLevelDefinitionDto
                {
                    Key = key, Label = label, Depth = depth, HasChildren = hasChildren,
                    IsDataLevel = isDataLevel, IsVirtual = isVirtual, InheritsFrom = inheritsFrom
                });
                depth++;
            }
            return parsed.Count > 0 ? parsed : defaultLevels;
        }

        return defaultLevels;
    }

    private static bool ContainsId(JsonElement array, string? targetId, string[] levels, int currentDepth, int targetDepth)
    {
        if (targetId == null) return false;

        foreach (var item in array.EnumerateArray())
        {
            if (currentDepth == targetDepth)
            {
                var itemId = item.TryGetProperty("id", out var idProp)
                    ? idProp.GetRawText().Trim('"')
                    : null;
                if (itemId == targetId) return true;
            }
            else if (currentDepth < targetDepth)
            {
                var childKey = levels[currentDepth + 1] + "s";
                if (item.TryGetProperty(childKey, out var children) &&
                    children.ValueKind == JsonValueKind.Array &&
                    ContainsId(children, targetId, levels, currentDepth + 1, targetDepth))
                {
                    return true;
                }
            }
        }

        return false;
    }
}
