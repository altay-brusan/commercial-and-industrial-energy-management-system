using Backend.Application.Tenants.Queries.GetTenant;
using Backend.Application.Users.Queries.GetUserInfo;
using Backend.Domain.Entities;

namespace Backend.Application.Users.Queries.GetUser;

public class UserDto
{
    public required string Id { get; set; }
    public required string Email { get; init; }
    public TenantDto? Tenant { get; init; }
    public string? Name { get; set; }
    public string? Surname { get; set; }
    public string? Title { get; set; }
    public string? Description { get; set; }
    public List<string>? Roles { get; set; }
    public List<UserFirmFacilityDto>? UserFirmFacilities { get; set; }


    private class Mapping : Profile
    {
        public Mapping()
        {
            CreateMap<ApplicationUser, UserDto>();
        }
    }
}
