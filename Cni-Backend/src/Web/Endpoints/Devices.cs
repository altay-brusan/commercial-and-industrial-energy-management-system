
using Backend.Application.Devices.Queries.GetDevice;
using Microsoft.AspNetCore.Http.HttpResults;
using Backend.Application.Devices.Queries;
using Backend.Application.Common.Models;
using Backend.Application.Devices.Queries.GetDeviceList;
using Backend.Application.Devices.Queries.GetDeviceTree;
using Backend.Application.Devices.Commands;
using Backend.Application.Devices.Commands.RegisterDeviceComposite;


namespace Backend.Web.Endpoints;
public class Devices : EndpointGroupBase
{
    public override void Map(WebApplication app)
    {
        app.MapGroup(this)
            .RequireAuthorization()
            .MapGet(GetDevice, "{id}")
            .MapGet(GetDevicesWithPagination)
            .MapGet(GetDeviceTree, "{id}/Tree")
            .MapPost(CreateDevice)
            .MapPost(RegisterComposite, "RegisterComposite");
    }
    public async Task<Ok<DeviceDto>> GetDevice(ISender sender, [AsParameters] GetDeviceQuery query)
    {
        var result = await sender.Send(query);

        return TypedResults.Ok(result);
    }
    public async Task<Ok<PaginatedList<DeviceDto>>> GetDevicesWithPagination(ISender sender, [AsParameters] GetDevicesWithPaginationQuery query)
    {
        var result = await sender.Send(query);

        return TypedResults.Ok(result);
    }

    public async Task<Ok<DeviceTreeNodeDto>> GetDeviceTree(ISender sender, Guid id)
    {
        var result = await sender.Send(new GetDeviceTreeQuery { DeviceId = id });
        return TypedResults.Ok(result);
    }

    public async Task<Created<Guid>> CreateDevice(ISender sender, CreateDeviceCommand command)
    {
        var id = await sender.Send(command);

        return TypedResults.Created($"/{nameof(Devices)}/{id}", id);
    }

    public async Task<Created<Guid>> RegisterComposite(ISender sender, RegisterDeviceCompositeCommand command)
    {
        var id = await sender.Send(command);
        return TypedResults.Created($"/{nameof(Devices)}/{id}", id);
    }
}
