using Backend.Domain.Entities;

namespace Backend.Application.Firms.Queries.GetFirm;

public class FirmDto
{
    public int Id { get; init; }
    public int TenantId { get; set; }
    public string? Name { get; set; }
    public string? LogoUrl { get; set; }
    public string? Type { get; set; }
    public string? Country { get; set; }
    public string? State { get; set; }
    public string? City { get; set; }
    public string? District { get; set; }
    public string? PostalCode { get; set; }
    public string? Address { get; set; }
    public string? ContactEmail { get; set; }
    public string? ContactPhone { get; set; }
    public string? Fax { get; set; }
    public string? TaxNumber { get; set; }
    public string? TaxOffice { get; set; }

    private class Mapping : Profile
    {
        public Mapping()
        {
            CreateMap<Firm, FirmDto>();
        }
    }
}
