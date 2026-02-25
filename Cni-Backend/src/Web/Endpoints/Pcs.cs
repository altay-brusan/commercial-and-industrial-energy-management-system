using Backend.Application.Common.Models.Monitoring;
using Backend.Application.Pcs.Queries.GetPcsCurves;
using Backend.Application.Pcs.Queries.GetPcsFaultFlags;
using Backend.Application.Pcs.Queries.GetPcsGeneral;
using Backend.Application.Pcs.Queries.GetPcsSchema;
using Backend.Application.Pcs.Queries.GetPcsUnitDetail;
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

        group.MapGet("/Units", GetUnits);
        group.MapGet("/Units/{unitId}", GetUnitDetail);
        group.MapGet("/Units/{unitId}/General", GetGeneral);
        group.MapGet("/Units/{unitId}/FaultFlags", GetFaultFlags);
        group.MapGet("/Units/{unitId}/Curves", GetCurves);
        group.MapGet("/Schema", GetSchema);
    }

    public static async Task<Ok<PcsUnitsResultDto>> GetUnits(
        ISender sender, [AsParameters] GetPcsUnitsQuery query)
    {
        var result = await sender.Send(query);
        return TypedResults.Ok(result);
    }

    public static async Task<Ok<PcsUnitDetailDto>> GetUnitDetail(
        ISender sender, Guid unitId)
    {
        var result = await sender.Send(new GetPcsUnitDetailQuery { UnitId = unitId });
        return TypedResults.Ok(result);
    }

    public static async Task<Ok<PcsGeneralDto>> GetGeneral(
        ISender sender, Guid unitId)
    {
        var result = await sender.Send(new GetPcsGeneralQuery { UnitId = unitId });
        return TypedResults.Ok(result);
    }

    public static async Task<Ok<FaultFlagsResponseDto>> GetFaultFlags(
        ISender sender, Guid unitId)
    {
        var result = await sender.Send(new GetPcsFaultFlagsQuery { UnitId = unitId });
        return TypedResults.Ok(result);
    }

    public static async Task<Ok<ChartResponseDto>> GetCurves(
        ISender sender, Guid unitId, string? period, string? interval)
    {
        var result = await sender.Send(new GetPcsCurvesQuery
        {
            UnitId = unitId,
            Period = period ?? MonitoringPeriods.TwentyFourHours,
            Interval = interval
        });
        return TypedResults.Ok(result);
    }

    public static async Task<Ok<SchemaResponseDto>> GetSchema(ISender sender)
    {
        var result = await sender.Send(new GetPcsSchemaQuery());
        return TypedResults.Ok(result);
    }
}
