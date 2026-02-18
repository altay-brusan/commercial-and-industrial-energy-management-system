
using Backend.Application.Devices.Queries.GetDevice;
using Microsoft.AspNetCore.Http.HttpResults;
using Backend.Application.Devices.Queries;
using Backend.Application.Common.Models;
using Backend.Application.Devices.Queries.GetDeviceList;
using Backend.Application.Devices.Commands;


namespace Backend.Web.Endpoints;
public class Devices : EndpointGroupBase
{
    public override void Map(WebApplication app)
    {
        app.MapGroup(this)
            .RequireAuthorization()
            .MapGet(GetDevice, "{id}")
            .MapGet(GetDevicesWithPagination)
            .MapPost(CreateDevice);
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

    public async Task<Created<Guid>> CreateDevice(ISender sender, CreateDeviceCommand command)
    {
        var id = await sender.Send(command);

        return TypedResults.Created($"/{nameof(Devices)}/{id}", id);
    }

   
}
