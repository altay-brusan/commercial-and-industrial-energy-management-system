using Backend.Application.Tenants.Queries.GetTenant;
using Backend.Domain.Entities;

namespace Backend.Application.Users.Queries.GetUserInfo;
public sealed class UserInfoDto
{
    public required string Email { get; init; }
    public List<UserInfoTenantDto>? Tenants { get; init; }
    public string? Name { get; set; }
    public string? Surname { get; set; }
    public string? Title { get; set; }
    public List<string>? Roles { get; set; }
}
public sealed class UserInfoTenantDto
{
    public int Id { get; init; }
    public string? Name { get; set; }
    public List<UserInfoTenantDto>? Items { get; set; }
}
public sealed class UserFirmFacilityDto
{
    public int? FirmId { get; set; }
    public string? FirmName { get; set; }
    public int? FacilityId { get; set; }
    public string? FacilityName { get; set; }
    private class Mapping : Profile
    {
        public Mapping()
        {
            CreateMap<UserFirmFacility, UserFirmFacilityDto>()
             .ForMember(dest => dest.FirmName, opt => opt.MapFrom(src => src.Firm != null ? src.Firm.Name : "")) 
             .ForMember(dest => dest.FacilityName, opt => opt.MapFrom(src => src.Facility != null ? src.Facility.Name : ""));
        }
    }
}
