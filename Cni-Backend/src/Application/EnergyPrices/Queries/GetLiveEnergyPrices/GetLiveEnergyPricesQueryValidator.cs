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
            .WithMessage("Period must be 15, 30, or 60 minutes.");

        RuleFor(v => v.EndDate)
            .GreaterThanOrEqualTo(v => v.StartDate)
            .When(v => v.StartDate.HasValue && v.EndDate.HasValue)
            .WithMessage("End date must be greater than or equal to start date.");

        // Limit date range to prevent excessive API calls (max 7 days)
        RuleFor(v => v)
            .Must(v => !v.StartDate.HasValue || !v.EndDate.HasValue ||
                       (v.EndDate.Value - v.StartDate.Value).TotalDays <= 7)
            .WithMessage("Date range cannot exceed 7 days to prevent excessive API calls.");
    }
}
