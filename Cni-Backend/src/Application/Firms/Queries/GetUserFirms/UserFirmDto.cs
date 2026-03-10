using Backend.Domain.Entities;

namespace Backend.Application.Firms.Queries.GetUserFirms;

public class UserFirmDto
{
    public Guid Id { get; init; }
    public string? Name { get; init; }
    public string? LogoUrl { get; init; }
    public string? Type { get; init; }
    public string? City { get; init; }
    public string? Country { get; init; }

    private class Mapping : Profile
    {
        public Mapping()
        {
            CreateMap<Firm, UserFirmDto>();
        }
    }
}
