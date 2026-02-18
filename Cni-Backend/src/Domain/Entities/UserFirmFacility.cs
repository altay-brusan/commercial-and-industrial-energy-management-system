namespace Backend.Domain.Entities;
public class UserFirmFacility : BaseAuditableEntity, IMultiTenantEntity
{
    public Guid TenantId { get; set; }
    public string? UserId { get; set; }
    public ApplicationUser? User { get; set; }

    public Guid? FirmId { get; set; }
    public Firm? Firm { get; set; }

    public Guid? FacilityId { get; set; }
    public Facility? Facility { get; set; }

}
