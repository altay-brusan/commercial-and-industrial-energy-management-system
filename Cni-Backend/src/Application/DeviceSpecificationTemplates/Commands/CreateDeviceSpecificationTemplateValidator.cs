namespace Backend.Application.DeviceSpecificationTemplates.Commands;

public class CreateDeviceSpecificationTemplateCommandValidator : AbstractValidator<CreateDeviceSpecificationTemplateCommand>
{
    public CreateDeviceSpecificationTemplateCommandValidator()
    {
        RuleFor(x => x.DeviceTypeId)
            .NotEmpty();

        RuleFor(x => x.Model)
            .NotEmpty()
            .MaximumLength(200);

        RuleFor(x => x.Manufacturer)
            .MaximumLength(200);

        RuleFor(x => x.SpecificationsTemplate)
            .NotEmpty();

        RuleFor(x => x.MetadataTemplate)
            .NotEmpty();
    }
}
