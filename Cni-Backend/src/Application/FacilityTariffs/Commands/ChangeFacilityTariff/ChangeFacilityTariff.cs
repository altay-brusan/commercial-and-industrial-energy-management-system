using Backend.Application.Common.Interfaces;
using Backend.Application.Common.Security;
using Backend.Domain.Entities;

namespace Backend.Application.FacilityTariffs.Commands.ChangeFacilityTariff;

/// <summary>
/// Changes a facility's tariff assignment.
/// This is used when a facility changes its distributor or country tariff.
/// The current assignment is deactivated and a new one is created.
/// Only Administrators can change facility tariffs.
/// </summary>
[Authorize(Roles = "Administrator")]
public record ChangeFacilityTariffCommand : IRequest<int>
{
    /// <summary>
    /// The ID of the current facility-tariff assignment to deactivate.
    /// </summary>
    public int CurrentAssignmentId { get; init; }

    /// <summary>
    /// The ID of the new tariff to assign.
    /// </summary>
    public int NewTariffId { get; init; }

    // Optional contract details for special arrangements
    public string? ContractNumber { get; init; }
    public decimal DiscountRate { get; init; } = 0m;
    public DateTimeOffset? ContractStartDate { get; init; }
    public DateTimeOffset? ContractEndDate { get; init; }
    public string? Notes { get; init; }
}

public class ChangeFacilityTariffCommandValidator : AbstractValidator<ChangeFacilityTariffCommand>
{
    public ChangeFacilityTariffCommandValidator()
    {
        RuleFor(x => x.CurrentAssignmentId)
            .GreaterThan(0)
            .WithMessage("Current assignment ID must be greater than 0.");

        RuleFor(x => x.NewTariffId)
            .GreaterThan(0)
            .WithMessage("New tariff ID must be greater than 0.");

        RuleFor(x => x.DiscountRate)
            .InclusiveBetween(0m, 1m)
            .WithMessage("Discount rate must be between 0 and 1 (0% to 100%).");

        RuleFor(x => x.ContractNumber)
            .MaximumLength(100)
            .When(x => x.ContractNumber != null)
            .WithMessage("Contract number cannot exceed 100 characters.");

        RuleFor(x => x.Notes)
            .MaximumLength(1000)
            .When(x => x.Notes != null)
            .WithMessage("Notes cannot exceed 1000 characters.");

        RuleFor(x => x.ContractEndDate)
            .GreaterThan(x => x.ContractStartDate)
            .When(x => x.ContractStartDate.HasValue && x.ContractEndDate.HasValue)
            .WithMessage("Contract end date must be after the start date.");
    }
}

public class ChangeFacilityTariffCommandHandler : IRequestHandler<ChangeFacilityTariffCommand, int>
{
    private readonly IApplicationDbContext _context;

    public ChangeFacilityTariffCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<int> Handle(ChangeFacilityTariffCommand request, CancellationToken cancellationToken)
    {
        // Use a transaction to ensure atomicity
        await using var transaction = await _context.BeginTransactionAsync(cancellationToken);

        try
        {
            // Find the current assignment
            var currentAssignment = await _context.FacilityTariffs
                .FirstOrDefaultAsync(ft => ft.Id == request.CurrentAssignmentId, cancellationToken);
            Guard.Against.NotFound(request.CurrentAssignmentId, currentAssignment);

            // Check if the assignment is still active
            if (!currentAssignment.IsActive)
            {
                throw new InvalidOperationException(
                    $"Cannot change facility tariff assignment {request.CurrentAssignmentId} because it is already inactive. " +
                    "Please use the current active assignment.");
            }

            // Verify new tariff exists and is active
            var newTariff = await _context.Tariffs
                .FirstOrDefaultAsync(t => t.Id == request.NewTariffId, cancellationToken);
            Guard.Against.NotFound(request.NewTariffId, newTariff);

            if (!newTariff.IsActive)
            {
                throw new InvalidOperationException(
                    $"Cannot assign tariff {request.NewTariffId} because it is not active. " +
                    "Please use an active tariff.");
            }

            // Check if trying to assign the same tariff
            if (currentAssignment.TariffId == request.NewTariffId)
            {
                throw new InvalidOperationException(
                    "The new tariff is the same as the current tariff. No change needed.");
            }

            // Deactivate the current assignment
            currentAssignment.IsActive = false;

            // Also deactivate any other active assignments for the same facility
            // (in case of data inconsistency)
            var otherActiveAssignments = await _context.FacilityTariffs
                .Where(ft => ft.Id != request.CurrentAssignmentId &&
                            ft.FacilityId == currentAssignment.FacilityId &&
                            ft.IsActive)
                .ToListAsync(cancellationToken);

            foreach (var assignment in otherActiveAssignments)
            {
                assignment.IsActive = false;
            }

            // Create the new assignment with contract details
            var newAssignment = new FacilityTariff
            {
                FacilityId = currentAssignment.FacilityId,
                TariffId = request.NewTariffId,
                IsActive = true,
                ContractNumber = request.ContractNumber,
                DiscountRate = request.DiscountRate,
                ContractStartDate = request.ContractStartDate,
                ContractEndDate = request.ContractEndDate,
                Notes = request.Notes
            };

            _context.FacilityTariffs.Add(newAssignment);
            await _context.SaveChangesAsync(cancellationToken);

            await transaction.CommitAsync(cancellationToken);

            return newAssignment.Id;
        }
        catch
        {
            await transaction.RollbackAsync(cancellationToken);
            throw;
        }
    }
}
