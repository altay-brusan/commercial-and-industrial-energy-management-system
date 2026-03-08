namespace Backend.Domain.Entities.Huawei;

public class HuaweiAccount : BaseAuditableEntity
{
    public Guid TenantId { get; set; }
    public string UserName { get; set; } = string.Empty;
    public string EncryptedSystemCode { get; set; } = string.Empty;
    public string BaseUrl { get; set; } = "https://eu5.fusionsolar.huawei.com/thirdData";
    public bool IsActive { get; set; } = true;

    // Navigation
    public Tenant? Tenant { get; set; }
    public ICollection<HuaweiStationMapping> StationMappings { get; set; } = new List<HuaweiStationMapping>();
}
