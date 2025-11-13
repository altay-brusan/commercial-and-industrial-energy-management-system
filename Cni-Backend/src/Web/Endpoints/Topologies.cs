using Backend.Application.Topologies.Queries.GetTopology;
using Microsoft.AspNetCore.Http.HttpResults;

namespace Backend.Web.Endpoints;

public class Topologies : EndpointGroupBase
{
    public override void Map(WebApplication app)
    {
        app.MapGroup(this)
            .RequireAuthorization()
            .MapGet(GetTopology, "{facilityId}");
    }
    public async Task<Ok<TopologyDto>> GetTopology(ISender sender, [AsParameters] GetTopologyQuery query)
    {
        var result = await sender.Send(query);

        return TypedResults.Ok(result);
    }

    
}
