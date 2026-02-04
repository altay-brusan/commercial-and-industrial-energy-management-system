namespace Backend.Application.Common.Interfaces;

/// <summary>
/// Service interface for mapping area codes to countries and timezones.
/// </summary>
public interface ICountryMappingService
{
    /// <summary>
    /// Gets the country name from an area code.
    /// </summary>
    /// <param name="areaCode">The area code (e.g., "10YNL----------L" or "TR")</param>
    /// <returns>The country name if found, null otherwise</returns>
    string? GetCountryNameFromAreaCode(string? areaCode);

    /// <summary>
    /// Gets the TSO-specific area code for a country.
    /// </summary>
    /// <param name="tsoCode">The TSO code (e.g., "ENTSOE", "EPIAS")</param>
    /// <param name="country">The country name</param>
    /// <returns>The area code if found, null otherwise</returns>
    string? GetAreaCode(string tsoCode, string country);

    /// <summary>
    /// Checks if a country is supported by a TSO.
    /// </summary>
    bool IsCountrySupported(string tsoCode, string country);

    /// <summary>
    /// Gets the IANA timezone ID for a country.
    /// </summary>
    /// <param name="country">The country name</param>
    /// <returns>The IANA timezone ID if found, null otherwise</returns>
    string? GetTimezone(string? country);
}
