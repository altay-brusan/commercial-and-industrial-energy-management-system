namespace Backend.Domain.Entities;

/// <summary>
/// Junction entity linking a Firm to a Market Operator with firm-specific configuration.
/// Allows firms to select which Market Operator to use and configure their specific area code and sampling period.
/// </summary>
public class FirmMarketOperator : BaseAuditableEntity
{
    /// <summary>
    /// The Firm this Market Operator configuration belongs to
    /// </summary>
    public Guid FirmId { get; set; }

    /// <summary>
    /// The Market Operator being configured
    /// </summary>
    public Guid MarketOperatorId { get; set; }

    /// <summary>
    /// The area/bidding zone code selected by this firm.
    /// Must be one of the codes in MarketOperator.AreaCodes.
    /// </summary>
    public string AreaCode { get; set; } = string.Empty;

    /// <summary>
    /// The sampling period in minutes for this firm.
    /// Must be one of the values in MarketOperator.SupportedResolutions.
    /// </summary>
    public int SamplingPeriodMinutes { get; set; } = 60;

    /// <summary>
    /// Whether this Market Operator configuration is active for the firm
    /// </summary>
    public bool IsActive { get; set; } = true;

    // Navigation properties
    public Firm? Firm { get; set; }
    public MarketOperator? MarketOperator { get; set; }
}
