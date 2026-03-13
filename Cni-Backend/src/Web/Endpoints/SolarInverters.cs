using Backend.Application.Common.Models.Monitoring;
using Backend.Application.SolarInverters.Commands.SendSolarCommand;
using Backend.Application.SolarInverters.Queries.GetSolarCurves;
using Backend.Application.SolarInverters.Queries.GetSolarDeviceTree;
using Backend.Application.SolarInverters.Queries.GetSolarEnergyAnalytics;
using Backend.Application.SolarInverters.Queries.GetSolarFaults;
using Backend.Application.SolarInverters.Queries.GetSolarGeneral;
using Backend.Application.SolarInverters.Queries.GetSolarHistory;
using Backend.Application.SolarInverters.Queries.GetSolarPerformance;
using Backend.Application.SolarInverters.Queries.GetSolarUnitHeader;
using Backend.Application.SolarInverters.Queries.GetSolarUnits;
using Microsoft.AspNetCore.Http.HttpResults;

namespace Backend.Web.Endpoints;

public class SolarInverters : EndpointGroupBase
{
    public override void Map(WebApplication app)
    {
        var group = app.MapGroup("/api/v1/SolarInverters")
            .WithGroupName("SolarInverters")
            .WithTags("SolarInverters")
            .RequireAuthorization();

        group.MapGet("/Units/{facilityId}", GetUnits);
        group.MapGet("/Units/{unitId}/Header", GetHeader);
        group.MapGet("/Units/{unitId}/{tab}", GetTabContent);
        group.MapGet("/DeviceTree/{facilityId}", GetDeviceTree);
        group.MapGet("/EnergyAnalytics/{facilityId}", GetEnergyAnalytics);
        group.MapGet("/Performance/{facilityId}", GetPerformance);
        group.MapPost("/Units/{unitId}/Commands/{action}", SendCommand);
    }

    public static async Task<Ok<SolarUnitsResultDto>> GetUnits(
        ISender sender, Guid facilityId, string? search, string? sortBy, string? sortDirection)
    {
        var result = await sender.Send(new GetSolarUnitsQuery
        {
            FacilityId = facilityId,
            Search = search,
            SortBy = sortBy,
            SortDirection = sortDirection
        });
        return TypedResults.Ok(result);
    }

    public static async Task<Ok<SolarUnitHeaderDto>> GetHeader(
        ISender sender, Guid unitId)
    {
        var result = await sender.Send(new GetSolarUnitHeaderQuery { UnitId = unitId });
        return TypedResults.Ok(result);
    }

    public static async Task<IResult> GetTabContent(
        ISender sender, Guid unitId, string tab,
        string? period, string? interval,
        int? page, int? pageSize, string? severity, bool? isActive)
    {
        switch (tab.ToLowerInvariant())
        {
            case "general":
                var general = await sender.Send(new GetSolarGeneralQuery { UnitId = unitId });
                return TypedResults.Ok(general);

            case "faults":
                var faults = await sender.Send(new GetSolarFaultsQuery
                {
                    UnitId = unitId,
                    Page = page ?? 1,
                    PageSize = pageSize ?? 20,
                    Severity = severity,
                    IsActive = isActive
                });
                return TypedResults.Ok(faults);

            case "curves":
                var curves = await sender.Send(new GetSolarCurvesQuery
                {
                    UnitId = unitId,
                    Period = period ?? MonitoringPeriods.TwentyFourHours,
                    Interval = interval
                });
                return TypedResults.Ok(curves);

            case "history":
                var history = await sender.Send(new GetSolarHistoryQuery
                {
                    UnitId = unitId,
                    Period = period ?? MonitoringPeriods.TwentyFourHours,
                    Interval = interval ?? MonitoringIntervals.FiveMin
                });
                return TypedResults.Ok(history);

            default:
                return TypedResults.NotFound();
        }
    }

    public static async Task<Ok<SolarDeviceTreeResponseDto>> GetDeviceTree(
        ISender sender, Guid facilityId)
    {
        var result = await sender.Send(new GetSolarDeviceTreeQuery { FacilityId = facilityId });
        return TypedResults.Ok(result);
    }

    public static async Task<Ok<SolarEnergyAnalyticsDto>> GetEnergyAnalytics(
        ISender sender, Guid facilityId, string? period)
    {
        var result = await sender.Send(new GetSolarEnergyAnalyticsQuery
        {
            FacilityId = facilityId,
            Period = period
        });
        return TypedResults.Ok(result);
    }

    public static async Task<Ok<SolarPerformanceDto>> GetPerformance(
        ISender sender, Guid facilityId, string? period)
    {
        var result = await sender.Send(new GetSolarPerformanceQuery
        {
            FacilityId = facilityId,
            Period = period
        });
        return TypedResults.Ok(result);
    }

    public static async Task<Ok<SolarCommandResultDto>> SendCommand(
        ISender sender, Guid unitId, string action)
    {
        var result = await sender.Send(new SendSolarCommandCommand
        {
            UnitId = unitId,
            Action = action
        });
        return TypedResults.Ok(result);
    }
}
