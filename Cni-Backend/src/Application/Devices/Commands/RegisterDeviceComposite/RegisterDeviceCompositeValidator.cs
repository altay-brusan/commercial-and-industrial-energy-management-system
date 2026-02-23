namespace Backend.Application.Devices.Commands.RegisterDeviceComposite;

public class RegisterDeviceCompositeCommandValidator : AbstractValidator<RegisterDeviceCompositeCommand>
{
    public RegisterDeviceCompositeCommandValidator()
    {
        RuleFor(x => x.RtuId)
            .NotEmpty();

        RuleFor(x => x.TemplateId)
            .NotEmpty();

        RuleFor(x => x.Name)
            .NotEmpty()
            .MaximumLength(100);

        RuleFor(x => x.SerialNumber)
            .NotEmpty()
            .MaximumLength(100);
    }
}
