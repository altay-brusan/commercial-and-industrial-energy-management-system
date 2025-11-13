using Backend.Domain.Entities;

namespace Backend.Application.Facilities.Queries.GetFacility;

public class FacilityDto
{
    public int Id { get; init; }
    public int TenantId { get; set; }
    public int FirmId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public int Capacity { get; set; }
    public double Longitude { get; set; }
    public double Latitude { get; set; }

    private class Mapping : Profile
    {
        public Mapping()
        {
            CreateMap<Facility, FacilityDto>();
        }
    }
}
