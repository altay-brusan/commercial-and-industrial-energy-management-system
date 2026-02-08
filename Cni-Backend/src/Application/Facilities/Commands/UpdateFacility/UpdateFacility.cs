using Backend.Application.Common.Interfaces;
using Backend.Application.Common.Security;
using Backend.Domain.Enums;

namespace Backend.Application.Facilities.Commands.UpdateFacility;

[Authorize(Roles = "Administrator,Tenant.Admin")]
[RequirePermission(Backend.Domain.Constants.Permissions.FacilityEdit)]
public record UpdateFacilityCommand : IRequest
{
    public int Id { get; set; }
    public int FirmId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public int EnergyCapacity { get; set; }
    public EnergyUnit EnergyUnit { get; set; } = EnergyUnit.KWh;
    public double Longitude { get; set; }
    public double Latitude { get; set; }
    public string? Topology { get; set; }
}

public class UpdateFacilityCommandHandler : IRequestHandler<UpdateFacilityCommand>
{
    private readonly IApplicationDbContext _context;

    public UpdateFacilityCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task Handle(UpdateFacilityCommand request, CancellationToken cancellationToken)
    {
        var entity = await _context.Facilities
            .FindAsync(new object[] { request.Id }, cancellationToken);

        Guard.Against.NotFound(request.Id, entity);

        entity.Name = request.Name ?? entity.Name;
        entity.FirmId = request.FirmId;
        entity.Address = request.Address ?? entity.Address;
        entity.EnergyCapacity = request.EnergyCapacity;
        entity.EnergyUnit = request.EnergyUnit;
        entity.Latitude = request.Latitude;
        entity.Longitude = request.Longitude;
        entity.Topology = request.Topology;

        await _context.SaveChangesAsync(cancellationToken);
    }
}
