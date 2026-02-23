using Backend.Application.DeviceSpecificationTemplates.Commands;
using Backend.Application.DeviceSpecificationTemplates.Queries;
using Microsoft.AspNetCore.Http.HttpResults;

namespace Backend.Web.Endpoints;

public class DeviceSpecificationTemplates : EndpointGroupBase
{
    public override void Map(WebApplication app)
    {
        var group = app.MapGroup("/api/v1/DeviceSpecificationTemplates")
            .WithGroupName("DeviceSpecificationTemplates")
            .WithTags("DeviceSpecificationTemplates")
            .RequireAuthorization();

        group.MapGet("/", GetTemplates);
        group.MapGet("/{id}", GetTemplate);
        group.MapPost("/", CreateTemplate);
        group.MapPut("/{id}", UpdateTemplate);
    }

    public static async Task<Ok<List<DeviceSpecificationTemplateDto>>> GetTemplates(
        ISender sender, Guid? deviceTypeId)
    {
        var result = await sender.Send(new GetDeviceSpecificationTemplatesQuery { DeviceTypeId = deviceTypeId });
        return TypedResults.Ok(result);
    }

    public static async Task<Ok<DeviceSpecificationTemplateDto>> GetTemplate(
        ISender sender, Guid id)
    {
        var result = await sender.Send(new GetDeviceSpecificationTemplateQuery { Id = id });
        return TypedResults.Ok(result);
    }

    public static async Task<Created<Guid>> CreateTemplate(
        ISender sender, CreateDeviceSpecificationTemplateCommand command)
    {
        var id = await sender.Send(command);
        return TypedResults.Created($"/api/v1/DeviceSpecificationTemplates/{id}", id);
    }

    public static async Task<NoContent> UpdateTemplate(
        ISender sender, Guid id, UpdateDeviceSpecificationTemplateCommand command)
    {
        if (id != command.Id)
            throw new BadHttpRequestException("Route ID does not match body ID");

        await sender.Send(command);
        return TypedResults.NoContent();
    }
}
