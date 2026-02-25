namespace Backend.Application.Common.Models.Monitoring;

public record HierarchyResponseDto
{
    public List<HierarchyLevelDefinitionDto> Levels { get; init; } = new();
    public List<HierarchyItemDto> RootItems { get; init; } = new();
}

public record HierarchyLevelDefinitionDto
{
    public string Key { get; init; } = string.Empty;
    public string Label { get; init; } = string.Empty;
    public int Depth { get; init; }
    public bool HasChildren { get; init; }
    public bool IsDataLevel { get; init; } = true;
    public bool IsVirtual { get; init; }
    public string? InheritsFrom { get; init; }
}

public record HierarchyItemDto
{
    public string Id { get; init; } = string.Empty;
    public string Name { get; init; } = string.Empty;
    public string Level { get; init; } = string.Empty;
    public StatusBadgeDto? Status { get; init; }
    public Dictionary<string, object?>? Data { get; init; }
    public bool HasChildren { get; init; }
    public int ChildCount { get; init; }
    public bool IsVirtual { get; init; }
}

public record HierarchyLevelResponseDto
{
    public string Level { get; init; } = string.Empty;
    public string? ParentId { get; init; }
    public List<BreadcrumbDto> Breadcrumb { get; init; } = new();
    public List<HierarchyItemDto> Items { get; init; } = new();
}

public record BreadcrumbDto
{
    public string Id { get; init; } = string.Empty;
    public string Name { get; init; } = string.Empty;
    public string Level { get; init; } = string.Empty;
}
