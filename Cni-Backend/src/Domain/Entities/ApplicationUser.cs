using Microsoft.AspNetCore.Identity;

namespace Backend.Domain.Entities;

public class ApplicationUser : IdentityUser
{
    public int? TenantId { get; set; }
    public string? Name { get; set; }
    public string? Surname { get; set; }
    public string? Title { get; set; }
    public string? Description { get; set; }

    // Kullanıcının bağlı olduğu firma & tesis bilgileri
    public ICollection<UserFirmFacility> UserFirmFacilities { get; set; } = new List<UserFirmFacility>();
    public ICollection<UserTenantRole> UserTenantRoles { get; set; } = new List<UserTenantRole>();
}
