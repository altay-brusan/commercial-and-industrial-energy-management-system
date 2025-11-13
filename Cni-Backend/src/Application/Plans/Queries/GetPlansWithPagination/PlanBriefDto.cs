using Backend.Domain.Entities;

namespace Backend.Application.Plans.Queries.GetPlansWithPagination;

public class PlanBriefDto
{
    public int Id { get; init; }
    public string Name { get; set; } = string.Empty;
    public string Modules { get; set; } = string.Empty; // JSON veya CSV formatında tutulabilir
    public bool ApiAccess { get; set; }
    public int MaxFacilities { get; set; }
    public int MaxUsers { get; set; }
    public decimal Price { get; set; }

    private class Mapping : Profile
    {
        public Mapping()
        {
            CreateMap<Plan, PlanBriefDto>();
        }
    }
}
