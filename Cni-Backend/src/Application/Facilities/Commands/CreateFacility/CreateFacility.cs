using Backend.Application.Common.Interfaces;
using Backend.Application.Common.Security;
using Backend.Domain.Entities;
using Backend.Domain.Enums;
using Backend.Domain.Events;

namespace Backend.Application.Facilities.Commands.CreateFacility;

[Authorize(Roles = "Administrator,Tenant.Admin")]
[RequirePermission(Backend.Domain.Constants.Permissions.FacilityCreate)]
public record CreateFacilityCommand : IRequest<int>
{
    public int FirmId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public int EnergyCapacity { get; set; }
    public EnergyUnit EnergyUnit { get; set; } = EnergyUnit.KWh;
    public double Longitude { get; set; }
    public double Latitude { get; set; }
}

public class CreateFacilityCommandHandler : IRequestHandler<CreateFacilityCommand, int>
{
    private readonly IApplicationDbContext _context;

    public CreateFacilityCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<int> Handle(CreateFacilityCommand request, CancellationToken cancellationToken)
    {
        var entity = new Facility
        {
            FirmId = request.FirmId,
            Name = request.Name,
            Address = request.Address,
            EnergyCapacity = request.EnergyCapacity,
            EnergyUnit = request.EnergyUnit,
            Longitude = request.Longitude,
            Latitude = request.Latitude
        };
        entity.AddDomainEvent(new FacilityCreatedEvent(entity));

        _context.Facilities.Add(entity);

        await _context.SaveChangesAsync(cancellationToken);
        return entity.Id;
    }
}
