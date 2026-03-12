using Backend.Domain.Constants;
using Microsoft.AspNetCore.Http.HttpResults;

namespace Backend.Web.Endpoints;

public class Lookups : EndpointGroupBase
{
    public override void Map(WebApplication app)
    {
        var group = app.MapGroup("/api/v1/Lookups")
            .WithGroupName("Lookups")
            .WithTags("Lookups")
            .RequireAuthorization();

        group.MapGet("/Countries", GetCountries);
        group.MapGet("/Provinces/{country}", GetProvinces);
    }

    public static Ok<List<CountryInfo>> GetCountries()
    {
        return TypedResults.Ok(Countries.All.ToList());
    }

    public static Results<Ok<List<ProvinceInfo>>, Ok<List<object>>> GetProvinces(string country)
    {
        if (string.Equals(country, "Türkiye", StringComparison.OrdinalIgnoreCase))
        {
            return TypedResults.Ok(TurkeyProvinces.All.ToList());
        }

        // For non-Türkiye countries, return empty list (provinces are free-text)
        return TypedResults.Ok(new List<object>());
    }
}
