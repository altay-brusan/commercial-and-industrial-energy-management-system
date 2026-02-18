using Backend.Domain.Entities;

namespace Backend.Application.FacilityTariffs;

public class FacilityTariffDto
{
    public Guid Id { get; set; }
    public Guid FacilityId { get; set; }
    public string FacilityName { get; set; } = string.Empty;
    public Guid TariffId { get; set; }
    public string TariffCountryName { get; set; } = string.Empty;
    public string? TariffDistributorName { get; set; }
    public decimal DistributionFee { get; set; }
    public decimal TransmissionFee { get; set; }
    public decimal TaxRate { get; set; }
    public string Currency { get; set; } = "EUR";
    public bool IsActive { get; set; }

    // Contract details
    public string? ContractNumber { get; set; }
    public decimal DiscountRate { get; set; }
    public DateTimeOffset? ContractStartDate { get; set; }
    public DateTimeOffset? ContractEndDate { get; set; }
    public string? Notes { get; set; }

    public DateTimeOffset Created { get; set; }
    public string? CreatedBy { get; set; }
    public DateTimeOffset LastModified { get; set; }
    public string? LastModifiedBy { get; set; }

    private class Mapping : Profile
    {
        public Mapping()
        {
            CreateMap<FacilityTariff, FacilityTariffDto>()
                .ForMember(d => d.FacilityName, opt => opt.MapFrom(s => s.Facility != null ? s.Facility.Name : string.Empty))
                .ForMember(d => d.TariffCountryName, opt => opt.MapFrom(s => s.Tariff != null ? s.Tariff.CountryName : string.Empty))
                .ForMember(d => d.TariffDistributorName, opt => opt.MapFrom(s => s.Tariff != null ? s.Tariff.DistributorName : null))
                .ForMember(d => d.DistributionFee, opt => opt.MapFrom(s => s.Tariff != null ? s.Tariff.DistributionFee : 0))
                .ForMember(d => d.TransmissionFee, opt => opt.MapFrom(s => s.Tariff != null ? s.Tariff.TransmissionFee : 0))
                .ForMember(d => d.TaxRate, opt => opt.MapFrom(s => s.Tariff != null ? s.Tariff.TaxRate : 0))
                .ForMember(d => d.Currency, opt => opt.MapFrom(s => s.Tariff != null ? s.Tariff.Currency : "EUR"));
        }
    }
}

public class FacilityTariffBriefDto
{
    public Guid Id { get; set; }
    public Guid FacilityId { get; set; }
    public string FacilityName { get; set; } = string.Empty;
    public Guid TariffId { get; set; }
    public string TariffCountryName { get; set; } = string.Empty;
    public string? TariffDistributorName { get; set; }
    public bool IsActive { get; set; }
    public string? ContractNumber { get; set; }
    public decimal DiscountRate { get; set; }

    private class Mapping : Profile
    {
        public Mapping()
        {
            CreateMap<FacilityTariff, FacilityTariffBriefDto>()
                .ForMember(d => d.FacilityName, opt => opt.MapFrom(s => s.Facility != null ? s.Facility.Name : string.Empty))
                .ForMember(d => d.TariffCountryName, opt => opt.MapFrom(s => s.Tariff != null ? s.Tariff.CountryName : string.Empty))
                .ForMember(d => d.TariffDistributorName, opt => opt.MapFrom(s => s.Tariff != null ? s.Tariff.DistributorName : null));
        }
    }
}
