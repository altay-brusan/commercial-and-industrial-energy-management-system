using Backend.Application.Permissions.Queries.GetAllPermissions;
using Backend.Application.Permissions.Queries.GetPermissionsByFeature;
using Microsoft.AspNetCore.Http.HttpResults;

namespace Backend.Web.Endpoints;

public class Permissions : EndpointGroupBase
{
    public override void Map(WebApplication app)
    {
        app.MapGroup(this)
            .RequireAuthorization()
            .MapGet(GetAllPermissions)
            .MapGet(GetPermissionsByFeature, "by-feature/{featureKey}");
    }

    public async Task<Ok<List<FeaturePermissionsDto>>> GetAllPermissions(ISender sender)
    {
        var result = await sender.Send(new GetAllPermissionsQuery());
        return TypedResults.Ok(result);
    }

    public async Task<Ok<List<PermissionDto>>> GetPermissionsByFeature(ISender sender, string featureKey)
    {
        var result = await sender.Send(new GetPermissionsByFeatureQuery(featureKey));
        return TypedResults.Ok(result);
    }
}
