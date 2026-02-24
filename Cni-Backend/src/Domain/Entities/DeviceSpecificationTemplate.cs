namespace Backend.Domain.Entities;

public class DeviceSpecificationTemplate : BaseAuditableEntity
{
    public Guid DeviceTypeId { get; set; }
    public string Model { get; set; } = string.Empty;
    public string? Manufacturer { get; set; }
    public string SpecificationsTemplate { get; set; } = "{}";
    public string? ParameterMapTemplate { get; set; }
    public string MetadataTemplate { get; set; } = "{}";
    public string? HierarchyTemplate { get; set; }
    public bool IsDefault { get; set; }

    // Navigation
    public DeviceType? DeviceType { get; set; }
    public ICollection<Device> Devices { get; set; } = new List<Device>();
}
