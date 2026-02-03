namespace Backend.Domain.Entities;

/// <summary>
/// Represents a Transmission System Operator (TSO) - an energy price provider.
/// Examples: ENTSO-E, EPIAS
/// </summary>
public class TransmissionSystemOperator : BaseAuditableEntity, IMultiTenantEntity
{
    public int TenantId { get; set; }

    /// <summary>
    /// Display name of the TSO (e.g., "ENTSO-E", "EPIAS")
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Unique code identifier (e.g., "ENTSOE", "EPIAS")
    /// </summary>
    public string Code { get; set; } = string.Empty;

    /// <summary>
    /// Base URL for the TSO's API
    /// </summary>
    public string BaseUrl { get; set; } = string.Empty;

    /// <summary>
    /// API key or authentication token for the TSO
    /// </summary>
    public string? ApiKey { get; set; }

    /// <summary>
    /// Time resolution of the TSO's data in minutes (e.g., 15, 60)
    /// </summary>
    public int TimeResolutionMinutes { get; set; } = 60;

    /// <summary>
    /// Currency code for prices (e.g., "EUR", "TRY")
    /// </summary>
    public string Currency { get; set; } = "EUR";

    /// <summary>
    /// Whether this TSO is currently active and available for use
    /// </summary>
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// Area/bidding zone code for the TSO (e.g., "10YNL----------L" for Netherlands)
    /// </summary>
    public string? AreaCode { get; set; }

    public Tenant? Tenant { get; set; }

    public ICollection<EnergyPriceHistory> PriceHistories { get; set; } = new List<EnergyPriceHistory>();
}
