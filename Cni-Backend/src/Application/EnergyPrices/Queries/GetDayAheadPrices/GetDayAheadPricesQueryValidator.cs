namespace Backend.Application.EnergyPrices.Queries.GetDayAheadPrices;

public class GetDayAheadPricesQueryValidator : AbstractValidator<GetDayAheadPricesQuery>
{
    public GetDayAheadPricesQueryValidator()
    {
        RuleFor(v => v.FacilityId)
            .NotEmpty()
            .WithMessage("Facility ID is required.");

        RuleFor(v => v.StartDate)
            .NotEmpty()
            .WithMessage("Start date is required.");

        RuleFor(v => v.EndDate)
            .GreaterThanOrEqualTo(v => v.StartDate)
            .When(v => v.EndDate.HasValue)
            .WithMessage("End date must be greater than or equal to start date.");
    }
}
