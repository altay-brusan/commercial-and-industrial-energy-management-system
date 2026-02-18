using Backend.Domain.Enums;

namespace Backend.Application.Facilities.Commands.CreateFacility;

public class CreateFacilityCommandValidator : AbstractValidator<CreateFacilityCommand>
{
    public CreateFacilityCommandValidator()
    {
        RuleFor(v => v.FirmId)
            .NotEmpty()
            .WithMessage("FirmId is required.");

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
