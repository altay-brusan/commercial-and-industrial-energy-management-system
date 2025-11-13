using Backend.Domain.Entities.Proto.Shared;
using System.Net.NetworkInformation;

namespace Backend.Domain.Entities.Proto;

public class AppPCSData : BaseAuditableEntity, IMultiTenantEntity
{
    public int TenantId { get; set; }
    public int FacilityId { get; set; }
    public int PCSId { get; set; }
    // Proto tanımındaki kompleks tipler (generic) kullanılacak:
    public AppDCPower DcPower { get; set; } = new();
    public AppACPower AcPower { get; set; } = new();
    public AppDeviceStatus Status { get; set; } = new();

    // Diğer alanlar proto tanımında yer alanlar:
    public double? TemperaturePcs { get; set; } // Temperature of the PCS in Celsius
    public double? Efficiency { get; set; } // Efficiency of the PCS in percentage
    public double? TotalEnergyDelivered { get; set; } // Total energy delivered to the grid in kWh
    public double?   TotalEnergyAbsorbed { get; set; } // Total energy absorbed from the grid in kWh
}
