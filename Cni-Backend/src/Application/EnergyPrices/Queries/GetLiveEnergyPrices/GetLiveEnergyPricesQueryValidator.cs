namespace Backend.Application.EnergyPrices.Queries.GetLiveEnergyPrices;

public class GetLiveEnergyPricesQueryValidator : AbstractValidator<GetLiveEnergyPricesQuery>
{
    public GetLiveEnergyPricesQueryValidator()
    {
        RuleFor(v => v.FirmId)
            .GreaterThan(0)
            .WithMessage("FirmId is required.");

        RuleFor(v => v.Period)
            .Must(p => p == null || new[] { 15, 30, 60 }.Contains(p.Value))
            .WithMessage("Period must be 15, 30, or 60 minutes");
    }
}
