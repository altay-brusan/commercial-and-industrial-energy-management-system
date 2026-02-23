using Backend.Application.DeviceData.Commands.IngestDeviceData;
using Backend.Web.Infrastructure;
using Microsoft.AspNetCore.Http.HttpResults;

namespace Backend.Web.Endpoints;

public class DeviceData : EndpointGroupBase
{
    public override void Map(WebApplication app)
    {
        var group = app.MapGroup("/api/v1/DeviceData")
            .WithGroupName("DeviceData")
            .WithTags("DeviceData")
            .AddEndpointFilter<ApiKeyEndpointFilter>();

        group.MapPost("/Ingest", Ingest);
        group.MapPost("/Ingest/Batch", IngestBatch);
    }

    public static async Task<Ok<IngestResultDto>> Ingest(ISender sender, IngestDeviceDataCommand command)
    {
        var result = await sender.Send(command);
        return TypedResults.Ok(new IngestResultDto { Success = result });
    }

    public static async Task<Ok<IngestBatchResultDto>> IngestBatch(ISender sender, IngestDeviceDataBatchCommand command)
    {
        var result = await sender.Send(command);
        return TypedResults.Ok(result);
    }
}

public record IngestResultDto
{
    public bool Success { get; init; }
}
