namespace Backend.Application.Tariffs.Queries.GetActiveTariffByCountry;

public class GetActiveTariffByCountryQueryValidator : AbstractValidator<GetActiveTariffByCountryQuery>
{
    public GetActiveTariffByCountryQueryValidator()
    {
        RuleFor(x => x.CountryName)
            .NotEmpty()
            .MaximumLength(100)
            .WithMessage("Country name is required and must be at most 100 characters.");
    }
}
