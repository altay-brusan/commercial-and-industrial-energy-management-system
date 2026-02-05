namespace Backend.Application.Plans.Commands.CreatePlan;

public class CreatePlanCommandValidator : AbstractValidator<CreatePlanCommand>
{
    public CreatePlanCommandValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Modules).MaximumLength(500);
        RuleFor(x => x.MaxFacilities).GreaterThanOrEqualTo(0);
        RuleFor(x => x.MaxUsers).GreaterThanOrEqualTo(0);
        RuleFor(x => x.Price).GreaterThanOrEqualTo(0);
    }
}
