using Backend.Application.Common.Models;
using Backend.Application.Devices.Commands;
using Backend.Application.Devices.Queries.GetDevice;
using Backend.Application.Devices.Queries.GetDeviceList;
using Backend.Application.Devices.Queries;
using Microsoft.AspNetCore.Http.HttpResults;
using Backend.Application.Scada.Queries.GetScadaData;
using Backend.Domain.Entities.Proto.DTO;
using Backend.Application.Scada.Commands.ChangeTime;
using Backend.Application.Scada.Queries.GetScadaDataMinimal;

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
