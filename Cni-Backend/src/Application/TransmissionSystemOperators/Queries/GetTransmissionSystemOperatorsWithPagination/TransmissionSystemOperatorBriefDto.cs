using Backend.Domain.Entities;

namespace Backend.Application.TransmissionSystemOperators.Queries.GetTransmissionSystemOperatorsWithPagination;

public class TransmissionSystemOperatorBriefDto
{
    public int Id { get; init; }
    public int TenantId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public decimal TaxRate { get; set; }
    public decimal DistributionFee { get; set; }
    public string Currency { get; set; } = string.Empty;
    public bool IsActive { get; set; }

    private class Mapping : Profile
    {
        public Mapping()
        {
            CreateMap<TransmissionSystemOperator, TransmissionSystemOperatorBriefDto>();
        }
    }
}
