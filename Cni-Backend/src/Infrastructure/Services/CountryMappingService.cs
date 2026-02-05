using Backend.Application.Common.Interfaces;

namespace Backend.Infrastructure.Services;

/// <summary>
/// Service implementation for mapping area codes to countries and timezones.
/// Wraps the static AreaCodeMapping class for dependency injection.
/// </summary>
public class CountryMappingService : ICountryMappingService
{
    public string? GetCountryNameFromAreaCode(string? areaCode)
    {
        return AreaCodeMapping.GetCountryNameFromAreaCode(areaCode);
    }

    public string? GetAreaCode(string moCode, string country)
    {
        return AreaCodeMapping.GetAreaCode(moCode, country);
    }

    public bool IsCountrySupported(string moCode, string country)
    {
        return AreaCodeMapping.IsCountrySupported(moCode, country);
    }

    public string? GetTimezone(string? country)
    {
        return AreaCodeMapping.GetTimezone(country);
    }
}
