using Backend.Domain.Enums;

namespace Backend.Application.Common.Services;

public static class DeviceDataCategorizer
{
    private static readonly (string Prefix, DeviceDataCategory Category)[] PrefixMappings =
    {
        ("ContainerBattery", DeviceDataCategory.Bess),
        ("Chiller", DeviceDataCategory.Bess),
        ("HVCU", DeviceDataCategory.Bess),
        ("Battery", DeviceDataCategory.Bess),
        ("Bess", DeviceDataCategory.Bess),
        ("Rack", DeviceDataCategory.Bess),
        ("Pack", DeviceDataCategory.Bess),
        ("Module", DeviceDataCategory.Bess),
        ("Pcs", DeviceDataCategory.Pcs),
        ("PCS", DeviceDataCategory.Pcs),
        ("Solar", DeviceDataCategory.Solar),
        ("Pv", DeviceDataCategory.Solar),
        ("PV", DeviceDataCategory.Solar),
        ("Irradiance", DeviceDataCategory.Solar),
        ("Meter", DeviceDataCategory.SmartMeter),
        ("Grid", DeviceDataCategory.SmartMeter),
        ("SmartMeter", DeviceDataCategory.SmartMeter),
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
