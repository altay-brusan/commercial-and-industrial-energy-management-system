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
