using Backend.Application.Common.Models;
using Backend.Application.Devices.Commands;
using Backend.Application.Devices.Queries.GetDevice;
using Backend.Application.Devices.Queries.GetDeviceList;
using Backend.Application.Devices.Queries;
using Microsoft.AspNetCore.Http.HttpResults;
using Backend.Application.Graphs.Queries.GetSolarEnergyProduction;
using Backend.Application.Graphs.Queries;
using Backend.Application.Graphs.Queries.GetSolarPowerProduction;
using Backend.Application.Graphs.Queries.GetInverterGraph;
using Backend.Application.Graphs.Queries.GetPowerGeneralGraph;

namespace Backend.Web.Endpoints;


public class Graphs : EndpointGroupBase
{
    public override void Map(WebApplication app)
    {
        app.MapGroup(this)
            .RequireAuthorization()
            .MapGet(GetSolarEnergyProduction, "/EnergyProduction/{id}")
            .MapGet(GetSolarPowerGeneration, "/PowerProduction/{id}")
            .MapGet(GetInverterGraphData, "/InverterGraphData/{id}")
            .MapGet(GetPowerGeneralGraphData, "/PowerGeneralGraphData/{id}");
    }
    public async Task<Ok<SolarGraphDto>> GetSolarEnergyProduction(ISender sender, [AsParameters] GetSolarEnergyProductionQuery query)
    {
        var result = await sender.Send(query);

        return TypedResults.Ok(result);
    }
    public async Task<Ok<SolarGraphDto>> GetSolarPowerGeneration(ISender sender, [AsParameters] GetSolarPowerProductionQuery query)
    {
        var result = await sender.Send(query);

        return TypedResults.Ok(result);
    } 
    public async Task<Ok<List<PowerGeneralGraphDto>>> GetPowerGeneralGraphData(ISender sender, [AsParameters] GetPowerGeneralGraphQuery query)
    {
        var result = await sender.Send(query);

        return TypedResults.Ok(result);
    }
    public async Task<Ok<List<InverterGraphDto>>> GetInverterGraphData(ISender sender, [AsParameters] GetInverterGraphQuery query)
    {
        var result = await sender.Send(query);

        return TypedResults.Ok(result);
    }

}
