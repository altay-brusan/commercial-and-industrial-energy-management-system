using Backend.Application.Common.Interfaces;
using Backend.Application.Common.Security;
using Backend.Domain.Entities;

namespace Backend.Application.Tariffs.Commands.UpdateTariff;

/// <summary>
/// Updates a tariff by creating a new version and deactivating the old one.
/// This preserves tariff history for auditing purposes.
/// All facilities using the old tariff will be automatically migrated to the new version.
/// Only Administrators can update tariffs.
/// </summary>
[Authorize(Roles = "Administrator")]
public record UpdateTariffCommand : IRequest<UpdateTariffResult>
{
    /// <summary>
    /// The ID of the tariff to update (this tariff will be deactivated).
    /// </summary>
    public Guid Id { get; init; }

    public string CountryName { get; init; } = string.Empty;
    public string? DistributorName { get; init; }
    public decimal DistributionFee { get; init; }
    public decimal TransmissionFee { get; init; }
    public decimal TaxRate { get; init; }
    public string Currency { get; init; } = "EUR";
}

/// <summary>
/// Result of the UpdateTariff operation.
/// </summary>
public class UpdateTariffResult
{
    /// <summary>
    /// The ID of the newly created tariff version.
    /// </summary>
    public Guid NewTariffId { get; set; }

    /// <summary>
    /// The ID of the old (now deactivated) tariff.
    /// </summary>
    public Guid OldTariffId { get; set; }

    /// <summary>
    /// Number of facilities that were migrated to the new tariff.
    /// </summary>
    public int MigratedFacilitiesCount { get; set; }
}

public class UpdateTariffCommandHandler : IRequestHandler<UpdateTariffCommand, UpdateTariffResult>
{
    private readonly IApplicationDbContext _context;

    public UpdateTariffCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<UpdateTariffResult> Handle(UpdateTariffCommand request, CancellationToken cancellationToken)
    {
        // Use a transaction to ensure atomicity
        await using var transaction = await _context.BeginTransactionAsync(cancellationToken);

        try
        {
            // Find the existing tariff
            var existingTariff = await _context.Tariffs
                .FirstOrDefaultAsync(t => t.Id == request.Id, cancellationToken);

            Guard.Against.NotFound(request.Id, existingTariff);

            // Check if the tariff is still active (prevent updating already deactivated tariffs)
            if (!existingTariff.IsActive)
            {
                throw new InvalidOperationException(
                    $"Cannot update tariff {request.Id} because it has already been deactivated. " +
                    "Please create a new tariff instead.");
            }

            // Deactivate the existing tariff
            existingTariff.IsActive = false;

            // Also deactivate any other active tariffs for the same country
            // (in case of concurrent updates or data inconsistency)
            var otherActiveTariffs = await _context.Tariffs
                .Where(t => t.Id != request.Id &&
                           t.IsActive &&
                           t.CountryName.ToLower() == request.CountryName.ToLower())
                .ToListAsync(cancellationToken);

            foreach (var tariff in otherActiveTariffs)
            {
                tariff.IsActive = false;
            }

            // Create the new tariff version
            var newTariff = new Tariff
            {
                CountryName = request.CountryName,
                DistributorName = request.DistributorName,
                DistributionFee = request.DistributionFee,
                TransmissionFee = request.TransmissionFee,
                TaxRate = request.TaxRate,
                Currency = request.Currency,
                IsActive = true
            };

            _context.Tariffs.Add(newTariff);
            await _context.SaveChangesAsync(cancellationToken);

            // Migrate all facilities using the old tariff to the new tariff
            // Find all active facility-tariff assignments that reference the old tariff
            var facilityAssignmentsToMigrate = await _context.FacilityTariffs
                .Where(ft => ft.TariffId == request.Id && ft.IsActive)
                .ToListAsync(cancellationToken);

            var migratedCount = 0;

            foreach (var oldAssignment in facilityAssignmentsToMigrate)
            {
                // Deactivate the old assignment
                oldAssignment.IsActive = false;

                // Create a new assignment pointing to the new tariff
                var newAssignment = new FacilityTariff
                {
                    FacilityId = oldAssignment.FacilityId,
                    TariffId = newTariff.Id,
                    IsActive = true
                };

                _context.FacilityTariffs.Add(newAssignment);
                migratedCount++;
            }

            if (migratedCount > 0)
            {
                await _context.SaveChangesAsync(cancellationToken);
            }

            await transaction.CommitAsync(cancellationToken);

            return new UpdateTariffResult
            {
                NewTariffId = newTariff.Id,
                OldTariffId = request.Id,
                MigratedFacilitiesCount = migratedCount
            };
        }
        catch
        {
            await transaction.RollbackAsync(cancellationToken);
            throw;
        }
    }
}
