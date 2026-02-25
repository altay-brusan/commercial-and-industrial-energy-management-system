using System.Text.Json;
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

        // TODO: TEMPORARY — ThingsBoard demo integration endpoint. Remove once MQTT-based ingestion
        // from ThingsBoard is fully operational and this HTTP bridge is no longer needed.
        // ThingsBoard HTTP Integration sends an array of telemetry messages with PascalCase
        // property names and Payload as a JSON object (not a string). This endpoint translates
        // that format into our standard IngestDeviceDataCommand pipeline.
        group.MapPost("/Ingest/ThingsBoard", IngestThingsBoard);
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

    // TODO: TEMPORARY — Remove this endpoint when ThingsBoard demo is decommissioned.
    public static async Task<Ok<IngestBatchResultDto>> IngestThingsBoard(ISender sender, List<ThingsBoardTelemetryDto> messages)
    {
        var commands = messages.Select(m => new IngestDeviceDataCommand
        {
            RtuSn = m.RtuSn,
            DeviceSn = m.DeviceSn,
            Kind = m.Kind,
            Payload = m.Payload.GetRawText(),
            DeviceLocalTime = m.DeviceLocalTime
        }).ToList();

        var batchCommand = new IngestDeviceDataBatchCommand { Messages = commands };
        var result = await sender.Send(batchCommand);
        return TypedResults.Ok(result);
    }
}

public record IngestResultDto
{
    public bool Success { get; init; }
}

/// <summary>
/// TODO: TEMPORARY DTO for ThingsBoard HTTP Integration.
/// ThingsBoard sends telemetry as a JSON array with PascalCase property names
/// and Payload as a JSON object. Remove when ThingsBoard demo is decommissioned.
/// </summary>
public record ThingsBoardTelemetryDto
{
    public string DeviceSn { get; init; } = string.Empty;
    public string RtuSn { get; init; } = string.Empty;
    public string Kind { get; init; } = string.Empty;
    public JsonElement Payload { get; init; }
    public DateTimeOffset? DeviceLocalTime { get; init; }
    public int? Version { get; init; }
    public int? PayloadVersion { get; init; }
}
