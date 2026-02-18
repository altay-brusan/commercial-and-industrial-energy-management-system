using Backend.Application.Common.Interfaces;
using Backend.Application.Common.Security;
using Backend.Domain.Entities;

namespace Backend.Application.FacilityTariffs.Commands.AssignTariffToFacility;

/// <summary>
/// Assigns a tariff to a facility.
/// If the facility already has an active tariff assignment, it will be deactivated.
/// Only Administrators can assign tariffs.
/// </summary>
[Authorize(Roles = "Administrator")]
public record AssignTariffToFacilityCommand : IRequest<Guid>
{
    public Guid FacilityId { get; init; }
    public Guid TariffId { get; init; }

    // Optional contract details for special arrangements
    public string? ContractNumber { get; init; }
    public decimal DiscountRate { get; init; } = 0m;
    public DateTimeOffset? ContractStartDate { get; init; }
    public DateTimeOffset? ContractEndDate { get; init; }
    public string? Notes { get; init; }
}

public class AssignTariffToFacilityCommandHandler : IRequestHandler<AssignTariffToFacilityCommand, Guid>
{
    private readonly IApplicationDbContext _context;

    public AssignTariffToFacilityCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Guid> Handle(AssignTariffToFacilityCommand request, CancellationToken cancellationToken)
    {
        // Use a transaction to ensure atomicity
        await using var transaction = await _context.BeginTransactionAsync(cancellationToken);

        try
        {
            // Verify facility exists
            var facility = await _context.Facilities
                .FirstOrDefaultAsync(f => f.Id == request.FacilityId, cancellationToken);
            Guard.Against.NotFound(request.FacilityId, facility);

            // Verify tariff exists and is active
            var tariff = await _context.Tariffs
                .FirstOrDefaultAsync(t => t.Id == request.TariffId, cancellationToken);
            Guard.Against.NotFound(request.TariffId, tariff);

            if (!tariff.IsActive)
            {
                throw new InvalidOperationException(
                    $"Cannot assign tariff {request.TariffId} because it is not active. " +
                    "Please use an active tariff.");
            }

            // Deactivate any existing active tariff assignments for this facility
            var existingActiveAssignments = await _context.FacilityTariffs
                .Where(ft => ft.FacilityId == request.FacilityId && ft.IsActive)
                .ToListAsync(cancellationToken);

            foreach (var existingAssignment in existingActiveAssignments)
            {
                existingAssignment.IsActive = false;
            }

            // Create the new assignment with contract details
            var facilityTariff = new FacilityTariff
            {
                FacilityId = request.FacilityId,
                TariffId = request.TariffId,
                IsActive = true,
                ContractNumber = request.ContractNumber,
                DiscountRate = request.DiscountRate,
                ContractStartDate = request.ContractStartDate,
                ContractEndDate = request.ContractEndDate,
                Notes = request.Notes
            };

            _context.FacilityTariffs.Add(facilityTariff);
            await _context.SaveChangesAsync(cancellationToken);

            await transaction.CommitAsync(cancellationToken);

            return facilityTariff.Id;
        }
        catch
        {
            await transaction.RollbackAsync(cancellationToken);
            throw;
        }
    }
}
