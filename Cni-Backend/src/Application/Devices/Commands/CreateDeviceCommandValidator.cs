namespace Backend.Application.Devices.Commands;

public class CreateDeviceCommandValidator : AbstractValidator<CreateDeviceCommand>
{
    public CreateDeviceCommandValidator()
    {
        RuleFor(x => x.FacilityId)
            .NotEmpty()
            .WithMessage("FacilityId is required.");
        RuleFor(x => x.DeviceTypeId)
            .NotEmpty()
            .WithMessage("DeviceTypeId is required.");
        RuleFor(x => x.Name)
            .NotEmpty()
            .MaximumLength(100)
            .WithMessage("Device name is required.");
        RuleFor(x => x.Model)
            .NotEmpty()
            .MaximumLength(200)
            .WithMessage("Device model is required.");
        RuleFor(x => x.SerialNumber)
            .NotEmpty()
            .MaximumLength(100)
            .WithMessage("Serial number is required.");
        RuleFor(x => x.Manufacturer)
            .MaximumLength(200);
    }
}
