using Backend.Domain.Enums;

namespace Backend.Application.Facilities.Commands.UpdateFacility;

public class UpdateFacilityCommandValidator : AbstractValidator<UpdateFacilityCommand>
{
    public UpdateFacilityCommandValidator()
    {
        RuleFor(v => v.Name)
            .NotEmpty()
            .MaximumLength(500);

        RuleFor(v => v.Address)
            .MaximumLength(1000);

        RuleFor(v => v.EnergyCapacity)
            .GreaterThanOrEqualTo(0);

        RuleFor(v => v.EnergyUnit)
            .IsInEnum();
    }
}
