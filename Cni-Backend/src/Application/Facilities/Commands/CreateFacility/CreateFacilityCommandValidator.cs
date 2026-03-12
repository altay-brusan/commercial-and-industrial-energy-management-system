using Backend.Domain.Constants;
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

        RuleFor(v => v.Country)
            .NotEmpty()
            .MaximumLength(200)
            .Must(Countries.IsValidName)
            .WithMessage("Invalid country name.");

        RuleFor(v => v.Province)
            .NotEmpty()
            .MaximumLength(200);

        RuleFor(v => v.Province)
            .Must(TurkeyProvinces.IsValid)
            .When(v => string.Equals(v.Country, "Türkiye", StringComparison.OrdinalIgnoreCase))
            .WithMessage("Invalid Turkish province code.");

        RuleFor(v => v.City)
            .NotEmpty()
            .MaximumLength(200);

        RuleFor(v => v.Address)
            .MaximumLength(1000);

        RuleFor(v => v.EnergyCapacity)
            .GreaterThanOrEqualTo(0);

        RuleFor(v => v.EnergyUnit)
            .IsInEnum();
    }
}
