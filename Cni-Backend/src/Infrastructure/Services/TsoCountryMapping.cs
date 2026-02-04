namespace Backend.Infrastructure.Services;

/// <summary>
/// Maps country names to TSO-specific area codes and timezones.
/// Each TSO may use different coding schemes.
/// </summary>
public static class TsoCountryMapping
{
    /// <summary>
    /// ENTSO-E EIC (Energy Identification Code) area codes by country.
    /// Reference: https://www.entsoe.eu/data/energy-identification-codes-eic/
    /// </summary>
    private static readonly Dictionary<string, string> EntsoeAreaCodes = new(StringComparer.OrdinalIgnoreCase)
    {
        // Western Europe
        { "Netherlands", "10YNL----------L" },
        { "Germany", "10YDE-VE-------2" },
        { "France", "10YFR-RTE------C" },
        { "Belgium", "10YBE----------2" },
        { "Austria", "10YAT-APG------L" },
        { "Switzerland", "10YCH-SWISSGRIDZ" },
        { "Luxembourg", "10YLU-CEGEDEL-NQ" },

        // Southern Europe
        { "Spain", "10YES-REE------0" },
        { "Portugal", "10YPT-REN------W" },
        { "Italy", "10YIT-GRTN-----B" },
        { "Greece", "10YGR-HTSO-----Y" },

        // Northern Europe
        { "Denmark", "10Y1001A1001A65H" },
        { "Sweden", "10YSE-1--------K" },
        { "Norway", "10YNO-0--------C" },
        { "Finland", "10YFI-1--------U" },

        // Central/Eastern Europe
        { "Poland", "10YPL-AREA-----S" },
        { "Czech Republic", "10YCZ-CEPS-----N" },
        { "Czechia", "10YCZ-CEPS-----N" },
        { "Slovakia", "10YSK-SEPS-----K" },
        { "Hungary", "10YHU-MAVIR----U" },
        { "Romania", "10YRO-TEL------P" },
        { "Bulgaria", "10YCA-BULGARIA-R" },
        { "Slovenia", "10YSI-ELES-----O" },
        { "Croatia", "10YHR-HEP------M" },
        { "Serbia", "10YCS-SERBIATSOV" },

        // Baltic States
        { "Estonia", "10Y1001A1001A39I" },
        { "Latvia", "10YLV-1001A00074" },
        { "Lithuania", "10YLT-1001A0008Q" },

        // United Kingdom & Ireland
        { "United Kingdom", "10YGB----------A" },
        { "UK", "10YGB----------A" },
        { "Ireland", "10YIE-1001A00010" },
        { "Northern Ireland", "10Y1001A1001A016" },

        // Turkey (connected to ENTSO-E but limited data)
        { "Türkiye", "10YTR-TEIAS----W" },
        { "Turkey", "10YTR-TEIAS----W" },
    };

    /// <summary>
    /// EPIAS does not use area codes - Turkey is the only supported country.
    /// </summary>
    private static readonly HashSet<string> EpiasSupportedCountries = new(StringComparer.OrdinalIgnoreCase)
    {
        "Türkiye",
        "Turkey"
    };

    /// <summary>
    /// IANA timezone IDs by country for proper local time conversion.
    /// </summary>
    private static readonly Dictionary<string, string> CountryTimezones = new(StringComparer.OrdinalIgnoreCase)
    {
        // Western Europe
        { "Netherlands", "Europe/Amsterdam" },
        { "Germany", "Europe/Berlin" },
        { "France", "Europe/Paris" },
        { "Belgium", "Europe/Brussels" },
        { "Austria", "Europe/Vienna" },
        { "Switzerland", "Europe/Zurich" },
        { "Luxembourg", "Europe/Luxembourg" },

        // Southern Europe
        { "Spain", "Europe/Madrid" },
        { "Portugal", "Europe/Lisbon" },
        { "Italy", "Europe/Rome" },
        { "Greece", "Europe/Athens" },

        // Northern Europe
        { "Denmark", "Europe/Copenhagen" },
        { "Sweden", "Europe/Stockholm" },
        { "Norway", "Europe/Oslo" },
        { "Finland", "Europe/Helsinki" },

        // Central/Eastern Europe
        { "Poland", "Europe/Warsaw" },
        { "Czech Republic", "Europe/Prague" },
        { "Czechia", "Europe/Prague" },
        { "Slovakia", "Europe/Bratislava" },
        { "Hungary", "Europe/Budapest" },
        { "Romania", "Europe/Bucharest" },
        { "Bulgaria", "Europe/Sofia" },
        { "Slovenia", "Europe/Ljubljana" },
        { "Croatia", "Europe/Zagreb" },
        { "Serbia", "Europe/Belgrade" },

        // Baltic States
        { "Estonia", "Europe/Tallinn" },
        { "Latvia", "Europe/Riga" },
        { "Lithuania", "Europe/Vilnius" },

        // United Kingdom & Ireland
        { "United Kingdom", "Europe/London" },
        { "UK", "Europe/London" },
        { "Ireland", "Europe/Dublin" },
        { "Northern Ireland", "Europe/London" },

        // Turkey
        { "Türkiye", "Europe/Istanbul" },
        { "Turkey", "Europe/Istanbul" },
    };

    /// <summary>
    /// Gets the TSO-specific area code for a country.
    /// </summary>
    /// <param name="tsoCode">The TSO code (e.g., "ENTSOE", "EPIAS")</param>
    /// <param name="country">The country name</param>
    /// <returns>The area code if found, null otherwise</returns>
    public static string? GetAreaCode(string tsoCode, string country)
    {
        if (string.IsNullOrWhiteSpace(country))
            return null;

        return tsoCode.ToUpperInvariant() switch
        {
            "ENTSOE" => EntsoeAreaCodes.TryGetValue(country, out var code) ? code : null,
            "EPIAS" => EpiasSupportedCountries.Contains(country) ? null : null, // EPIAS doesn't use area codes
            _ => null
        };
    }

    /// <summary>
    /// Checks if a country is supported by a TSO.
    /// </summary>
    public static bool IsCountrySupported(string tsoCode, string country)
    {
        if (string.IsNullOrWhiteSpace(country))
            return false;

        return tsoCode.ToUpperInvariant() switch
        {
            "ENTSOE" => EntsoeAreaCodes.ContainsKey(country),
            "EPIAS" => EpiasSupportedCountries.Contains(country),
            _ => false
        };
    }

    /// <summary>
    /// Gets the list of supported countries for a TSO.
    /// </summary>
    public static IReadOnlyList<string> GetSupportedCountries(string tsoCode)
    {
        return tsoCode.ToUpperInvariant() switch
        {
            "ENTSOE" => EntsoeAreaCodes.Keys.ToList(),
            "EPIAS" => EpiasSupportedCountries.ToList(),
            _ => Array.Empty<string>()
        };
    }

    /// <summary>
    /// Gets the appropriate error message when a country is not supported.
    /// </summary>
    public static string GetUnsupportedCountryMessage(string tsoCode, string country)
    {
        var supportedCountries = GetSupportedCountries(tsoCode);
        var countriesList = string.Join(", ", supportedCountries.Take(10));

        if (supportedCountries.Count > 10)
            countriesList += $", ... and {supportedCountries.Count - 10} more";

        return $"Country '{country}' is not supported by {tsoCode}. Supported countries: {countriesList}";
    }

    /// <summary>
    /// Gets the country name from an ENTSO-E area code.
    /// </summary>
    /// <param name="areaCode">The ENTSO-E EIC area code</param>
    /// <returns>The country name if found, null otherwise</returns>
    public static string? GetCountryNameFromAreaCode(string? areaCode)
    {
        if (string.IsNullOrWhiteSpace(areaCode))
            return null;

        // Special case for EPIAS (Turkey)
        if (areaCode.Equals("TR", StringComparison.OrdinalIgnoreCase))
            return "Turkey";

        // Reverse lookup in ENTSO-E area codes
        var entry = EntsoeAreaCodes.FirstOrDefault(kvp =>
            kvp.Value.Equals(areaCode, StringComparison.OrdinalIgnoreCase));

        return entry.Key; // Returns null if not found
    }

    /// <summary>
    /// Gets the IANA timezone ID for a country.
    /// </summary>
    /// <param name="country">The country name</param>
    /// <returns>The IANA timezone ID if found, null otherwise</returns>
    public static string? GetTimezone(string? country)
    {
        if (string.IsNullOrWhiteSpace(country))
            return null;

        return CountryTimezones.TryGetValue(country, out var timezone) ? timezone : null;
    }

    /// <summary>
    /// Converts a UTC time to the local time for a given country.
    /// </summary>
    /// <param name="utcTime">The UTC time to convert</param>
    /// <param name="country">The country name</param>
    /// <returns>The time converted to the country's local timezone, or the original time if country not found</returns>
    public static DateTimeOffset ConvertToLocalTime(DateTimeOffset utcTime, string? country)
    {
        var timezoneId = GetTimezone(country);
        if (timezoneId == null)
            return utcTime;

        try
        {
            var timeZoneInfo = TimeZoneInfo.FindSystemTimeZoneById(timezoneId);
            return TimeZoneInfo.ConvertTime(utcTime, timeZoneInfo);
        }
        catch (TimeZoneNotFoundException)
        {
            // Fallback: try Windows timezone ID mapping
            try
            {
                var windowsId = TryGetWindowsTimezoneId(timezoneId);
                if (windowsId != null)
                {
                    var timeZoneInfo = TimeZoneInfo.FindSystemTimeZoneById(windowsId);
                    return TimeZoneInfo.ConvertTime(utcTime, timeZoneInfo);
                }
            }
            catch
            {
                // Ignore and return original time
            }
            return utcTime;
        }
    }

    /// <summary>
    /// Maps IANA timezone IDs to Windows timezone IDs for cross-platform compatibility.
    /// </summary>
    private static string? TryGetWindowsTimezoneId(string ianaId)
    {
        // Common IANA to Windows timezone mappings
        return ianaId switch
        {
            "Europe/Amsterdam" => "W. Europe Standard Time",
            "Europe/Berlin" => "W. Europe Standard Time",
            "Europe/Paris" => "Romance Standard Time",
            "Europe/Brussels" => "Romance Standard Time",
            "Europe/Vienna" => "W. Europe Standard Time",
            "Europe/Zurich" => "W. Europe Standard Time",
            "Europe/Luxembourg" => "W. Europe Standard Time",
            "Europe/Madrid" => "Romance Standard Time",
            "Europe/Lisbon" => "GMT Standard Time",
            "Europe/Rome" => "W. Europe Standard Time",
            "Europe/Athens" => "GTB Standard Time",
            "Europe/Copenhagen" => "Romance Standard Time",
            "Europe/Stockholm" => "W. Europe Standard Time",
            "Europe/Oslo" => "W. Europe Standard Time",
            "Europe/Helsinki" => "FLE Standard Time",
            "Europe/Warsaw" => "Central European Standard Time",
            "Europe/Prague" => "Central European Standard Time",
            "Europe/Bratislava" => "Central European Standard Time",
            "Europe/Budapest" => "Central European Standard Time",
            "Europe/Bucharest" => "GTB Standard Time",
            "Europe/Sofia" => "FLE Standard Time",
            "Europe/Ljubljana" => "Central European Standard Time",
            "Europe/Zagreb" => "Central European Standard Time",
            "Europe/Belgrade" => "Central European Standard Time",
            "Europe/Tallinn" => "FLE Standard Time",
            "Europe/Riga" => "FLE Standard Time",
            "Europe/Vilnius" => "FLE Standard Time",
            "Europe/London" => "GMT Standard Time",
            "Europe/Dublin" => "GMT Standard Time",
            "Europe/Istanbul" => "Turkey Standard Time",
            _ => null
        };
    }
}
