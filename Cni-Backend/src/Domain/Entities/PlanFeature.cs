using Backend.Domain.Common;

namespace Backend.Domain.Entities;

public class PlanFeature : BaseAuditableEntity
{
    public int PlanId { get; set; }
    public Plan Plan { get; set; } = null!;

    public int FeatureId { get; set; }
    public Feature Feature { get; set; } = null!;
}
