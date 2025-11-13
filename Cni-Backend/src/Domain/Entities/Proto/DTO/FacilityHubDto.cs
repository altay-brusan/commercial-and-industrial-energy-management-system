using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Backend.Domain.Entities.Proto.DTO;


public class GeneratorDeviceDto
{
    public int DeviceId { get; set; }
    public int? NodeId { get; set; }
    public double TotalPower { get; set; }
    public double LoadGeneratorContribution { get; set; }
}

public class SystemDataDto
{
    public double GeneratorPower { get; set; }
    public double GridPower { get; set; }
    public double LoadGeneratorContribution { get; set; }
    public double LoadGridContribution { get; set; }
    public double LoadPcsContribution { get; set; }
    public double LoadSolarContribution { get; set; }
    public double PcsPower { get; set; }
    public double SolarPower { get; set; }
    public double? Cost_Saving_TL_Daily { get; set; }
    public double? Grid_Consumption_TL_Daily { get; set; }

}


public class FacilityHubDto
{
    public List<BESSDeviceDto> BessDevices { get; set; } = new();
    public List<SolarDeviceDto> SolarDevices { get; set; } = new();
    public List<PCSDeviceDto> PcsDevices { get; set; } = new();
    public List<GeneratorDeviceDto> GeneratorDevices { get; set; } = new();
    public List<GridPowerDto> GridDevices { get; set; } = new();
    public FacilityPowerDto FacilityData { get; set; } = new();
    public SystemDataDto SystemData { get; set; } = new();
}

// BESS Cihazları
public class BESSDeviceDto
{
    public int DeviceId { get; set; }
    public int? NodeId { get; set; }
    public string? DeviceName { get; set; }
    public double StateOfCharge { get; set; }
    public double StateOfHealth { get; set; }
    public DCPowerDto DcPower { get; set; } = new();
    public double BatteryTemperature { get; set; }
    public double TotalEnergyStored { get; set; }
    public bool CoolingActive { get; set; }
    public double Capacity { get; set; }
    public BESSStatusDto ChargeStatus { get; set; }
    public DeviceStatusDto Status { get; set; } = new();
}

// Solar Cihazlar
public class SolarDeviceDto
{
    public int DeviceId { get; set; }
    public int? NodeId { get; set; }
    public string?DeviceName { get; set; }
    public DCPowerDto DCOutput { get; set; } = new();
    public DCPowerDto PanelPower { get; set; } = new();
    public double PerformanceRatio { get; set; }
    public double PvPanelTemperature { get; set; }
    public DeviceStatusDto Status { get; set; } = new();
    public double? Weather { get; set; }
    public double? Irradiance { get; set; }
}

// PCS Cihazları
public class PCSDeviceDto
{
    public int DeviceId { get; set; }

    public int? NodeId { get; set; }
    public string? DeviceName { get; set; }
    public double Efficiency { get; set; }
    public double TemperaturePcs { get; set; }
    public ACPowerDto AcPower { get; set; } = new();
    public DCPowerDto DcPower { get; set; } = new();
    public DeviceStatusDto Status { get; set; } = new();
}

// Grid ve Facility Özel Yapıları
public class GridPowerDto
{
    public int? NodeId { get; set; }
    public int DeviceId { get; set; }
    public string? DeviceName { get; set; }

    public ACPowerDto AcPower { get; set; } = new();
    public DeviceStatusDto Status { get; set; } = new();
}

public class FacilityPowerDto
{
    public int DeviceId { get; set; }

    public int? NodeId { get; set; }

    public ACPowerDto AcPower { get; set; } = new();
    public DeviceStatusDto Status { get; set; } = new();
}

// Ortak Power Yapıları
public class DCPowerDto
{

    public double? Voltage { get; set; }
    public double? Current { get; set; }
    public double? Power { get; set; }
    public double? TotalEnergyDaily { get; set; }

    public double? TotalEnergyLifetime { get; set; }
}

public class ACPowerDto
{
    public double? TotalActivePower { get; set; }
    public double? TotalReactivePower { get; set; }
    public double? TotalPowerFactor { get; set; }
    public double? TotalEnergyDaily { get; set; }
    public double? CurrentRms { get; set; }
    public double? VoltageRms { get; set; }

}

// Diğer Ortak Yapılar
public class DeviceStatusDto
{
    public OperatingMode OperatingMode { get; set; }
    public bool WarningStatus { get; set; }
    public List<string> WarningMessages { get; set; } = new();
    public bool FaultStatus { get; set; }
    public List<string> FaultMessages { get; set; } = new();
}

public enum BESSStatusDto { Standby, Charging, Discharging, Error }
public enum OperatingMode { Standby, Active, Fault }
