using Backend.Domain.Enums;

namespace Backend.Application.Common.Services;

public static class DeviceDataCategorizer
{
    private static readonly (string Prefix, DeviceDataCategory Category)[] PrefixMappings =
    {
        // Legacy Kind prefixes (pre-standardization) — keep for backward compatibility
        ("ContainerBattery", DeviceDataCategory.Bess),
        ("Chiller", DeviceDataCategory.Bess),
        ("HVCU", DeviceDataCategory.Bess),
        ("Bess", DeviceDataCategory.Bess),
        ("Rack", DeviceDataCategory.Bess),
        ("Pack", DeviceDataCategory.Bess),
        ("Module", DeviceDataCategory.Bess),
        ("PCS", DeviceDataCategory.Pcs),
        ("SmartMeter", DeviceDataCategory.SmartMeter),
        ("Irradiance", DeviceDataCategory.Solar),

        // Standard package name prefixes (Battery.Energy, Pcs.Status, etc.)
        ("Battery", DeviceDataCategory.Bess),
        ("Pcs", DeviceDataCategory.Pcs),
        ("Solar", DeviceDataCategory.Solar),
        ("Wind", DeviceDataCategory.Wind),
        ("Grid", DeviceDataCategory.SmartMeter),
        ("Meter", DeviceDataCategory.SmartMeter),
        ("Device", DeviceDataCategory.Unknown), // Device.RuntimeInfo, Device.Maintenance — handled specially
    };

    /// <summary>
    /// Returns true if the kind represents an alarm package (e.g., Battery.Alarm, Pcs.Alarm).
    /// Alarm packages are processed separately from telemetry.
    /// </summary>
    public static bool IsAlarmPackage(string? kind)
    {
        if (string.IsNullOrWhiteSpace(kind))
            return false;

        return kind.EndsWith(".Alarm", StringComparison.OrdinalIgnoreCase);
    }

    public static DeviceDataCategory Categorize(string? kind)
    {
        if (string.IsNullOrWhiteSpace(kind))
            return DeviceDataCategory.Unknown;

        foreach (var (prefix, category) in PrefixMappings)
        {
            if (kind.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
                return category;
        }

        return DeviceDataCategory.Unknown;
    }
}
