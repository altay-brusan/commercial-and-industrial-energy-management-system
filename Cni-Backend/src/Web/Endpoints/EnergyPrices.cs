using Backend.Application.Common.Models;
using Backend.Application.EnergyPrices.Queries.GetDayAheadPrices;
using Backend.Application.EnergyPrices.Queries.GetLiveDayAheadEnergyPrices;
using Backend.Application.FacilityTariffs;
using Backend.Application.FacilityTariffs.Commands.AssignTariffToFacility;
using Backend.Application.FacilityTariffs.Commands.ChangeFacilityTariff;
using Backend.Application.FacilityTariffs.Commands.DeactivateFacilityTariff;
using Backend.Application.FacilityTariffs.Queries.GetActiveTariffForFacility;
using Backend.Application.FacilityTariffs.Queries.GetFacilityTariff;
using Backend.Application.FacilityTariffs.Queries.GetFacilityTariffs;
using Backend.Application.Tariffs;
using Backend.Application.Tariffs.Commands.CreateTariff;
using Backend.Application.Tariffs.Commands.DeactivateTariff;
using Backend.Application.Tariffs.Commands.UpdateTariff;
using Backend.Application.Tariffs.Queries.GetActiveTariffByCountry;
using Backend.Application.Tariffs.Queries.GetTariff;
using Backend.Application.Tariffs.Queries.GetTariffs;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Web.Endpoints;

/// <summary>
/// Energy Prices API endpoints organized into Day-Ahead, Tariffs, and FacilityTariffs categories.
///
/// Day-Ahead endpoints:
///   - GET /Day-Ahead: Query stored Day-Ahead price data by facility from database
///   - GET /Day-Ahead/Live: Fetch real-time Day-Ahead prices directly from Market Operator API
///
/// Tariffs endpoints: Manage tariff configurations (Administrator only)
/// FacilityTariffs endpoints: Manage facility-tariff assignments (Administrator only)
/// </summary>
public class EnergyPrices : EndpointGroupBase
{
    public override void Map(WebApplication app)
    {
        var group = app.MapGroup(this)
            .RequireAuthorization();

        // Day-Ahead endpoints
        group.MapGet(GetDayAheadPrices, "Day-Ahead");
        group.MapGet(GetLiveDayAheadPrices, "Day-Ahead/Live");

        // Tariffs endpoints - Administrator only
        group.MapGet(GetTariffs, "Tariffs");
        group.MapGet(GetTariffById, "Tariffs/{id}");
        group.MapGet(GetActiveTariffByCountry, "Tariffs/Country/{countryName}");
        group.MapPost(CreateTariff, "Tariffs");
        group.MapPut(UpdateTariff, "Tariffs/{id}");
        group.MapDelete(DeactivateTariff, "Tariffs/{id}");

        // FacilityTariffs endpoints - Administrator only
        group.MapGet(GetFacilityTariffs, "FacilityTariffs");
        group.MapGet(GetFacilityTariffById, "FacilityTariffs/{id}");
        group.MapGet(GetActiveTariffForFacility, "FacilityTariffs/Facility/{facilityId}");
        group.MapPost(AssignTariffToFacility, "FacilityTariffs");
        group.MapPut(ChangeFacilityTariff, "FacilityTariffs/{id}");
        group.MapDelete(DeactivateFacilityTariff, "FacilityTariffs/{id}");
    }

    #region Day-Ahead Endpoints

    /// <summary>
    /// Gets Day-Ahead energy prices for a facility within a date range from the database.
    /// Uses the facility's country (via Firm's MarketOperator configuration) to fetch historical prices.
    /// Includes the facility's tariff information including any contract discounts.
    /// </summary>
    /// <param name="sender">MediatR sender</param>
    /// <param name="facilityId">Facility ID to get prices for</param>
    /// <param name="startDate">Start date (inclusive)</param>
    /// <param name="endDate">End date (inclusive). If null, only startDate's data is returned.</param>
    public async Task<Results<Ok<DayAheadEnergyPricesDto>, NotFound>> GetDayAheadPrices(
        ISender sender,
        [FromQuery] Guid facilityId,
        [FromQuery] DateTimeOffset startDate,
        [FromQuery] DateTimeOffset? endDate = null)
    {
        var query = new GetDayAheadPricesQuery
        {
            FacilityId = facilityId,
            StartDate = startDate,
            EndDate = endDate
        };
        var result = await sender.Send(query);

        if (result == null)
        {
            return TypedResults.NotFound();
        }

        return TypedResults.Ok(result);
    }

    /// <summary>
    /// Fetches live Day-Ahead prices directly from the Market Operator API.
    /// Returns prices with tax and distribution fees applied based on firm's tariff.
    /// Resolution is determined by the Market Operator (e.g., ENTSO-E returns PT15M or PT60M based on market).
    /// This does NOT store data - use this for real-time display.
    /// </summary>
    /// <param name="sender">MediatR sender</param>
    /// <param name="firmId">Firm ID to fetch prices for.</param>
    /// <param name="startDate">Start date (inclusive). If not specified, uses today or tomorrow based on MO publication rules.</param>
    /// <param name="endDate">End date (inclusive). If not specified, defaults to startDate + 1 day.</param>
    public async Task<Ok<LiveDayAheadEnergyPricesDto>> GetLiveDayAheadPrices(
        ISender sender,
        [FromQuery] Guid firmId,
        [FromQuery] DateTimeOffset? startDate = null,
        [FromQuery] DateTimeOffset? endDate = null)
    {
        var query = new GetLiveDayAheadEnergyPricesQuery
        {
            FirmId = firmId,
            StartDate = startDate,
            EndDate = endDate
        };
        var result = await sender.Send(query);
        return TypedResults.Ok(result);
    }

    #endregion

    #region Tariffs Endpoints

    /// <summary>
    /// Gets a paginated list of tariffs. Administrator only.
    /// </summary>
    /// <param name="sender">MediatR sender</param>
    /// <param name="pageNumber">Page number (default: 1)</param>
    /// <param name="pageSize">Page size (default: 10, max: 100)</param>
    /// <param name="isActive">Filter by active status (optional)</param>
    /// <param name="countryName">Filter by country name (optional, partial match)</param>
    public async Task<Ok<PaginatedList<TariffBriefDto>>> GetTariffs(
        ISender sender,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] bool? isActive = null,
        [FromQuery] string? countryName = null)
    {
        var query = new GetTariffsQuery
        {
            PageNumber = pageNumber,
            PageSize = pageSize,
            IsActive = isActive,
            CountryName = countryName
        };
        var result = await sender.Send(query);
        return TypedResults.Ok(result);
    }

    /// <summary>
    /// Gets a single tariff by ID. Administrator only.
    /// </summary>
    /// <param name="sender">MediatR sender</param>
    /// <param name="id">Tariff ID</param>
    public async Task<Results<Ok<TariffDto>, NotFound>> GetTariffById(ISender sender, Guid id)
    {
        var query = new GetTariffQuery { Id = id };
        var result = await sender.Send(query);

        if (result == null)
        {
            return TypedResults.NotFound();
        }

        return TypedResults.Ok(result);
    }

    /// <summary>
    /// Gets the active tariff for a specific country.
    /// Returns the most recently created active tariff for the country.
    /// </summary>
    /// <param name="sender">MediatR sender</param>
    /// <param name="countryName">Country name (e.g., "Netherlands", "Germany", "Turkey")</param>
    public async Task<Results<Ok<TariffDto>, NotFound>> GetActiveTariffByCountry(ISender sender, string countryName)
    {
        var query = new GetActiveTariffByCountryQuery { CountryName = countryName };
        var result = await sender.Send(query);

        if (result == null)
        {
            return TypedResults.NotFound();
        }

        return TypedResults.Ok(result);
    }

    /// <summary>
    /// Creates a new tariff configuration. Administrator only.
    /// If a tariff already exists for the country, the existing active tariff(s) will be deactivated.
    /// </summary>
    /// <param name="sender">MediatR sender</param>
    /// <param name="command">Tariff creation data</param>
    /// <returns>The ID of the newly created tariff</returns>
    public async Task<Created<Guid>> CreateTariff(ISender sender, CreateTariffCommand command)
    {
        var id = await sender.Send(command);
        return TypedResults.Created($"/api/EnergyPrices/Tariffs/{id}", id);
    }

    /// <summary>
    /// Updates a tariff by creating a new version and deactivating the old one. Administrator only.
    /// This preserves tariff history for auditing purposes.
    /// All facilities using the old tariff will be automatically migrated to the new version.
    /// </summary>
    /// <param name="sender">MediatR sender</param>
    /// <param name="id">The ID of the tariff to update (will be deactivated)</param>
    /// <param name="command">New tariff data</param>
    /// <returns>Result containing new tariff ID and migration count</returns>
    public async Task<Ok<UpdateTariffResult>> UpdateTariff(ISender sender, Guid id, UpdateTariffCommand command)
    {
        // Ensure the ID in the route matches the command
        if (command.Id != id)
        {
            command = command with { Id = id };
        }

        var result = await sender.Send(command);
        return TypedResults.Ok(result);
    }

    /// <summary>
    /// Deactivates (soft deletes) a tariff. Administrator only.
    /// The tariff record is preserved for historical reference but marked as inactive.
    /// </summary>
    /// <param name="sender">MediatR sender</param>
    /// <param name="id">The ID of the tariff to deactivate</param>
    public async Task<Results<Ok, NotFound>> DeactivateTariff(ISender sender, Guid id)
    {
        var command = new DeactivateTariffCommand { Id = id };
        await sender.Send(command);
        return TypedResults.Ok();
    }

    #endregion

    #region FacilityTariffs Endpoints

    /// <summary>
    /// Gets a paginated list of facility-tariff assignments. Administrator only.
    /// </summary>
    /// <param name="sender">MediatR sender</param>
    /// <param name="pageNumber">Page number (default: 1)</param>
    /// <param name="pageSize">Page size (default: 10, max: 100)</param>
    /// <param name="facilityId">Filter by facility ID (optional)</param>
    /// <param name="tariffId">Filter by tariff ID (optional)</param>
    /// <param name="isActive">Filter by active status (optional)</param>
    public async Task<Ok<PaginatedList<FacilityTariffBriefDto>>> GetFacilityTariffs(
        ISender sender,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] Guid? facilityId = null,
        [FromQuery] Guid? tariffId = null,
        [FromQuery] bool? isActive = null)
    {
        var query = new GetFacilityTariffsQuery
        {
            PageNumber = pageNumber,
            PageSize = pageSize,
            FacilityId = facilityId,
            TariffId = tariffId,
            IsActive = isActive
        };
        var result = await sender.Send(query);
        return TypedResults.Ok(result);
    }

    /// <summary>
    /// Gets a single facility-tariff assignment by ID. Administrator only.
    /// </summary>
    /// <param name="sender">MediatR sender</param>
    /// <param name="id">FacilityTariff ID</param>
    public async Task<Results<Ok<FacilityTariffDto>, NotFound>> GetFacilityTariffById(ISender sender, Guid id)
    {
        var query = new GetFacilityTariffQuery { Id = id };
        var result = await sender.Send(query);

        if (result == null)
        {
            return TypedResults.NotFound();
        }

        return TypedResults.Ok(result);
    }

    /// <summary>
    /// Gets the active tariff assignment for a specific facility.
    /// </summary>
    /// <param name="sender">MediatR sender</param>
    /// <param name="facilityId">Facility ID</param>
    public async Task<Results<Ok<FacilityTariffDto>, NotFound>> GetActiveTariffForFacility(ISender sender, Guid facilityId)
    {
        var query = new GetActiveTariffForFacilityQuery { FacilityId = facilityId };
        var result = await sender.Send(query);

        if (result == null)
        {
            return TypedResults.NotFound();
        }

        return TypedResults.Ok(result);
    }

    /// <summary>
    /// Assigns a tariff to a facility. Administrator only.
    /// If the facility already has an active tariff assignment, it will be deactivated.
    /// </summary>
    /// <param name="sender">MediatR sender</param>
    /// <param name="command">Assignment data (FacilityId and TariffId)</param>
    /// <returns>The ID of the newly created assignment</returns>
    public async Task<Created<Guid>> AssignTariffToFacility(ISender sender, AssignTariffToFacilityCommand command)
    {
        var id = await sender.Send(command);
        return TypedResults.Created($"/api/EnergyPrices/FacilityTariffs/{id}", id);
    }

    /// <summary>
    /// Changes a facility's tariff assignment. Administrator only.
    /// The current assignment is deactivated and a new one is created.
    /// Use this when a facility changes its distributor.
    /// </summary>
    /// <param name="sender">MediatR sender</param>
    /// <param name="id">The ID of the current assignment to replace</param>
    /// <param name="command">Change data (NewTariffId)</param>
    /// <returns>The ID of the newly created assignment</returns>
    public async Task<Ok<Guid>> ChangeFacilityTariff(ISender sender, Guid id, ChangeFacilityTariffCommand command)
    {
        // Ensure the ID in the route matches the command
        if (command.CurrentAssignmentId != id)
        {
            command = command with { CurrentAssignmentId = id };
        }

        var newId = await sender.Send(command);
        return TypedResults.Ok(newId);
    }

    /// <summary>
    /// Deactivates (soft deletes) a facility-tariff assignment. Administrator only.
    /// The assignment record is preserved for historical reference but marked as inactive.
    /// </summary>
    /// <param name="sender">MediatR sender</param>
    /// <param name="id">The ID of the assignment to deactivate</param>
    public async Task<Results<Ok, NotFound>> DeactivateFacilityTariff(ISender sender, Guid id)
    {
        var command = new DeactivateFacilityTariffCommand { Id = id };
        await sender.Send(command);
        return TypedResults.Ok();
    }

    #endregion
}
