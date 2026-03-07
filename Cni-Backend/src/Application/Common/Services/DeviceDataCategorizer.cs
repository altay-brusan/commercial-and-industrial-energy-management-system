using Backend.Domain.Enums;

namespace Backend.Application.Common.Services;

public static class DeviceDataCategorizer
{
    private static readonly (string Prefix, DeviceDataCategory Category)[] PrefixMappings =
    {
        // Legacy Kind prefixes (pre-standardization)
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
        ("Pv", DeviceDataCategory.Solar),
        ("PV", DeviceDataCategory.Solar),
        ("Wind", DeviceDataCategory.Wind),
        ("Grid", DeviceDataCategory.SmartMeter),
        ("Meter", DeviceDataCategory.SmartMeter),
    };

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
