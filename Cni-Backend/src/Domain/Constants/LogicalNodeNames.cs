namespace Backend.Domain.Constants;

public abstract class LogicalNodeNames
{
    // BESS
    public const string BatteryNameplateData = "BatteryNameplateData";
    public const string BatteryStatusData = "BatteryStatusData";
    public const string BatteryEnergyData = "BatteryEnergyData";
    public const string BatteryHealthData = "BatteryHealthData";
    public const string BatteryPackEnergyData = "BatteryPackEnergyData";
    public const string BatteryModuleEnergyData = "BatteryModuleEnergyData";
    public const string HvacStatusData = "HvacStatusData";

    // PCS
    public const string PcsStatusData = "PcsStatusData";
    public const string PcsAcMeasurementData = "PcsAcMeasurementData";
    public const string PcsDcMeasurementData = "PcsDcMeasurementData";
    public const string PcsChargeAlgorithmData = "PcsChargeAlgorithmData";

    // Solar
    public const string SolarStatusData = "SolarStatusData";
    public const string SolarInverterData = "SolarInverterData";

    // Grid
    public const string GridMeterPowerData = "GridMeterPowerData";
    public const string GridBreakerLifecycleData = "GridBreakerLifecycleData";

    // Wind
    public const string WindPowerData = "WindPowerData";
    public const string WindConditionData = "WindConditionData";
    public const string WindElectricalData = "WindElectricalData";

    // Common
    public const string DeviceMaintenanceData = "DeviceMaintenanceData";
    public const string DeviceNameplateData = "DeviceNameplateData";
}
