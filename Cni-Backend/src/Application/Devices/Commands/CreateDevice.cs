using Backend.Application.Common.Interfaces;
using Backend.Application.Common.Security;
using Backend.Domain.Entities;
using Backend.Domain.Enums;
using Backend.Domain.Events;

namespace Backend.Application.Devices.Commands;

[Authorize(Roles = "Administrator")]
[RequirePermission(Backend.Domain.Constants.Permissions.DeviceCreate)]
public record CreateDeviceCommand : IRequest<Guid>
{
    public Guid RtuId { get; init; }
    public Guid DeviceTypeId { get; init; }
    public Guid? ParentDeviceId { get; init; }
    public string Name { get; init; } = string.Empty;
    public string Model { get; init; } = string.Empty;
    public string SerialNumber { get; init; } = string.Empty;
    public string? Manufacturer { get; init; }
    public string Ports { get; init; } = "[]";
    public string? Metadata { get; init; }
    public Guid? DeviceSpecificationTemplateId { get; init; }
    public Guid? LogicalDeviceId { get; init; }
}

public class CreateDeviceCommandHandler : IRequestHandler<CreateDeviceCommand, Guid>
{
    private readonly IApplicationDbContext _context;

    public CreateDeviceCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Guid> Handle(CreateDeviceCommand request, CancellationToken cancellationToken)
    {
        var entity = new Device
        {
            RtuId = request.RtuId,
            DeviceTypeId = request.DeviceTypeId,
            ParentDeviceId = request.ParentDeviceId,
            Name = request.Name,
            Model = request.Model,
            SerialNumber = request.SerialNumber,
            Manufacturer = request.Manufacturer,
            Ports = request.Ports,
            Metadata = request.Metadata,
            DeviceSpecificationTemplateId = request.DeviceSpecificationTemplateId,
            LogicalDeviceId = request.LogicalDeviceId
        };

        entity.AddDomainEvent(new DeviceCreatedEvent(entity));

        _context.Devices.Add(entity);

        await _context.SaveChangesAsync(cancellationToken);

        return entity.Id;
    }
}
