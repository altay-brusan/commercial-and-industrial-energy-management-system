using Backend.Application.TenantMarketOperators.Commands.CreateTenantMarketOperator;
using Backend.Application.TenantMarketOperators.Commands.UpdateTenantMarketOperator;
using Backend.Application.TenantMarketOperators.Commands.DeleteTenantMarketOperator;
using Backend.Application.TenantMarketOperators.Queries.GetTenantMarketOperator;
using Microsoft.AspNetCore.Http.HttpResults;

namespace Backend.Web.Endpoints;

public class TenantMarketOperators : EndpointGroupBase
{
    public override void Map(WebApplication app)
    {
        app.MapGroup(this)
            .RequireAuthorization()
            .MapGet(GetTenantMarketOperator, "Tenants/{tenantId}/MarketOperator")
            .MapPost(CreateTenantMarketOperator, "Tenants/{tenantId}/MarketOperator")
            .MapPut(UpdateTenantMarketOperator, "Tenants/{tenantId}/MarketOperator/{id}")
            .MapDelete(DeleteTenantMarketOperator, "Tenants/{tenantId}/MarketOperator/{id}");
    }

    /// <summary>
    /// Gets the Market Operator configuration for a tenant.
    /// </summary>
    public async Task<Ok<TenantMarketOperatorDto>> GetTenantMarketOperator(ISender sender, int tenantId)
    {
        var query = new GetTenantMarketOperatorQuery { TenantId = tenantId };
        var result = await sender.Send(query);
        return TypedResults.Ok(result);
    }

    /// <summary>
    /// Assigns a Market Operator to a tenant.
    /// </summary>
    public async Task<Created<int>> CreateTenantMarketOperator(ISender sender, int tenantId, CreateTenantMarketOperatorCommand command)
    {
        command.TenantId = tenantId;
        var id = await sender.Send(command);
        return TypedResults.Created($"/Tenants/{tenantId}/MarketOperator/{id}", id);
    }

    /// <summary>
    /// Updates the Market Operator configuration for a tenant.
    /// </summary>
    public async Task<Results<NoContent, BadRequest>> UpdateTenantMarketOperator(ISender sender, int tenantId, int id, UpdateTenantMarketOperatorCommand command)
    {
        if (id != command.Id) return TypedResults.BadRequest();
        await sender.Send(command);
        return TypedResults.NoContent();
    }

    /// <summary>
    /// Removes the Market Operator assignment from a tenant.
    /// </summary>
    public async Task<NoContent> DeleteTenantMarketOperator(ISender sender, int tenantId, int id)
    {
        await sender.Send(new DeleteTenantMarketOperatorCommand(id));
        return TypedResults.NoContent();
    }
}
