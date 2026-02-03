using Backend.Domain.Entities;

namespace Backend.Application.EnergyPrices.Queries.GetEnergyPrice;

public class EnergyPriceDto
{
    public int Id { get; init; }
    public int TsoId { get; set; }
    public string TsoName { get; set; } = string.Empty;
    public string TsoCode { get; set; } = string.Empty;
    public decimal RawPrice { get; set; }
    public decimal AppliedTaxRate { get; set; }
    public decimal AppliedDistributionFee { get; set; }
    public decimal TotalPrice { get; set; }
    public DateTimeOffset PriceTimestamp { get; set; }
    public string Currency { get; set; } = string.Empty;

    private class Mapping : Profile
    {
        public Mapping()
        {
            CreateMap<EnergyPriceHistory, EnergyPriceDto>()
                .ForMember(d => d.TsoName, opt => opt.MapFrom(s => s.Tso != null ? s.Tso.Name : string.Empty))
                .ForMember(d => d.TsoCode, opt => opt.MapFrom(s => s.Tso != null ? s.Tso.Code : string.Empty));
        }
    }
}
