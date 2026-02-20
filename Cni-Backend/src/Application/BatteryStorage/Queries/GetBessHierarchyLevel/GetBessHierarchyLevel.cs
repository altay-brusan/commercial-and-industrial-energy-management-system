using System.Text.Json;
using Backend.Application.Common.Interfaces;
using Backend.Application.Common.Models.Monitoring;
using Backend.Application.Common.Security;
using Backend.Application.Common.Services;

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
            .Where(d => d.Id == request.UnitId && d.DeviceType != null && d.DeviceType.Name == "Battery")
            .FirstOrDefaultAsync(cancellationToken);

        Guard.Against.NotFound(request.UnitId, device);

        var latestData = await _context.DeviceBatteryData
            .Where(dd => dd.DeviceId == request.UnitId)
            .OrderByDescending(dd => dd.DeviceLocalTime)
            .FirstOrDefaultAsync(cancellationToken);

        var payload = PayloadDeserializer.GetElement(latestData?.Payload);
        var items = new List<HierarchyItemDto>();
        var breadcrumb = new List<BreadcrumbDto>();

        if (payload.HasValue)
        {
            NavigateAndExtract(payload.Value, request.Level, request.ParentId, items, breadcrumb);
        }

        return new HierarchyLevelResponseDto
        {
            Level = request.Level,
            ParentId = request.ParentId,
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

        // Navigate to the parent container in the payload tree
        JsonElement? container = root;

        // Walk up from the root level to reach the target parent
        for (int i = 0; i < levelIndex; i++)
        {
            var currentLevelPluralKey = HierarchyLevels[i] + "s";
            container = PayloadDeserializer.GetNestedElement(container.Value, currentLevelPluralKey);
            if (!container.HasValue || container.Value.ValueKind != JsonValueKind.Array) return;

            // Find the right parent in this level
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
                    // Check if this branch contains our parentId deeper
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

        // Now extract children at target level from the parent container
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
            if (nextLevelKey != null && item.TryGetProperty(nextLevelKey, out var children) &&
                children.ValueKind == JsonValueKind.Array)
            {
                childCount = children.GetArrayLength();
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
