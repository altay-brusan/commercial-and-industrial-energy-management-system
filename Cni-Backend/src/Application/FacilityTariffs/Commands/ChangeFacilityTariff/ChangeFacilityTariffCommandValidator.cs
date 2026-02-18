namespace Backend.Application.FacilityTariffs.Commands.ChangeFacilityTariff;

public class ChangeFacilityTariffCommandValidator : AbstractValidator<ChangeFacilityTariffCommand>
{
    public ChangeFacilityTariffCommandValidator()
    {
        RuleFor(x => x.CurrentAssignmentId)
            .NotEmpty()
            .WithMessage("Current assignment ID is required.");

        RuleFor(x => x.NewTariffId)
            .NotEmpty()
            .WithMessage("New tariff ID is required.");

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
