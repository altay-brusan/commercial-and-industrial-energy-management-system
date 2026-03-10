using Backend.Application.LogicalDevices.Commands;
using Backend.Application.LogicalDevices.Queries;
using Microsoft.AspNetCore.Http.HttpResults;

namespace Backend.Web.Endpoints;

public class LogicalDevices : EndpointGroupBase
{
    public override void Map(WebApplication app)
    {
        var group = app.MapGroup("/api/v1/LogicalDevices")
            .WithGroupName("LogicalDevices")
            .WithTags("LogicalDevices")
            .RequireAuthorization();

        group.MapGet("/", GetLogicalDevices);
        group.MapGet("/{id}", GetLogicalDevice);
        group.MapPost("/", CreateLogicalDevice);
        group.MapPut("/{id}", UpdateLogicalDevice);
    }

    public static async Task<Ok<List<LogicalDeviceDto>>> GetLogicalDevices(
        ISender sender, string? category)
    {
        var result = await sender.Send(new GetLogicalDevicesQuery { Category = category });
        return TypedResults.Ok(result);
    }

    public static async Task<Ok<LogicalDeviceDto>> GetLogicalDevice(
        ISender sender, Guid id)
    {
        var result = await sender.Send(new GetLogicalDeviceQuery { Id = id });
        return TypedResults.Ok(result);
    }

    public static async Task<Created<Guid>> CreateLogicalDevice(
        ISender sender, CreateLogicalDeviceCommand command)
    {
        var id = await sender.Send(command);
        return TypedResults.Created($"/api/v1/LogicalDevices/{id}", id);
    }

    public static async Task<NoContent> UpdateLogicalDevice(
        ISender sender, Guid id, UpdateLogicalDeviceCommand command)
    {
        if (id != command.Id)
            throw new BadHttpRequestException("Route ID does not match body ID");

        await sender.Send(command);
        return TypedResults.NoContent();
    }
}
