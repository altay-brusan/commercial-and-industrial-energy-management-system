using Backend.Application.Alarms.Commands.AcknowledgeAlarm;
using Backend.Application.Alarms.Commands.ResolveAlarm;
using Backend.Application.Alarms.Queries;
using Backend.Application.Alarms.Queries.GetAlarm;
using Backend.Application.Alarms.Queries.GetAlarms;
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
        group.MapGet("/Summary", GetAlarmSummary);
        group.MapGet("/{id}", GetAlarm);
        group.MapPatch("/{id}/Acknowledge", AcknowledgeAlarm);
        group.MapPatch("/{id}/Resolve", ResolveAlarm);
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
        ISender sender, Guid? facilityId)
    {
        var result = await sender.Send(new GetAlarmSummaryQuery { FacilityId = facilityId });
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
}
