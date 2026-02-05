namespace Backend.Application.Firms.Commands.CreateFirm;

public class CreateFirmCommandValidator : AbstractValidator<CreateFirmCommand>
{
    public CreateFirmCommandValidator()
    {
        RuleFor(v => v.Name)
            .NotEmpty()
            .MaximumLength(500);

        RuleFor(v => v.LogoUrl)
            .MaximumLength(1000);

        RuleFor(v => v.Type)
            .MaximumLength(100);

        RuleFor(v => v.Country)
            .MaximumLength(100)
            .NotEmpty();

        RuleFor(v => v.State)
            .MaximumLength(100)
            .NotEmpty();

        RuleFor(v => v.City)
            .MaximumLength(100)
            .NotEmpty();

        RuleFor(v => v.District)
            .MaximumLength(100);

        RuleFor(v => v.PostalCode)
            .MaximumLength(20);

        RuleFor(v => v.Address)
            .NotEmpty()
            .MaximumLength(1000);

        RuleFor(v => v.ContactEmail)
            .NotEmpty()
            .MaximumLength(255)
            .EmailAddress();

        RuleFor(v => v.ContactPhone)
            .NotEmpty()
            .MaximumLength(50);

        RuleFor(v => v.Fax)
            .MaximumLength(50);

        RuleFor(v => v.TaxNumber)
            .NotEmpty()
            .MaximumLength(50);

        RuleFor(v => v.TaxOffice)
            .NotEmpty()
            .MaximumLength(100);
    }
}
