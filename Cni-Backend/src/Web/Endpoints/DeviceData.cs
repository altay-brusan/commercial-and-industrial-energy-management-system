using System.Text.Json;
using Backend.Application.DeviceData.Commands.IngestDeviceAlarm;
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
        group.MapPost("/Ingest/Alarm", IngestAlarm);

        // TODO: TEMPORARY — ThingsBoard demo integration endpoint. Remove once MQTT-based ingestion
        // from ThingsBoard is fully operational and this HTTP bridge is no longer needed.
        // ThingsBoard HTTP Integration sends a single telemetry message with PascalCase
        // property names and Payload as a JSON object (not a string). This endpoint translates
        // that format into our standard IngestDeviceDataCommand pipeline.
        // No API key auth — ThingsBoard cannot send custom headers in its HTTP integration.
        app.MapGroup("/api/v1/DeviceData")
            .WithGroupName("DeviceData")
            .WithTags("DeviceData")
            .MapPost("/Ingest/ThingsBoard", IngestThingsBoard);
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

    public static async Task<Ok<IngestDeviceAlarmResultDto>> IngestAlarm(ISender sender, IngestDeviceAlarmCommand command)
    {
        var result = await sender.Send(command);
        return TypedResults.Ok(result);
    }

    // TODO: TEMPORARY — Remove this endpoint when ThingsBoard demo is decommissioned.
    public static async Task<Ok<IngestResultDto>> IngestThingsBoard(ISender sender, ThingsBoardTelemetryDto message)
    {
        var command = new IngestDeviceDataCommand
        {
            RtuSn = message.RtuSn,
            DeviceName = message.DeviceName,
            Kind = message.Kind,
            Path = message.Path,
            Payload = message.Payload.GetRawText(),
            DeviceLocalTime = message.DeviceLocalTime
        };

        var result = await sender.Send(command);
        return TypedResults.Ok(new IngestResultDto { Success = result });
    }
}

public record IngestResultDto
{
    public bool Success { get; init; }
}

/// <summary>
/// TODO: TEMPORARY DTO for ThingsBoard HTTP Integration.
/// ThingsBoard sends a single telemetry message with PascalCase property names
/// and Payload as a JSON object. Remove when ThingsBoard demo is decommissioned.
/// </summary>
public record ThingsBoardTelemetryDto
{
    public string DeviceName { get; init; } = string.Empty;
    public string RtuSn { get; init; } = string.Empty;
    public string Kind { get; init; } = string.Empty;
    public string? Path { get; init; }
    public JsonElement Payload { get; init; }
    public DateTimeOffset? DeviceLocalTime { get; init; }
    public int? Version { get; init; }
    public int? PayloadVersion { get; init; }
}
