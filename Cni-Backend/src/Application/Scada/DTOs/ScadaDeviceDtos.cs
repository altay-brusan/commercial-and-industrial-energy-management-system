namespace Backend.Application.Scada.DTOs;

/// <summary>
/// Hub DTO containing all facility device data for real-time updates
/// </summary>
public class FacilityHubDto
{
    public List<SolarDeviceDto> SolarDevices { get; set; } = new();
    public List<BESSDeviceDto> BessDevices { get; set; } = new();
    public List<PCSDeviceDto> PcsDevices { get; set; } = new();
    public List<GridPowerDto> GridDevices { get; set; } = new();
    public List<GeneratorDeviceDto> GeneratorDevices { get; set; } = new();
    public FacilityPowerDto? FacilityData { get; set; }
    public SystemDataDto? SystemData { get; set; }
}

/// <summary>
/// Base class for device telemetry DTOs
/// </summary>
public abstract class DeviceDataDtoBase
{
    public Guid DeviceId { get; set; }
    public string? DeviceName { get; set; }
    public int? NodeId { get; set; } // Kept for backwards compatibility, always null
    public DateTimeOffset Timestamp { get; set; }
    public DeviceStatusDto? Status { get; set; }
}

/// <summary>
/// Solar panel/array telemetry DTO
/// </summary>
public class SolarDeviceDto : DeviceDataDtoBase
{
    public DCPowerDto? PanelPower { get; set; }
    public DCPowerDto? DCOutput { get; set; }
    public double? PerformanceRatio { get; set; }
    public double? PvPanelTemperature { get; set; }
    public double? SolarEnergyProduced { get; set; }
    public double? Weather { get; set; }
    public double? Irradiance { get; set; }
}

/// <summary>
/// Battery Energy Storage System telemetry DTO
/// </summary>
public class BESSDeviceDto : DeviceDataDtoBase
{
    public double? StateOfCharge { get; set; }
    public double? StateOfHealth { get; set; }
    public DCPowerDto? DcPower { get; set; }
    public double? BatteryTemperature { get; set; }
    public double? TotalEnergyStored { get; set; }
    public double? ChargeLimit { get; set; }
    public double? DischargeLimit { get; set; }
    public string? ChargeStatus { get; set; }
}

/// <summary>
/// Power Conversion System telemetry DTO
/// </summary>
public class PCSDeviceDto : DeviceDataDtoBase
{
    public DCPowerDto? DcPower { get; set; }
    public ACPowerDto? AcPower { get; set; }
    public double? TemperaturePcs { get; set; }
    public double? Efficiency { get; set; }
    public double? TotalEnergyDelivered { get; set; }
    public double? TotalEnergyAbsorbed { get; set; }
}

/// <summary>
/// Grid/Smart Meter telemetry DTO
/// </summary>
public class GridPowerDto : DeviceDataDtoBase
{
    public ACPowerDto? AcPower { get; set; }
    public double? ActiveEnergyImport { get; set; }
    public double? ActiveEnergyExport { get; set; }
}

/// <summary>
/// Facility-level power data DTO
/// </summary>
public class FacilityPowerDto : DeviceDataDtoBase
{
    public ACPowerDto? AcPower { get; set; }
    public double? TotalActiveEnergy { get; set; }
}

/// <summary>
/// Generator device DTO
/// </summary>
public class GeneratorDeviceDto : DeviceDataDtoBase
{
    public double? TotalPower { get; set; }
    public double? LoadGeneratorContribution { get; set; }
}

/// <summary>
/// System-wide aggregated data DTO
/// </summary>
public class SystemDataDto
{
    public double? GeneratorPower { get; set; }
    public double? GridPower { get; set; }
    public double? SolarPower { get; set; }
    public double? BessPower { get; set; }
    public double? LoadGeneratorContribution { get; set; }
    public double? LoadGridContribution { get; set; }
    public double? LoadPcsContribution { get; set; }
    public double? LoadSolarContribution { get; set; }
    public double? Cost_Saving_TL_Daily { get; set; }
    public double? Grid_Consumption_TL_Daily { get; set; }
}

/// <summary>
/// DC Power readings
/// </summary>
public class DCPowerDto
{
    public double? Voltage { get; set; }
    public double? Current { get; set; }
    public double? Power { get; set; }
    public double? TotalEnergyDaily { get; set; }
    public double? TotalEnergyLifetime { get; set; }
}

/// <summary>
/// AC Power readings
/// </summary>
public class ACPowerDto
{
    public double? TotalActivePower { get; set; }
    public double? TotalReactivePower { get; set; }
    public double? TotalPowerFactor { get; set; }
    public double? VoltageRms { get; set; }
    public double? CurrentRms { get; set; }
    public double? TotalEnergyDaily { get; set; }
    public double? Frequency { get; set; }
}

/// <summary>
/// Device operational status
/// </summary>
public class DeviceStatusDto
{
    public string? OperatingMode { get; set; }
    public bool WarningStatus { get; set; }
    public List<string> WarningMessages { get; set; } = new();
    public bool FaultStatus { get; set; }
    public List<string> FaultMessages { get; set; } = new();
}
