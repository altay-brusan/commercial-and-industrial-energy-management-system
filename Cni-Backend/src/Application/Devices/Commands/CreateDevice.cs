
using Backend.Application.Common.Interfaces;
using Backend.Application.Common.Security;
using Backend.Domain.Entities;
using Backend.Domain.Enums;
using Backend.Domain.Events;

namespace Backend.Application.Devices.Commands;
internal class CreateDevice
{
}
[Authorize(Roles = "Administrator")]
public record CreateDeviceCommand : IRequest<int>
{
    public required string MachineId { get; init; }
    public int TenantId { get; init; }
    public DeviceTypeEnum Type { get; init; }
    public string? DeviceName { get; init; }
    public int FacilityId { get; init; }
}

public class CreateDeviceCommandValidator : AbstractValidator<CreateDeviceCommand>
{
    public CreateDeviceCommandValidator()
    {
        RuleFor(x => x.TenantId).NotEmpty().WithMessage("TenantId is required.");
        RuleFor(x => x.Type).IsInEnum().WithMessage("Invalid node type.");
        RuleFor(x => x.FacilityId).NotEmpty().WithMessage("FacilityId is required.");
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
            MachineId = request.MachineId,
            TenantId = request.TenantId,
            DeviceTypeId = (int)request.Type,
            DeviceName = request.DeviceName,
            GrpcDeviceStatus = 0,
            GrpcConnection = false,
            FacilityId = request.FacilityId
        };

        entity.AddDomainEvent(new DeviceCreatedEvent(entity));

        _context.Devices.Add(entity);

        await _context.SaveChangesAsync(cancellationToken);

        return entity.Id;
    }
}
