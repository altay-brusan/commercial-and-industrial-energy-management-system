namespace Backend.Domain.Entities.DeviceData;

/// <summary>
/// Solar panel/array telemetry data.
/// Payload JSON typically contains: PanelPower, DCOutput, ACOutput,
/// PerformanceRatio, PvPanelTemperature, SolarEnergyProduced, etc.
/// </summary>
public class DeviceSolarData : DeviceDataBase
{
}
