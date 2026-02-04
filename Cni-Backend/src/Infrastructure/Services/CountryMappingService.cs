using Backend.Application.Common.Interfaces;

namespace Backend.Infrastructure.Services;

/// <summary>
/// Service implementation for mapping area codes to countries and timezones.
/// Wraps the static TsoCountryMapping class for dependency injection.
/// </summary>
public class CountryMappingService : ICountryMappingService
{
    public string? GetCountryNameFromAreaCode(string? areaCode)
    {
        return TsoCountryMapping.GetCountryNameFromAreaCode(areaCode);
    }

    public string? GetAreaCode(string tsoCode, string country)
    {
        return TsoCountryMapping.GetAreaCode(tsoCode, country);
    }

    public bool IsCountrySupported(string tsoCode, string country)
    {
        return TsoCountryMapping.IsCountrySupported(tsoCode, country);
    }

    public string? GetTimezone(string? country)
    {
        return TsoCountryMapping.GetTimezone(country);
    }
}
