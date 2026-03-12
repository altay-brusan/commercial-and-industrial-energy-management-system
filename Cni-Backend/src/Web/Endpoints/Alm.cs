using System.Diagnostics;
using Backend.Application.Alm.Commands.ProcessAlmCallback;
using Backend.Application.Alm.Commands.TriggerPrediction;
using Backend.Application.Alm.Queries.GetLatestPrediction;
using Backend.Application.Alm.Queries.GetPredictions;
using Backend.Application.Common.Models.Alm;
using Backend.Domain.Enums;
using Backend.Web.Infrastructure;
using Microsoft.AspNetCore.Http.HttpResults;

namespace Backend.Web.Endpoints;

public class Alm : EndpointGroupBase
{
    public override void Map(WebApplication app)
    {
        // Callback group — API key auth (no JWT)
        var callbackGroup = app.MapGroup("/api/v1/Alm")
            .WithGroupName("Alm")
            .WithTags("Alm")
            .AddEndpointFilter<AlmApiKeyEndpointFilter>();

        callbackGroup.MapPost("/Callback", HandleCallback);

        // Authenticated group — JWT auth
        var authGroup = app.MapGroup("/api/v1/Alm")
            .WithGroupName("Alm")
            .WithTags("Alm")
            .RequireAuthorization();

        authGroup.MapGet("/Predictions/{facilityId}", GetPredictions);
        authGroup.MapGet("/Predictions/{facilityId}/Latest", GetLatestPrediction);
        authGroup.MapPost("/Predictions/{facilityId}/Trigger", TriggerPrediction);
    }

    public static async Task<Ok<AlmCallbackResultDto>> HandleCallback(ISender sender, AlmCallbackPayload payload)
    {
        var stopwatch = Stopwatch.StartNew();

        var command = new ProcessAlmCallbackCommand
        {
            JobId = payload.JobId,
            JobStatus = payload.JobStatus,
            ResultStatusCode = payload.Result?.StatusCode,
            ResultMessage = payload.Result?.Message,
            Forecast = payload.Result?.Forecast?.Select(f => new ProcessAlmForecastItem
            {
                Timestamp = f.Timestamp,
                Value = f.Value,
                Step = f.Step
            }).ToList()
        };

        var result = await sender.Send(command);

        stopwatch.Stop();
        var jobType = payload.JobStatus; // Use status as label since we don't know type at endpoint level
        AppMetrics.AlmCallbackDuration
            .WithLabels(jobType)
            .Observe(stopwatch.Elapsed.TotalSeconds);

        return TypedResults.Ok(new AlmCallbackResultDto { Success = result });
    }

    public static async Task<Ok<List<AlmPredictionSummaryDto>>> GetPredictions(
        ISender sender, Guid facilityId, AlmJobType? jobType, AlmJobStatus? status, int? limit)
    {
        var result = await sender.Send(new GetPredictionsQuery
        {
            FacilityId = facilityId,
            JobType = jobType,
            Status = status,
            Limit = limit ?? 20
        });
        return TypedResults.Ok(result);
    }

    public static async Task<Results<Ok<AlmPredictionDto>, NotFound>> GetLatestPrediction(
        ISender sender, Guid facilityId, AlmJobType jobType)
    {
        var result = await sender.Send(new GetLatestPredictionQuery
        {
            FacilityId = facilityId,
            JobType = jobType
        });

        return result != null
            ? TypedResults.Ok(result)
            : TypedResults.NotFound();
    }

    public static async Task<Ok<TriggerPredictionResult>> TriggerPrediction(
        ISender sender, Guid facilityId, AlmJobType jobType)
    {
        var result = await sender.Send(new TriggerPredictionCommand
        {
            FacilityId = facilityId,
            JobType = jobType
        });
        return TypedResults.Ok(result);
    }
}

public record AlmCallbackResultDto
{
    public bool Success { get; init; }
}
