using Backend.Domain.Common;
using Backend.Domain.Entities;

namespace Backend.Application.Users;
public sealed class UserDto
{
    public required string UserId { get; set; }
    public required string Email { get; init; }
    public Guid? TenantId { get; set; }
    public string? Name { get; set; }
    public string? Surname { get; set; }
    public string? Title { get; set; }
    public List<string>? Roles { get; set; }
    public List<UserFirmFacilityUserDto>? UserFirmFacilities { get; set; }

}
public class UserFirmFacilityUserDto
{
    public Guid? FirmId { get; set; }
    public Guid? FacilityId { get; set; }

}
