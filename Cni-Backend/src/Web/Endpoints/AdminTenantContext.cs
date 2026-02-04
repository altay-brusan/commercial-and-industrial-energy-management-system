using Backend.Application.AdminTenantContext.Queries.GetAvailableTenants;
using Backend.Application.AdminTenantContext.Queries.GetTenantContext;
using Microsoft.AspNetCore.Http.HttpResults;

namespace Backend.Web.Endpoints;

public class AdminTenantContext : EndpointGroupBase
{
    public override void Map(WebApplication app)
    {
        app.MapGroup(this)
            .RequireAuthorization("Administrator")
            .MapGet(GetCurrentContext)
            .MapGet(GetAvailableTenants, "tenants");
    }

    /// <summary>
    /// Gets the current tenant context information.
    /// Useful for debugging and understanding the current admin state.
    /// </summary>
    public async Task<Ok<TenantContextDto>> GetCurrentContext(ISender sender)
    {
        var result = await sender.Send(new GetTenantContextQuery());
        return TypedResults.Ok(result);
    }

    /// <summary>
    /// Gets a list of all available tenants for the admin tenant selector.
    /// Returns tenant ID, name, and identifier for display in a dropdown.
    /// </summary>
    public async Task<Ok<List<TenantSelectorDto>>> GetAvailableTenants(ISender sender)
    {
        var result = await sender.Send(new GetAvailableTenantsQuery());
        return TypedResults.Ok(result);
    }
}
