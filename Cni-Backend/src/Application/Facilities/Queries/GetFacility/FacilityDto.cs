using Backend.Domain.Entities;
using Backend.Domain.Enums;

namespace Backend.Application.Facilities.Queries.GetFacility;

public class FacilityDto
{
    public Guid Id { get; init; }
    public Guid FirmId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public int EnergyCapacity { get; set; }
    public EnergyUnit EnergyUnit { get; set; }
    public double Longitude { get; set; }
    public double Latitude { get; set; }
    public string? Topology { get; set; }

    private class Mapping : Profile
    {
        public Mapping()
        {
            CreateMap<Facility, FacilityDto>();
        }
    }
}
