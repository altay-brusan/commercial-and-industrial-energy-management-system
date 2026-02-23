using Backend.Application.BatteryStorage.Queries.GetBessCurves;
using Backend.Application.BatteryStorage.Queries.GetBessDeviceTree;
using Backend.Application.BatteryStorage.Queries.GetBessFaultFlags;
using Backend.Application.BatteryStorage.Queries.GetBessFaults;
using Backend.Application.BatteryStorage.Queries.GetBessHealth;
using Backend.Application.BatteryStorage.Queries.GetBessHierarchy;
using Backend.Application.BatteryStorage.Queries.GetBessHierarchyLevel;
using Backend.Application.BatteryStorage.Queries.GetBessSchema;
using Backend.Application.BatteryStorage.Queries.GetBessSummary;
using Backend.Application.BatteryStorage.Queries.GetBessUnitDetail;
using Backend.Application.BatteryStorage.Queries.GetBessUnits;
using Backend.Application.Common.Models.Monitoring;
using Backend.Application.Devices.Queries.GetDeviceTree;
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

        group.MapGet("/DeviceTree", GetDeviceTree);
        group.MapGet("/Units", GetUnits);
        group.MapGet("/Units/{unitId}", GetUnitDetail);
        group.MapGet("/Units/{unitId}/Hierarchy", GetHierarchy);
        group.MapGet("/Units/{unitId}/HierarchyLevel", GetHierarchyLevel);
        group.MapGet("/Units/{unitId}/Tree", GetTree);
        group.MapGet("/Units/{unitId}/Summary", GetSummary);
        group.MapGet("/Units/{unitId}/Health", GetHealth);
        group.MapGet("/Units/{unitId}/Curves", GetCurves);
        group.MapGet("/Units/{unitId}/FaultFlags", GetFaultFlags);
        group.MapGet("/Units/{unitId}/Faults", GetFaults);
        group.MapGet("/Schema", GetSchema);
    }

    public static async Task<Ok<BessUnitsResultDto>> GetUnits(
        ISender sender, [AsParameters] GetBessUnitsQuery query)
    {
        var result = await sender.Send(query);
        return TypedResults.Ok(result);
    }

    public static async Task<Ok<BessUnitDetailDto>> GetUnitDetail(
        ISender sender, Guid unitId)
    {
        var result = await sender.Send(new GetBessUnitDetailQuery { UnitId = unitId });
        return TypedResults.Ok(result);
    }

    public static async Task<Ok<HierarchyResponseDto>> GetHierarchy(
        ISender sender, Guid unitId)
    {
        var result = await sender.Send(new GetBessHierarchyQuery { UnitId = unitId });
        return TypedResults.Ok(result);
    }

    public static async Task<Ok<HierarchyLevelResponseDto>> GetHierarchyLevel(
        ISender sender, Guid unitId, string level, string? parentId)
    {
        var result = await sender.Send(new GetBessHierarchyLevelQuery
        {
            UnitId = unitId,
            Level = level,
            ParentId = parentId
        });
        return TypedResults.Ok(result);
    }

    public static async Task<Ok<List<DynamicSectionDto>>> GetSummary(
        ISender sender, Guid unitId)
    {
        var result = await sender.Send(new GetBessSummaryQuery { UnitId = unitId });
        return TypedResults.Ok(result);
    }

    public static async Task<Ok<List<DynamicSectionDto>>> GetHealth(
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

    public static async Task<Ok<FaultsPaginatedResponseDto>> GetFaults(
        ISender sender, Guid unitId, int? pageNumber, int? pageSize, string? severity, bool? isActive)
    {
        var result = await sender.Send(new GetBessFaultsQuery
        {
            UnitId = unitId,
            PageNumber = pageNumber ?? 1,
            PageSize = pageSize ?? 20,
            Severity = severity,
            IsActive = isActive
        });
        return TypedResults.Ok(result);
    }

    public static async Task<Ok<SchemaResponseDto>> GetSchema(ISender sender)
    {
        var result = await sender.Send(new GetBessSchemaQuery());
        return TypedResults.Ok(result);
    }

    public static async Task<Ok<DeviceTreeNodeDto>> GetTree(
        ISender sender, Guid unitId)
    {
        var result = await sender.Send(new GetDeviceTreeQuery { DeviceId = unitId });
        return TypedResults.Ok(result);
    }

    public static async Task<Ok<BessDeviceTreeResponseDto>> GetDeviceTree(
        ISender sender)
    {
        var result = await sender.Send(new GetBessDeviceTreeQuery());
        return TypedResults.Ok(result);
    }
}
