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
        group.MapGet("/Units/{unitId}/Header", GetUnitHeader);
        group.MapGet("/Units/{unitId}/{tab}", GetTabContent);
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

    public static async Task<IResult> GetTabContent(
        ISender sender, Guid unitId, string tab,
        string? period, string? interval,
        int? page, int? pageSize, string? severity, bool? isActive)
    {
        switch (tab.ToLowerInvariant())
        {
            case "summary":
                var summary = await sender.Send(new GetBessSummaryQuery { UnitId = unitId });
                return TypedResults.Ok(summary);

            case "health":
                var health = await sender.Send(new GetBessHealthQuery { UnitId = unitId });
                return TypedResults.Ok(health);

            case "curves":
                var curves = await sender.Send(new GetBessCurvesQuery
                {
                    UnitId = unitId,
                    Period = period ?? MonitoringPeriods.TwentyFourHours,
                    Interval = interval
                });
                return TypedResults.Ok(curves);

            case "faultflags":
                var flags = await sender.Send(new GetBessFaultFlagsQuery { UnitId = unitId });
                return TypedResults.Ok(flags);

            case "faults":
                var faults = await sender.Send(new GetBessFaultsQuery
                {
                    UnitId = unitId,
                    Page = page ?? 1,
                    PageSize = pageSize ?? 20,
                    Severity = severity,
                    IsActive = isActive
                });
                return TypedResults.Ok(faults);

            case "settings":
                var settings = await sender.Send(new GetBessSettingsQuery { UnitId = unitId });
                return TypedResults.Ok(settings);

            default:
                return TypedResults.NotFound();
        }
    }

    public static async Task<Ok<BessDeviceTreeResponseDto>> GetTree(
        ISender sender, Guid? facilityId)
    {
        var result = await sender.Send(new GetBessDeviceTreeQuery { FacilityId = facilityId });
        return TypedResults.Ok(result);
    }
}
