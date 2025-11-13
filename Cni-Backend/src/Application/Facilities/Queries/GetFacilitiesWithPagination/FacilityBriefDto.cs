using Backend.Domain.Entities;

namespace Backend.Application.Facilities.Queries.GetFacilitiesWithPagination;

public class FacilityBriefDto
{
    public int Id { get; init; }
    public int TenantId { get; set; }
    public int FirmId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public int Capacity { get; set; }
    public double Longitude { get; set; }
    public double Latitude { get; set; }
    public bool GrpcStatus { get; set; }
    public string GrpcChannel { get; set; } = string.Empty;

    private class Mapping : Profile
    {
        public Mapping()
        {
            CreateMap<Facility, FacilityBriefDto>();
        }
    }
}
