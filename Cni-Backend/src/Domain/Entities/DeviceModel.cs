namespace Backend.Domain.Entities;

public class DeviceModel : BaseAuditableEntity
{
    public Guid DeviceTypeId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Manufacturer { get; set; }
    public string? PortTemplate { get; set; }
    public string? Specifications { get; set; }

    // Navigation
    public DeviceType? DeviceType { get; set; }
    public ICollection<Device> Devices { get; set; } = new List<Device>();
}
