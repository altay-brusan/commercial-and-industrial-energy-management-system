using Backend.Application.Common.Interfaces;
using Backend.Application.Common.Security;

namespace Backend.Application.FacilityTariffs.Commands.DeactivateFacilityTariff;

/// <summary>
/// Deactivates (soft deletes) a facility-tariff assignment.
/// The assignment record is preserved for historical reference but marked as inactive.
/// Only Administrators can deactivate facility tariff assignments.
/// </summary>
[Authorize(Roles = "Administrator")]
public record DeactivateFacilityTariffCommand : IRequest<bool>
{
    /// <summary>
    /// The ID of the facility-tariff assignment to deactivate.
    /// </summary>
    public Guid Id { get; init; }
}

public class DeactivateFacilityTariffCommandHandler : IRequestHandler<DeactivateFacilityTariffCommand, bool>
{
    private readonly IApplicationDbContext _context;

    public DeactivateFacilityTariffCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<bool> Handle(DeactivateFacilityTariffCommand request, CancellationToken cancellationToken)
    {
        var facilityTariff = await _context.FacilityTariffs
            .FirstOrDefaultAsync(ft => ft.Id == request.Id, cancellationToken);

        Guard.Against.NotFound(request.Id, facilityTariff);

        // Check if already deactivated
        if (!facilityTariff.IsActive)
        {
            // Already inactive, return true (idempotent operation)
            return true;
        }

        facilityTariff.IsActive = false;
        await _context.SaveChangesAsync(cancellationToken);

        return true;
    }
}
