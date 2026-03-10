using Backend.Domain.Entities;
using Backend.Domain.Enums;

namespace Backend.Application.Firms.Queries.GetUserFirmFacilities;

public class UserFirmFacilityDto
{
    public Guid Id { get; init; }
    public Guid FirmId { get; init; }
    public string Name { get; init; } = string.Empty;
    public string Address { get; init; } = string.Empty;
    public int EnergyCapacity { get; init; }
    public EnergyUnit EnergyUnit { get; init; }
    public double Longitude { get; init; }
    public double Latitude { get; init; }

    private class Mapping : Profile
    {
        public Mapping()
        {
            CreateMap<Facility, UserFirmFacilityDto>();
        }
    }
}
