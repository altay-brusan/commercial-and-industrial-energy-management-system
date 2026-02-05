namespace Backend.Application.FacilityTariffs.Queries.GetFacilityTariffs;

public class GetFacilityTariffsQueryValidator : AbstractValidator<GetFacilityTariffsQuery>
{
    public GetFacilityTariffsQueryValidator()
    {
        RuleFor(x => x.PageNumber)
            .GreaterThanOrEqualTo(1)
            .WithMessage("PageNumber must be at least 1.");

        RuleFor(x => x.PageSize)
            .GreaterThanOrEqualTo(1)
            .LessThanOrEqualTo(100)
            .WithMessage("PageSize must be between 1 and 100.");
    }
}
