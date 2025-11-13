using Backend.Domain.Entities.Proto.Shared;
using System.Net.NetworkInformation;

namespace Backend.Domain.Entities.Proto;

public class AppSolarData : BaseAuditableEntity, IMultiTenantEntity
{
    public int TenantId { get; set; }
    public int FacilityId { get; set; }
    public int SolarId { get; set; }
    // Kompleks tipler
    public AppDCPower PanelPower { get; set; } = new();
    public AppDCPower ChargerPower { get; set; } = new();
    public AppDCPower DCOutput { get; set; } = new();
    public AppACPower ACOutput { get; set; } = new();

    // Yeni alanlar
    public double? PerformanceRatio { get; set; }
    public double? PvPanelTemperature { get; set; }
    public double? SolarEnergyProduced { get; set; }

    // Status bilgisi
    public AppDeviceStatus Status { get; set; } = new();

}
