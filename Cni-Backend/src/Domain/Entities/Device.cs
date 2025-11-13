using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Backend.Domain.Entities.Topology;

namespace Backend.Domain.Entities;
public class Device : BaseAuditableEntity, IMultiTenantEntity
{
    public int TenantId { get; set; }

    [Required]

    [ForeignKey("Facility")]
    public int FacilityId { get; set; }
    public Facility? Facility { get; set; }
    public Tenant? Tenant { get; set; }

    // Bu cihaza bağlı tek alt cihaz
    public Device? ConnectedDevice { get; set; }
    public required string MachineId { get; set; }
    public string? DeviceName { get; set; }
    [ForeignKey("DeviceType")]
    public int DeviceTypeId { get; set; }
    public DeviceType? DeviceType { get; set; }
    [ForeignKey("SourceDevice")]
    public int? SourceDeviceId { get; set; }
    public Device? SourceDevice { get; set; }

    public bool GrpcConnection { get; set; }
    public int GrpcDeviceStatus { get; set; }
    public DateTime? GrpcLastDataReviceDate { get; set; }

    public TopologyNode? TopologyNode { get; set; }

}
