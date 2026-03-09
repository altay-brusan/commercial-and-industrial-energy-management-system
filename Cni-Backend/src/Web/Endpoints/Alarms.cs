using Backend.Application.Alarms.Commands.AcknowledgeAlarm;
using Backend.Application.Alarms.Commands.AssignAlarmToMaintenance;
using Backend.Application.Alarms.Commands.ResolveAlarm;
using Backend.Application.Alarms.Queries;
using Backend.Application.Alarms.Queries.GetAlarm;
using Backend.Application.Alarms.Queries.GetAlarms;
using Backend.Application.Alarms.Queries.GetAlarmHistory;
using Backend.Application.Alarms.Queries.GetAlarmStatistics;
using Backend.Application.Alarms.Queries.GetAlarmSummary;
using Microsoft.AspNetCore.Http.HttpResults;

namespace Backend.Web.Endpoints;

public class Alarms : EndpointGroupBase
{
    public override void Map(WebApplication app)
    {
        var group = app.MapGroup("/api/v1/Alarms")
            .WithGroupName("Alarms")
            .WithTags("Alarms")
            .RequireAuthorization();

        group.MapGet("/", GetAlarms);
        group.MapGet("/{facilityId}/Summary", GetAlarmSummary);
        group.MapGet("/{facilityId}/Statistics", GetAlarmStatistics);
        group.MapGet("/History/{deviceName}", GetAlarmHistory);
        group.MapGet("/{id}", GetAlarm);
        group.MapPatch("/{id}/Acknowledge", AcknowledgeAlarm);
        group.MapPatch("/{id}/Resolve", ResolveAlarm);
        group.MapPost("/{id}/AssignToMaintenance", AssignToMaintenance);
    }

    public static async Task<Ok<AlarmListResponseDto>> GetAlarms(
        ISender sender,
        Guid? facilityId, string? severity, string? status, Guid? deviceId,
        bool? isActive, DateTimeOffset? dateFrom, DateTimeOffset? dateTo,
        string? searchTerm, int? page, int? pageSize)
    {
        var result = await sender.Send(new GetAlarmsQuery
        {
            FacilityId = facilityId,
            Severity = severity,
            Status = status,
            DeviceId = deviceId,
            IsActive = isActive,
            DateFrom = dateFrom,
            DateTo = dateTo,
            SearchTerm = searchTerm,
            Page = page ?? 1,
            PageSize = pageSize ?? 20
        });
        return TypedResults.Ok(result);
    }

    public static async Task<Ok<AlarmSummaryDto>> GetAlarmSummary(
        ISender sender, Guid facilityId)
    {
        var result = await sender.Send(new GetAlarmSummaryQuery { FacilityId = facilityId });
        return TypedResults.Ok(result);
    }

    public static async Task<Ok<AlarmStatisticsDto>> GetAlarmStatistics(
        ISender sender, Guid facilityId)
    {
        var result = await sender.Send(new GetAlarmStatisticsQuery { FacilityId = facilityId });
        return TypedResults.Ok(result);
    }

    public static async Task<Ok<AlarmHistoryResponseDto>> GetAlarmHistory(
        ISender sender, string deviceName)
    {
        var result = await sender.Send(new GetAlarmHistoryQuery { DeviceName = deviceName });
        return TypedResults.Ok(result);
    }

    public static async Task<Ok<AlarmDto>> GetAlarm(ISender sender, Guid id)
    {
        var result = await sender.Send(new GetAlarmQuery { Id = id });
        return TypedResults.Ok(result);
    }

    public static async Task<Ok<AlarmDto>> AcknowledgeAlarm(ISender sender, Guid id)
    {
        var result = await sender.Send(new AcknowledgeAlarmCommand { Id = id });
        return TypedResults.Ok(result);
    }

    public static async Task<Ok<AlarmDto>> ResolveAlarm(ISender sender, Guid id)
    {
        var result = await sender.Send(new ResolveAlarmCommand { Id = id });
        return TypedResults.Ok(result);
    }

    public static async Task<Ok<AssignAlarmToMaintenanceResponseDto>> AssignToMaintenance(
        ISender sender, Guid id, AssignAlarmToMaintenanceRequest body)
    {
        var result = await sender.Send(new AssignAlarmToMaintenanceCommand
        {
            AlarmId = id,
            Title = body.Title,
            Description = body.Description,
            Priority = body.Priority,
            AssignedToId = body.AssignedToId,
            DueDate = body.DueDate
        });
        return TypedResults.Ok(result);
    }
}

public record AssignAlarmToMaintenanceRequest(
    string Title,
    string? Description,
    string Priority,
    string AssignedToId,
    DateTimeOffset DueDate);
