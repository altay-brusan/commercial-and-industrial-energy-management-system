namespace Backend.Application.Plans.Commands.UpdatePlan;

public class UpdatePlanCommandValidator : AbstractValidator<UpdatePlanCommand>
{
    public UpdatePlanCommandValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Modules).MaximumLength(500);
        RuleFor(x => x.MaxFacilities).GreaterThanOrEqualTo(0);
        RuleFor(x => x.MaxUsers).GreaterThanOrEqualTo(0);
        RuleFor(x => x.Price).GreaterThanOrEqualTo(0);
    }
}
