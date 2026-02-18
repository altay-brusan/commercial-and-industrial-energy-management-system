using Backend.Domain.Entities;

namespace Backend.Application.Tariffs;

public class TariffDto
{
    public Guid Id { get; set; }
    public string CountryName { get; set; } = string.Empty;
    public string? DistributorName { get; set; }
    public decimal DistributionFee { get; set; }
    public decimal TransmissionFee { get; set; }
    public decimal TaxRate { get; set; }
    public string Currency { get; set; } = "EUR";
    public bool IsActive { get; set; }
    public DateTimeOffset Created { get; set; }
    public string? CreatedBy { get; set; }
    public DateTimeOffset LastModified { get; set; }
    public string? LastModifiedBy { get; set; }

    private class Mapping : Profile
    {
        public Mapping()
        {
            CreateMap<Tariff, TariffDto>();
        }
    }
}

public class TariffBriefDto
{
    public Guid Id { get; set; }
    public string CountryName { get; set; } = string.Empty;
    public string? DistributorName { get; set; }
    public decimal DistributionFee { get; set; }
    public decimal TransmissionFee { get; set; }
    public decimal TaxRate { get; set; }
    public string Currency { get; set; } = "EUR";
    public bool IsActive { get; set; }

    private class Mapping : Profile
    {
        public Mapping()
        {
            CreateMap<Tariff, TariffBriefDto>();
        }
    }
}
