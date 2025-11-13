using System.ComponentModel.DataAnnotations.Schema;

namespace Backend.Domain.Entities.Topology;

public class TopologyNode : BaseAuditableEntity, IMultiTenantEntity
{
    public int? DeviceId { get; set; } // Saha cihaz ID'si
    public int TenantId { get; set; }
    public TopologyNodeTypes Type { get; set; }
    public string? Extent { get; set; } // Düz çizgide null dönecek
    public int? ParentId { get; set; } // GroupId boş olabilir
    public int? TargetHandleCount { get; set; } // Tek nokta bağlanacaksa null, birden fazla bağlantı çıkacaksa sayısı girilecek
    public int? SourceHandleCount { get; set; } // Tek noktaysa boş olacak, birden fazla nokta varsa değer girilecek
    public string? Data { get; set; } // { label: "Energy Storage Units" }
    public string? Position { get; set; } // { x: 0, y: -300 }
    public string? Style { get; set; } // { width: 1000, height: 360, backgroundColor: 'transparent', borderStyle: 'dashed' }

    // Facility ile ilişki
    [ForeignKey("Facility")]
    public int FacilityId { get; set; }
    public Facility? Facility { get; set; }
    public Tenant? Tenant { get; set; }
    public Device? Device { get; set; }

    public ICollection<TopologyEdge>? SourceEdges { get; set; }
    public ICollection<TopologyEdge>? TargetEdges { get; set; }
}
