using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace Backend.Domain.Entities.Topology;

public class TopologyEdge : BaseAuditableEntity, IMultiTenantEntity
{
    public int TenantId { get; set; }

    [Required]
    [ForeignKey("SourceNode")]
    public int SourceId { get; set; }
    public TopologyNode? SourceNode { get; set; }

    [Required]
    [ForeignKey("TargetNode")]
    public int TargetId { get; set; }
    public TopologyNode? TargetNode { get; set; }

    public TopologyEdgeTypes Type { get; set; }
    public string? Data { get; set; } // {status=true} veya {status=false}
    [ForeignKey("Facility")]
    public int FacilityId { get; set; }
    public Facility? Facility { get; set; }
    public Tenant? Tenant { get; set; }
    public string? TargetHandle { get; set; } // Tek noktaysa boş olacak, birden fazla nokta varsa değer girilecek
    public string? SourceHandle { get; set; } // Tek noktaysa boş olacak, birden fazla nokta varsa değer girilecek
}
