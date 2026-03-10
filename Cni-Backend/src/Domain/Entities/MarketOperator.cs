namespace Backend.Domain.Entities;

/// <summary>
/// Represents a Market Operator (MO) - an energy price provider.
/// Global entity shared across all tenants.
/// Examples: ENTSO-E, EPIAS
/// </summary>
public class MarketOperator : BaseAuditableEntity
{
    /// <summary>
    /// Display name of the Market Operator (e.g., "ENTSO-E", "EPIAS")
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Unique code identifier (e.g., "ENTSOE", "EPIAS")
    /// </summary>
    public string Code { get; set; } = string.Empty;

    /// <summary>
    /// Base URL for the Market Operator's API
    /// </summary>
    public string BaseUrl { get; set; } = string.Empty;

    /// <summary>
    /// API key or authentication token for the Market Operator
    /// </summary>
    public string? ApiKey { get; set; }

    /// <summary>
    /// Supported time resolutions in minutes (e.g., [15, 60] for ENTSO-E, [60] for EPIAS)
    /// Stored as JSON array in database
    /// </summary>
    public List<int> SupportedResolutions { get; set; } = new() { 60 };

    /// <summary>
    /// Available area/bidding zone codes (e.g., ["10YNL----------L", "10YDE-VE-------2"] for ENTSO-E, ["TR"] for EPIAS)
    /// Stored as JSON array in database
    /// </summary>
    public List<string> AreaCodes { get; set; } = new();

    /// <summary>
    /// The energy unit that prices are provided in (e.g., "MWh", "kWh")
    /// </summary>
    public string PriceUnit { get; set; } = "MWh";

    /// <summary>
    /// Whether this Market Operator is currently active and available for use
    /// </summary>
    public bool IsActive { get; set; } = true;

    // Navigation properties
    public ICollection<DayAheadNetEnergyPriceHistory> DayAheadPriceHistories { get; set; } = new List<DayAheadNetEnergyPriceHistory>();
    public ICollection<FirmMarketOperator> FirmMarketOperators { get; set; } = new List<FirmMarketOperator>();
}
