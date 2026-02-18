namespace Backend.Application.Tariffs.Commands.DeactivateTariff;

public class DeactivateTariffCommandValidator : AbstractValidator<DeactivateTariffCommand>
{
    public DeactivateTariffCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty()
            .WithMessage("Tariff ID is required.");
    }
}
