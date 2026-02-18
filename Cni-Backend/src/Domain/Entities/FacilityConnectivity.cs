namespace Backend.Domain.Entities;

public class FacilityConnectivity : BaseAuditableEntity
{
    public Guid FacilityId { get; set; }
    public string RtuSerialNumber { get; set; } = string.Empty;
    public string? RtuModel { get; set; }
    public string? VpnIpAddress { get; set; }
    public string? PublicIpAddress { get; set; }
    public int? Port { get; set; }
    public string? Protocol { get; set; }
    public string? Status { get; set; }
    public DateTimeOffset? LastSeenUtc { get; set; }
    public string? Configuration { get; set; }

    // Navigation
    public Facility? Facility { get; set; }
}
