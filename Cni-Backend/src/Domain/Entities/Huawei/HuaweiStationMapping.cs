namespace Backend.Domain.Entities.Huawei;

public class HuaweiStationMapping : BaseAuditableEntity
{
    public Guid HuaweiAccountId { get; set; }
    public string StationCode { get; set; } = string.Empty;
    public string StationName { get; set; } = string.Empty;
    public Guid FacilityId { get; set; }
    public bool SyncEnabled { get; set; } = true;
    public DateTimeOffset? LastSyncUtc { get; set; }

    // Navigation
    public HuaweiAccount? HuaweiAccount { get; set; }
    public Facility? Facility { get; set; }
    public ICollection<HuaweiDeviceMapping> DeviceMappings { get; set; } = new List<HuaweiDeviceMapping>();
}
