using Backend.Domain.Entities;

namespace Backend.Application.TransmissionSystemOperators.Queries.GetTransmissionSystemOperator;

public class TransmissionSystemOperatorDto
{
    public int Id { get; init; }
    public int TenantId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public string BaseUrl { get; set; } = string.Empty;
    public string? ApiKey { get; set; }
    public decimal TaxRate { get; set; }
    public decimal DistributionFee { get; set; }
    public int TimeResolutionMinutes { get; set; }
    public string Currency { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public string? AreaCode { get; set; }

    private class Mapping : Profile
    {
        public Mapping()
        {
            CreateMap<TransmissionSystemOperator, TransmissionSystemOperatorDto>();
        }
    }
}
