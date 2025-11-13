using Backend.Domain.Entities;

namespace Backend.Application.Tenants.Queries.GetTenant;

public class TenantDto
{
    public int Id { get; init; }
    public string? Identifier { get; set; }
    public string? Name { get; set; }
    public string? ConnectionString { get; set; } 
    public string? Items { get; set; }

    private class Mapping : Profile
    {
        public Mapping()
        {
            CreateMap<Tenant, TenantDto>();
        }
    }
}
