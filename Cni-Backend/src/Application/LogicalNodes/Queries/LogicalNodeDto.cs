namespace Backend.Application.LogicalNodes.Queries;

public record LogicalNodeDto
{
    public Guid Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string? DisplayName { get; init; }
    public string? Description { get; init; }
    public string Parameters { get; init; } = "[]";
}
