using Backend.Domain.Entities;

namespace Backend.Application.FirmMarketOperators.Queries.GetFirmMarketOperator;

public class FirmMarketOperatorDto
{
    public int Id { get; init; }
    public int FirmId { get; set; }
    public int MarketOperatorId { get; set; }
    public string MarketOperatorName { get; set; } = string.Empty;
    public string MarketOperatorCode { get; set; } = string.Empty;
    public string AreaCode { get; set; } = string.Empty;
    public int SamplingPeriodMinutes { get; set; }
    public bool IsActive { get; set; }

    private class Mapping : Profile
    {
        public Mapping()
        {
            CreateMap<FirmMarketOperator, FirmMarketOperatorDto>()
                .ForMember(d => d.MarketOperatorName, opt => opt.MapFrom(s => s.MarketOperator != null ? s.MarketOperator.Name : string.Empty))
                .ForMember(d => d.MarketOperatorCode, opt => opt.MapFrom(s => s.MarketOperator != null ? s.MarketOperator.Code : string.Empty));
        }
    }
}
