using Backend.Domain.Entities;

namespace Backend.Application.Tenants.Queries.GetTenantsWithPagination;

public class TenantBriefDto
{
    public Guid Id { get; init; }
    public string? Identifier { get; set; }
    public string? Name { get; set; }

    private class Mapping : Profile
    {
        public Mapping()
        {
            CreateMap<Tenant, TenantBriefDto>();
        }
    }
}
