using Backend.Application.Common.Interfaces;
using Backend.Application.Common.Security;

namespace Backend.Application.Tariffs.Commands.DeactivateTariff;

/// <summary>
/// Deactivates (soft deletes) a tariff.
/// The tariff record is preserved for historical reference but marked as inactive.
/// Only Administrators can deactivate tariffs.
/// </summary>
[Authorize(Roles = "Administrator")]
public record DeactivateTariffCommand : IRequest<bool>
{
    /// <summary>
    /// The ID of the tariff to deactivate.
    /// </summary>
    public int Id { get; init; }
}

public class DeactivateTariffCommandValidator : AbstractValidator<DeactivateTariffCommand>
{
    public DeactivateTariffCommandValidator()
    {
        RuleFor(x => x.Id)
            .GreaterThan(0)
            .WithMessage("Tariff ID must be greater than 0.");
    }
}

public class DeactivateTariffCommandHandler : IRequestHandler<DeactivateTariffCommand, bool>
{
    private readonly IApplicationDbContext _context;

    public DeactivateTariffCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<bool> Handle(DeactivateTariffCommand request, CancellationToken cancellationToken)
    {
        var tariff = await _context.Tariffs
            .FirstOrDefaultAsync(t => t.Id == request.Id, cancellationToken);

        Guard.Against.NotFound(request.Id, tariff);

        // Check if already deactivated
        if (!tariff.IsActive)
        {
            // Already inactive, return true (idempotent operation)
            return true;
        }

        tariff.IsActive = false;
        await _context.SaveChangesAsync(cancellationToken);

        return true;
    }
}
