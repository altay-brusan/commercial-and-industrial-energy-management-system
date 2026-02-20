namespace Backend.Domain.Entities.DeviceData;

/// <summary>
/// Solar / MPPT (Maximum Power Point Tracking) telemetry data.
/// Payload JSON typically contains: PanelPower, DCOutput, ACOutput,
/// PerformanceRatio, PvPanelTemperature, SolarEnergyProduced, etc.
/// </summary>
public class DeviceSolarData : DeviceDataBase
{
}
