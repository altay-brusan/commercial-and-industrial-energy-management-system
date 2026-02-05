namespace Backend.Application.FacilityTariffs.Commands.AssignTariffToFacility;

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
