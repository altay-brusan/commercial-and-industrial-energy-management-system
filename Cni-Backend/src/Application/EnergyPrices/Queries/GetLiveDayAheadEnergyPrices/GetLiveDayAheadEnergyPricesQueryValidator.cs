namespace Backend.Application.EnergyPrices.Queries.GetLiveDayAheadEnergyPrices;

public class GetLiveDayAheadEnergyPricesQueryValidator : AbstractValidator<GetLiveDayAheadEnergyPricesQuery>
{
    private const int MaxDateRangeDays = 365; // 1 year

    public GetLiveDayAheadEnergyPricesQueryValidator()
    {
        RuleFor(v => v.FirmId)
            .GreaterThan(0)
            .WithMessage("FirmId is required.");

        RuleFor(v => v.EndDate)
            .GreaterThanOrEqualTo(v => v.StartDate)
            .When(v => v.StartDate.HasValue && v.EndDate.HasValue)
            .WithMessage("End date must be greater than or equal to start date.");

        // Limit date range to 1 year maximum
        RuleFor(v => v)
            .Must(v => !v.StartDate.HasValue || !v.EndDate.HasValue ||
                       (v.EndDate.Value - v.StartDate.Value).TotalDays <= MaxDateRangeDays)
            .WithMessage($"Date range cannot exceed {MaxDateRangeDays} days (1 year).");
    }
}
