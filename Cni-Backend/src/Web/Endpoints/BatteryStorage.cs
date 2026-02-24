using Backend.Application.BatteryStorage.Queries.GetBessCurves;
using Backend.Application.BatteryStorage.Queries.GetBessDeviceTree;
using Backend.Application.BatteryStorage.Queries.GetBessFaultFlags;
using Backend.Application.BatteryStorage.Queries.GetBessFaults;
using Backend.Application.BatteryStorage.Queries.GetBessHealth;
using Backend.Application.BatteryStorage.Queries.GetBessSettings;
using Backend.Application.BatteryStorage.Queries.GetBessSummary;
using Backend.Application.BatteryStorage.Queries.GetBessUnitHeader;
using Backend.Application.BatteryStorage.Queries.GetBessUnits;
using Backend.Application.Common.Models.Monitoring;
using Microsoft.AspNetCore.Http.HttpResults;

namespace Backend.Web.Endpoints;

public class BatteryStorage : EndpointGroupBase
{
    public override void Map(WebApplication app)
    {
        var group = app.MapGroup("/api/v1/BatteryStorage")
            .WithGroupName("BatteryStorage")
            .WithTags("BatteryStorage")
            .RequireAuthorization();

        group.MapGet("/Tree", GetTree);
        group.MapGet("/Units", GetUnits);
        group.MapGet("/Units/Header/{unitId}", GetUnitHeader);
        group.MapGet("/Units/{unitId}/Summary", GetSummary);
        group.MapGet("/Units/{unitId}/Health", GetHealth);
        group.MapGet("/Units/{unitId}/Curves", GetCurves);
        group.MapGet("/Units/{unitId}/FaultFlags", GetFaultFlags);
        group.MapGet("/Units/{unitId}/Notifications", GetNotifications);
        group.MapGet("/Units/{unitId}/Settings", GetSettings);
    }

    public static async Task<Ok<BessUnitsResultDto>> GetUnits(
        ISender sender, [AsParameters] GetBessUnitsQuery query)
    {
        var result = await sender.Send(query);
        return TypedResults.Ok(result);
    }

    public static async Task<Ok<BessUnitHeaderDto>> GetUnitHeader(
        ISender sender, Guid unitId)
    {
        var result = await sender.Send(new GetBessUnitHeaderQuery { UnitId = unitId });
        return TypedResults.Ok(result);
    }

    public static async Task<Ok<SummaryResponseDto>> GetSummary(
        ISender sender, Guid unitId)
    {
        var result = await sender.Send(new GetBessSummaryQuery { UnitId = unitId });
        return TypedResults.Ok(result);
    }

    public static async Task<Ok<SummaryResponseDto>> GetHealth(
        ISender sender, Guid unitId)
    {
        var result = await sender.Send(new GetBessHealthQuery { UnitId = unitId });
        return TypedResults.Ok(result);
    }

    public static async Task<Ok<ChartResponseDto>> GetCurves(
        ISender sender, Guid unitId, string? period, string? interval)
    {
        var result = await sender.Send(new GetBessCurvesQuery
        {
            UnitId = unitId,
            Period = period ?? MonitoringPeriods.Day,
            Interval = interval ?? MonitoringIntervals.FifteenMin
        });
        return TypedResults.Ok(result);
    }

    public static async Task<Ok<FaultFlagsResponseDto>> GetFaultFlags(
        ISender sender, Guid unitId)
    {
        var result = await sender.Send(new GetBessFaultFlagsQuery { UnitId = unitId });
        return TypedResults.Ok(result);
    }

    public static async Task<Ok<FaultsPaginatedResponseDto>> GetNotifications(
        ISender sender, Guid unitId, int? page, int? pageSize, string? severity, bool? isActive)
    {
        var result = await sender.Send(new GetBessFaultsQuery
        {
            UnitId = unitId,
            Page = page ?? 1,
            PageSize = pageSize ?? 20,
            Severity = severity,
            IsActive = isActive
        });
        return TypedResults.Ok(result);
    }

    public static async Task<Ok<SummaryResponseDto>> GetSettings(
        ISender sender, Guid unitId)
    {
        var result = await sender.Send(new GetBessSettingsQuery { UnitId = unitId });
        return TypedResults.Ok(result);
    }

    public static async Task<Ok<BessDeviceTreeResponseDto>> GetTree(
        ISender sender, Guid? facilityId)
    {
        var result = await sender.Send(new GetBessDeviceTreeQuery { FacilityId = facilityId });
        return TypedResults.Ok(result);
    }
}
