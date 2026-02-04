using Backend.Domain.Entities;

namespace Backend.Application.TenantMarketOperators.Queries.GetTenantMarketOperator;

public class TenantMarketOperatorDto
{
    public int Id { get; init; }
    public int TenantId { get; set; }
    public int MarketOperatorId { get; set; }
    public string MarketOperatorName { get; set; } = string.Empty;
    public string MarketOperatorCode { get; set; } = string.Empty;
    public string AreaCode { get; set; } = string.Empty;
    public int SamplingFrequencyMinutes { get; set; }
    public bool IsActive { get; set; }

    private class Mapping : Profile
    {
        public Mapping()
        {
            CreateMap<TenantMarketOperator, TenantMarketOperatorDto>()
                .ForMember(d => d.MarketOperatorName, opt => opt.MapFrom(s => s.MarketOperator != null ? s.MarketOperator.Name : string.Empty))
                .ForMember(d => d.MarketOperatorCode, opt => opt.MapFrom(s => s.MarketOperator != null ? s.MarketOperator.Code : string.Empty));
        }
    }
}
