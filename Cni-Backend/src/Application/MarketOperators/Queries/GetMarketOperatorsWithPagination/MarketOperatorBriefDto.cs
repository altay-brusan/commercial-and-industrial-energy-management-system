using Backend.Domain.Entities;

namespace Backend.Application.MarketOperators.Queries.GetMarketOperatorsWithPagination;

public class MarketOperatorBriefDto
{
    public int Id { get; init; }
    public string Name { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public List<int> SupportedResolutions { get; set; } = new();
    public List<string> AreaCodes { get; set; } = new();
    public bool IsActive { get; set; }

    private class Mapping : Profile
    {
        public Mapping()
        {
            CreateMap<MarketOperator, MarketOperatorBriefDto>();
        }
    }
}
