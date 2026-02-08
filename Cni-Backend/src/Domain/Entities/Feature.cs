using Backend.Domain.Common;

namespace Backend.Domain.Entities;

public class Feature : BaseAuditableEntity
{
    public string Key { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? Category { get; set; }
    public bool IsActive { get; set; } = true;

    public ICollection<Permission> Permissions { get; set; } = new List<Permission>();
    public ICollection<PlanFeature> PlanFeatures { get; set; } = new List<PlanFeature>();
}
