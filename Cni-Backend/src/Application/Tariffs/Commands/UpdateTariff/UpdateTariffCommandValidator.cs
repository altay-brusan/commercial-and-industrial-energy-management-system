namespace Backend.Application.Tariffs.Commands.UpdateTariff;

public class UpdateTariffCommandValidator : AbstractValidator<UpdateTariffCommand>
{
    public UpdateTariffCommandValidator()
    {
        RuleFor(x => x.Id)
            .GreaterThan(0)
            .WithMessage("Tariff ID must be greater than 0.");

        RuleFor(x => x.CountryName)
            .NotEmpty()
            .MaximumLength(100)
            .WithMessage("Country name is required and must be at most 100 characters.");

        RuleFor(x => x.DistributorName)
            .MaximumLength(200)
            .WithMessage("Distributor name must be at most 200 characters.");

        RuleFor(x => x.DistributionFee)
            .GreaterThanOrEqualTo(0)
            .WithMessage("Distribution fee must be non-negative.");

        RuleFor(x => x.TransmissionFee)
            .GreaterThanOrEqualTo(0)
            .WithMessage("Transmission fee must be non-negative.");

        RuleFor(x => x.TaxRate)
            .InclusiveBetween(0, 1)
            .WithMessage("Tax rate must be between 0 and 1 (e.g., 0.21 for 21%).");

        RuleFor(x => x.Currency)
            .NotEmpty()
            .MaximumLength(10)
            .WithMessage("Currency is required and must be at most 10 characters.");
    }
}
