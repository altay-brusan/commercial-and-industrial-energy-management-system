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

        RuleFor(v => v.CountryCode)
            .NotEmpty()
            .Length(2)
            .Must(Countries.IsValid)
            .WithMessage("Invalid ISO 3166-1 alpha-2 country code.");

        RuleFor(v => v.Province)
            .NotEmpty()
            .MaximumLength(200);

        RuleFor(v => v.Province)
            .Must(TurkeyProvinces.IsValid)
            .When(v => v.CountryCode == "TR")
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
