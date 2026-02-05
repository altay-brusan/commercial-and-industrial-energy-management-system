namespace Backend.Application.Common;

/// <summary>
/// Default tariff values used when no country-specific tariff is configured.
/// Based on EU average values for distribution, transmission, and tax rates.
/// </summary>
public static class TariffDefaults
{
    /// <summary>
    /// Default distribution fee in €/MWh (local grid distribution costs).
    /// EU average typically ranges from 10-40 €/MWh.
    /// </summary>
    public const decimal DistributionFee = 20m;

    /// <summary>
    /// Default transmission fee in €/MWh (high-voltage transmission costs).
    /// EU average typically ranges from 3-8 €/MWh.
    /// </summary>
    public const decimal TransmissionFee = 5m;

    /// <summary>
    /// Default tax rate as decimal (VAT + electricity-specific taxes).
    /// EU average typically ranges from 18-25%.
    /// 0.21 = 21%
    /// </summary>
    public const decimal TaxRate = 0.21m;
}
