using Backend.Application.Common.Models.Monitoring;
using Backend.Application.Pcs.Queries.GetPcsCurves;
using Backend.Application.Pcs.Queries.GetPcsFaultFlags;
using Backend.Application.Pcs.Queries.GetPcsGeneral;
using Backend.Application.Pcs.Queries.GetPcsHeader;
using Backend.Application.Pcs.Queries.GetPcsSchema;
using Backend.Application.Pcs.Queries.GetPcsUnits;
using Microsoft.AspNetCore.Http.HttpResults;

namespace Backend.Web.Endpoints;

public class Pcs : EndpointGroupBase
{
    public override void Map(WebApplication app)
    {
        var group = app.MapGroup("/api/v1/Pcs")
            .WithGroupName("Pcs")
            .WithTags("Pcs")
            .RequireAuthorization();

        group.MapGet("/Units/{facilityId}", GetUnits);
        group.MapGet("/Units/{unitId}/Header", GetHeader);
        group.MapGet("/Units/{unitId}/{tab}", GetTabContent);
        group.MapGet("/Schema", GetSchema);
    }

    public static async Task<Ok<PcsUnitsResultDto>> GetUnits(
        ISender sender, Guid facilityId)
    {
        var result = await sender.Send(new GetPcsUnitsQuery { FacilityId = facilityId });
        return TypedResults.Ok(result);
    }

    public static async Task<Ok<PcsHeaderDto>> GetHeader(
        ISender sender, Guid unitId)
    {
        var result = await sender.Send(new GetPcsHeaderQuery { UnitId = unitId });
        return TypedResults.Ok(result);
    }

    public static async Task<IResult> GetTabContent(
        ISender sender, Guid unitId, string tab, string? period, string? interval)
    {
        switch (tab.ToLowerInvariant())
        {
            case "general":
                var general = await sender.Send(new GetPcsGeneralQuery { UnitId = unitId });
                return TypedResults.Ok(general);

            case "fault":
                var faults = await sender.Send(new GetPcsFaultFlagsQuery { UnitId = unitId });
                return TypedResults.Ok(faults);

            case "curve":
                var curves = await sender.Send(new GetPcsCurvesQuery
                {
                    UnitId = unitId,
                    Period = period ?? MonitoringPeriods.TwentyFourHours,
                    Interval = interval
                });
                return TypedResults.Ok(curves);

            default:
                return TypedResults.NotFound();
        }
    }

    public static async Task<Ok<SchemaResponseDto>> GetSchema(ISender sender)
    {
        var result = await sender.Send(new GetPcsSchemaQuery());
        return TypedResults.Ok(result);
    }
}
