using Backend.Application.Common.Interfaces;
using Backend.Application.Common.Security;
using Backend.Domain.Entities;
using Backend.Domain.Enums;
using Backend.Domain.Events;

namespace Backend.Application.Devices.Commands;

[Authorize(Roles = "Administrator")]
public record CreateDeviceCommand : IRequest<int>
{
    public int FacilityId { get; init; }
    public int DeviceTypeId { get; init; }
    public string Name { get; init; } = string.Empty;
    public string Model { get; init; } = string.Empty;
    public string SerialNumber { get; init; } = string.Empty;
    public string? Manufacturer { get; init; }
    public string Ports { get; init; } = "[]";
}

public class CreateDeviceCommandValidator : AbstractValidator<CreateDeviceCommand>
{
    public CreateDeviceCommandValidator()
    {
        RuleFor(x => x.FacilityId)
            .GreaterThan(0)
            .WithMessage("FacilityId is required.");
        RuleFor(x => x.DeviceTypeId)
            .GreaterThan(0)
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

public class CreateDeviceCommandHandler : IRequestHandler<CreateDeviceCommand, int>
{
    private readonly IApplicationDbContext _context;

    public CreateDeviceCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<int> Handle(CreateDeviceCommand request, CancellationToken cancellationToken)
    {
        var entity = new Device
        {
            FacilityId = request.FacilityId,
            DeviceTypeId = request.DeviceTypeId,
            Name = request.Name,
            Model = request.Model,
            SerialNumber = request.SerialNumber,
            Manufacturer = request.Manufacturer,
            Ports = request.Ports
        };

        entity.AddDomainEvent(new DeviceCreatedEvent(entity));

        _context.Devices.Add(entity);

        await _context.SaveChangesAsync(cancellationToken);

        return entity.Id;
    }
}
