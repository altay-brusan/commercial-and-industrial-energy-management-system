namespace Backend.Domain.Entities;
public class UserFirmFacility : BaseAuditableEntity, IMultiTenantEntity
{
    public int TenantId { get; set; }
    public string? UserId { get; set; } 
    public ApplicationUser? User { get; set; }

    public int? FirmId { get; set; }
    public Firm? Firm { get; set; }

    public int? FacilityId { get; set; }
    public Facility? Facility { get; set; }

}
