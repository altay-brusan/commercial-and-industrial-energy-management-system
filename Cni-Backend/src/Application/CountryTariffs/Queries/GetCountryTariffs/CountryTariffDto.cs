using Backend.Domain.Entities;

namespace Backend.Application.CountryTariffs.Queries.GetCountryTariffs;

public class CountryTariffDto
{
    public int Id { get; init; }
    public int TenantId { get; set; }
    public string CountryCode { get; set; } = string.Empty;
    public string CountryName { get; set; } = string.Empty;
    public decimal DistributionFee { get; set; }
    public decimal TransmissionFee { get; set; }
    public decimal TaxRate { get; set; }
    public bool IsActive { get; set; }

    private class Mapping : Profile
    {
        public Mapping()
        {
            CreateMap<CountryTariff, CountryTariffDto>();
        }
    }
}
