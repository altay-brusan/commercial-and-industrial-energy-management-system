namespace Backend.Domain.Entities.Huawei;

public class HuaweiDeviceMapping : BaseAuditableEntity
{
    public Guid StationMappingId { get; set; }
    public long HuaweiDeviceId { get; set; }
    public int HuaweiDevTypeId { get; set; }
    public Guid DeviceId { get; set; }
    public string DeviceName { get; set; } = string.Empty;
    public string? SerialNumber { get; set; }
    public string? Model { get; set; }

    // Navigation
    public HuaweiStationMapping? StationMapping { get; set; }
    public Device? Device { get; set; }
}
