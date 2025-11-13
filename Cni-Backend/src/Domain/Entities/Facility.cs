namespace Backend.Domain.Entities;
public class Facility : BaseAuditableEntity, IMultiTenantEntity
{
    public int TenantId { get; set; }
    public int FirmId { get;set; }
    public string Name { get; set; } = string.Empty; 
    public string Address { get; set; } = string.Empty; 
    public int Capacity { get; set; } 
    public double Longitude { get; set; } 
    public double Latitude { get; set; }
    public bool GrpcStatus { get; set; }
    public string GrpcChannel { get; set; } = string.Empty;
    public Tenant? Tenant { get; set; }
    public Firm? Firm { get; set; }

    // Kullanıcının bağlı olduğu firma & tesis bilgileri
    public ICollection<UserFirmFacility> UserCompanyFacilities { get; set; } = new List<UserFirmFacility>();

}
