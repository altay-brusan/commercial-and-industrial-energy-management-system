using Backend.Domain.Entities.Proto.DTO;
using Backend.Domain.Entities.Proto.Shared;

public class AppBESSData : BaseAuditableEntity, IMultiTenantEntity
{
    public int TenantId { get; set; }
    public int FacilityId { get; set; }
    public int BESSId { get; set; }

    // Ana BESS Özellikleri
    public double? StateOfCharge { get; set; }
    public double? StateOfHealth { get; set; }
    public AppDCPower DcPower { get; set; } = new(); // DC_Power kompleks tipi
    public double? BatteryTemperature { get; set; }
    public double? TotalEnergyStored { get; set; }
    public double? EnergyThroughput { get; set; }
    public double? ChargeLimit { get; set; }
    public double? DischargeLimit { get; set; }

    // Yeni Eklenenler
    public int? NumberOfPacks { get; set; }
    public int? NumberOfModules { get; set; }
    public int? NumberOfCells { get; set; }
    public double? Capacity { get; set; }
    public bool CoolingActive { get; set; }

    // Durum Bilgileri
    public AppBESSStatus ChargeStatus { get; set; }
    public AppDeviceStatus Status { get; set; } = new();

    // Kaldırılan Alanlar:
    // - Hücre seviyesi voltaj/sıcaklık metrikleri
    // - Solar şarj ile ilgili alanlar
    // - İnvertör detayları
    // - Grid etkileşim metrikleri
}
public enum AppBESSStatus { Standby, Charging, Discharging, Error }
