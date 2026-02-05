using Backend.Domain.Entities;

namespace Backend.Application.MarketOperators.Queries.GetMarketOperator;

public class MarketOperatorDto
{
    public int Id { get; init; }
    public string Name { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public string BaseUrl { get; set; } = string.Empty;
    public string? ApiKey { get; set; }
    public List<int> SupportedResolutions { get; set; } = new();
    public List<string> AreaCodes { get; set; } = new();
    public bool IsActive { get; set; }

    private class Mapping : Profile
    {
        public Mapping()
        {
            CreateMap<MarketOperator, MarketOperatorDto>();
        }
    }
}
