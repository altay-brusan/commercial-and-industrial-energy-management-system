using Backend.Domain.Entities.Proto.Shared;
using System.Net.NetworkInformation;

namespace Backend.Domain.Entities.Proto;

public class AppSmartMeterData : BaseAuditableEntity, IMultiTenantEntity
{
    public int TenantId { get; set; }
    public int FacilityId { get; set; }
    public int SmartMeterId { get; set; }

    public AppACPower AcPower { get; set; } = new();

    // Enerji değerleri (yeni proto tanımına göre double olarak)
    public double? ActiveEnergyImport { get; set; }
    public double? ActiveEnergyExport { get; set; }
    public double? ReactiveEnergyImport { get; set; }
    public double? ReactiveEnergyExport { get; set; }
    public double? TotalActiveEnergy { get; set; }
    public double? TotalReactiveEnergy { get; set; }
    public double? CurrentDemand { get; set; }
    public double? PowerDemand { get; set; }

    // Smart Meter durum bilgisini içeren generic tip
    public AppDeviceStatus Status { get; set; } = new();
}
