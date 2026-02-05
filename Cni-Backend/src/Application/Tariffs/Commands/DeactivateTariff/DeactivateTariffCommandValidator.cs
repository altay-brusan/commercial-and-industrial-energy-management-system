namespace Backend.Application.Tariffs.Commands.DeactivateTariff;

public class DeactivateTariffCommandValidator : AbstractValidator<DeactivateTariffCommand>
{
    public DeactivateTariffCommandValidator()
    {
        RuleFor(x => x.Id)
            .GreaterThan(0)
            .WithMessage("Tariff ID must be greater than 0.");
    }
}
