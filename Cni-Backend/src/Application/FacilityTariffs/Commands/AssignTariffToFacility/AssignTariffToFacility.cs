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
public record AssignTariffToFacilityCommand : IRequest<int>
{
    public int FacilityId { get; init; }
    public int TariffId { get; init; }
}

public class AssignTariffToFacilityCommandValidator : AbstractValidator<AssignTariffToFacilityCommand>
{
    public AssignTariffToFacilityCommandValidator()
    {
        RuleFor(x => x.FacilityId)
            .GreaterThan(0)
            .WithMessage("Facility ID must be greater than 0.");

        RuleFor(x => x.TariffId)
            .GreaterThan(0)
            .WithMessage("Tariff ID must be greater than 0.");
    }
}

public class AssignTariffToFacilityCommandHandler : IRequestHandler<AssignTariffToFacilityCommand, int>
{
    private readonly IApplicationDbContext _context;

    public AssignTariffToFacilityCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<int> Handle(AssignTariffToFacilityCommand request, CancellationToken cancellationToken)
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

            // Create the new assignment
            var facilityTariff = new FacilityTariff
            {
                FacilityId = request.FacilityId,
                TariffId = request.TariffId,
                IsActive = true
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
