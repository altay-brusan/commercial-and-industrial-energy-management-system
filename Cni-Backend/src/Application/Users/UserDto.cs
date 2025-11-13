using Backend.Domain.Common;
using Backend.Domain.Entities;

namespace Backend.Application.Users;
public sealed class UserDto
{
    public required string UserId { get; set; }
    public required string Email { get; init; }
    public int? TenantId { get; set; }
    public string? Name { get; set; }
    public string? Surname { get; set; }
    public string? Title { get; set; }
    public List<string>? Roles { get; set; }
    public List<UserFirmFacilityUserDto>? UserFirmFacilities { get; set; }

}
public class UserFirmFacilityUserDto
{
    public int? FirmId { get; set; }
    public int? FacilityId { get; set; }

}
