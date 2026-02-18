namespace Backend.Domain.Entities;

public class Firm : BaseAuditableEntity, IMultiTenantEntity
{
    public Guid TenantId { get; set; }
    public string? Name { get; set; } 
    public string? LogoUrl { get; set; }
    public string? Type { get; set; } 
    public string? Country { get; set; } 
    public string? State { get; set; } 
    public string? City { get; set; } 
    public string? District { get; set; } 
    public string? PostalCode { get; set; } 
    public string? Address { get; set; } 
    public string? ContactEmail { get; set; }
    public string? ContactPhone { get; set; }
    public string? Fax { get; set; }
    public string? TaxNumber { get; set; } 
    public string? TaxOffice { get; set; } 
    
    public ICollection<Facility>? Facilities { get; set; }

    public Tenant? Tenant { get; set; }

    // Kullanıcının bağlı olduğu firma & tesis bilgileri
    public ICollection<UserFirmFacility> UserCompanyFacilities { get; set; } = new List<UserFirmFacility>();
}
