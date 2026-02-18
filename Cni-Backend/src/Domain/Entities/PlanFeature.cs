using Backend.Domain.Common;

namespace Backend.Domain.Entities;

public class PlanFeature : BaseAuditableEntity
{
    public Guid PlanId { get; set; }
    public Plan Plan { get; set; } = null!;

    public Guid FeatureId { get; set; }
    public Feature Feature { get; set; } = null!;
}
