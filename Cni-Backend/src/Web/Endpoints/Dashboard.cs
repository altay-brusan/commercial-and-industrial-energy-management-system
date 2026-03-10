using Backend.Application.Dashboard.Queries.GetDashboardStats;
using Backend.Application.Dashboard.Queries.GetEnergyUsage;
using Backend.Application.Dashboard.Queries.GetEnergySource;
using Backend.Application.Dashboard.Queries.GetBessState;
using Backend.Application.Dashboard.Queries.GetPcsState;
using Backend.Application.Dashboard.Queries.GetSolarCapacity;
using Backend.Application.Dashboard.Queries.GetEnergyConsumption;
using Backend.Application.Dashboard.Queries.GetCo2Emissions;
using Backend.Application.Dashboard.Queries.GetSourceDistribution;
using Backend.Application.Dashboard.Queries.GetCostSavings;
using Microsoft.AspNetCore.Http.HttpResults;

namespace Backend.Web.Endpoints;

public class Dashboard : EndpointGroupBase
{
    public override void Map(WebApplication app)
    {
        var group = app.MapGroup("/api/v1/Dashboard")
            .WithGroupName("Dashboard")
            .WithTags("Dashboard")
            .RequireAuthorization();

        group.MapGet("/{facilityId}/Stats", GetStats);
        group.MapGet("/{facilityId}/EnergyUsage", GetEnergyUsage);
        group.MapGet("/{facilityId}/EnergySource", GetEnergySource);
        group.MapGet("/{facilityId}/BessState", GetBessState);
        group.MapGet("/{facilityId}/PcsState", GetPcsState);
        group.MapGet("/{facilityId}/SolarCapacity", GetSolarCapacity);
        group.MapGet("/{facilityId}/EnergyConsumption", GetEnergyConsumption);
        group.MapGet("/{facilityId}/Co2Emissions", GetCo2Emissions);
        group.MapGet("/{facilityId}/SourceDistribution", GetSourceDistribution);
        group.MapGet("/{facilityId}/CostSavings", GetCostSavings);
    }

    public static async Task<Ok<DashboardStatsDto>> GetStats(ISender sender, Guid facilityId)
    {
        var result = await sender.Send(new GetDashboardStatsQuery { FacilityId = facilityId });
        return TypedResults.Ok(result);
    }

    public static async Task<Ok<EnergyUsageDto>> GetEnergyUsage(ISender sender, Guid facilityId)
    {
        var result = await sender.Send(new GetEnergyUsageQuery { FacilityId = facilityId });
        return TypedResults.Ok(result);
    }

    public static async Task<Ok<EnergySourcePowerDto>> GetEnergySource(ISender sender, Guid facilityId)
    {
        var result = await sender.Send(new GetEnergySourceQuery { FacilityId = facilityId });
        return TypedResults.Ok(result);
    }

    public static async Task<Ok<DashboardBessStateDto>> GetBessState(ISender sender, Guid facilityId)
    {
        var result = await sender.Send(new GetBessStateQuery { FacilityId = facilityId });
        return TypedResults.Ok(result);
    }

    public static async Task<Ok<DashboardPcsStateDto>> GetPcsState(ISender sender, Guid facilityId)
    {
        var result = await sender.Send(new GetPcsStateQuery { FacilityId = facilityId });
        return TypedResults.Ok(result);
    }

    public static async Task<Ok<DashboardSolarCapacityDto>> GetSolarCapacity(ISender sender, Guid facilityId)
    {
        var result = await sender.Send(new GetSolarCapacityQuery { FacilityId = facilityId });
        return TypedResults.Ok(result);
    }

    public static async Task<Ok<EnergyConsumptionChartDto>> GetEnergyConsumption(
        ISender sender, Guid facilityId, string? period)
    {
        var result = await sender.Send(new GetEnergyConsumptionQuery
        {
            FacilityId = facilityId,
            Period = period
        });
        return TypedResults.Ok(result);
    }

    public static async Task<Ok<Co2EmissionsDto>> GetCo2Emissions(
        ISender sender, Guid facilityId, string? period)
    {
        var result = await sender.Send(new GetCo2EmissionsQuery
        {
            FacilityId = facilityId,
            Period = period
        });
        return TypedResults.Ok(result);
    }

    public static async Task<Ok<SourceDistributionChartDto>> GetSourceDistribution(
        ISender sender, Guid facilityId, string? period)
    {
        var result = await sender.Send(new GetSourceDistributionQuery
        {
            FacilityId = facilityId,
            Period = period
        });
        return TypedResults.Ok(result);
    }

    public static async Task<Ok<CostSavingsChartDto>> GetCostSavings(
        ISender sender, Guid facilityId, string? period)
    {
        var result = await sender.Send(new GetCostSavingsQuery
        {
            FacilityId = facilityId,
            Period = period
        });
        return TypedResults.Ok(result);
    }
}
