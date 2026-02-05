using Backend.Application.Scada.DTOs;
using Backend.Application.Scada.Queries.GetScadaData;
using Backend.Application.Scada.Commands.ChangeTime;
using Backend.Application.Scada.Queries.GetScadaDataMinimal;
using Microsoft.AspNetCore.Http.HttpResults;

namespace Backend.Web.Endpoints;

public class Scada : EndpointGroupBase
{
    public override void Map(WebApplication app)
    {
        app.MapGroup(this)
            .MapGet(GetScadaData, "{id}")
            .MapGet(GetScadaMinimalData, "GetMinimalScada")
            .MapPost(ChangeTime,"ChangeTime");
    }
    public async Task<Ok<FacilityHubDto>> GetScadaData(ISender sender, [AsParameters] GetScadaQuery query)
    {
        var result = await sender.Send(query);

        return TypedResults.Ok(result);
    }  
    public async Task<Ok<ScadaDataMinimalDto>> GetScadaMinimalData(ISender sender, [AsParameters] GetScadaDataMinimalQuery query)
    {
        var result = await sender.Send(query);

        return TypedResults.Ok(result);
    }
    public async Task<Ok<bool>> ChangeTime(ISender sender, ChangeTimeCommand command)
    {
        var result = await sender.Send(command);

        return TypedResults.Ok(result);
    }



}
