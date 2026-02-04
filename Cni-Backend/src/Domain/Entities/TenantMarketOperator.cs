namespace Backend.Domain.Entities;

/// <summary>
/// Junction entity linking a Tenant to a Market Operator with tenant-specific configuration.
/// Allows tenants to select which Market Operator to use and configure their specific area code and sampling frequency.
/// </summary>
public class TenantMarketOperator : BaseAuditableEntity, IMultiTenantEntity
{
    public int TenantId { get; set; }

    public int MarketOperatorId { get; set; }

    /// <summary>
    /// The area/bidding zone code selected by this tenant.
    /// Must be one of the codes in MarketOperator.AreaCodes.
    /// </summary>
    public string AreaCode { get; set; } = string.Empty;

    /// <summary>
    /// The sampling frequency in minutes for this tenant.
    /// Must be one of the values in MarketOperator.SupportedResolutions.
    /// </summary>
    public int SamplingFrequencyMinutes { get; set; } = 60;

    /// <summary>
    /// Whether this Market Operator configuration is active for the tenant
    /// </summary>
    public bool IsActive { get; set; } = true;

    // Navigation properties
    public Tenant? Tenant { get; set; }
    public MarketOperator? MarketOperator { get; set; }
}
