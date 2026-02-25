namespace Backend.Application.DeviceSpecificationTemplates.Queries;

public record DeviceSpecificationTemplateDto
{
    public Guid Id { get; init; }
    public Guid DeviceTypeId { get; init; }
    public string DeviceTypeName { get; init; } = string.Empty;
    public string Model { get; init; } = string.Empty;
    public string? Manufacturer { get; init; }
    public string SpecificationsTemplate { get; init; } = "{}";
    public string? ParameterMapTemplate { get; init; }
    public string MetadataTemplate { get; init; } = "{}";
    public string? HierarchyTemplate { get; init; }
    public bool IsDefault { get; init; }
}
