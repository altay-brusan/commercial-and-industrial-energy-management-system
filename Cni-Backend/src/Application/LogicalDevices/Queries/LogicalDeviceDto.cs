namespace Backend.Application.LogicalDevices.Queries;

public record LogicalDeviceDto
{
    public Guid Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string? DisplayName { get; init; }
    public string? Description { get; init; }
    public string Category { get; init; } = string.Empty;
    public string Ports { get; init; } = "[]";
    public string? HierarchyTemplate { get; init; }
    public string? UiTemplate { get; init; }
    public List<LogicalDeviceNodeDto> Nodes { get; init; } = new();
}

public record LogicalDeviceNodeDto
{
    public Guid LogicalNodeId { get; init; }
    public string LogicalNodeName { get; init; } = string.Empty;
    public string? NodeAlias { get; init; }
    public int InstanceCount { get; init; }
    public int SortOrder { get; init; }
}
