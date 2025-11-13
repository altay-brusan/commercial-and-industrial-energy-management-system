using Backend.Application.Common.Interfaces;
using Backend.Application.Common.Security;

namespace Backend.Application.Facilities.Commands.UpdateFacility;
[Authorize(Roles = "Administrator,Tenant.Admin")]
public record UpdateFacilityCommand : IRequest
{
    public int Id { get; set; }
    public int FirmId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public int Capacity { get; set; }
    public double Longitude { get; set; }
    public double Latitude { get; set; }
}

public class UpdateFacilityCommandValidator : AbstractValidator<UpdateFacilityCommand>
{
    public UpdateFacilityCommandValidator()
    {
        RuleFor(v => v.Name)
             .NotEmpty()
             .MaximumLength(500);
        RuleFor(v => v.Address).MaximumLength(1000);
        RuleFor(v => v.Capacity).GreaterThanOrEqualTo(0);
    }
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
        entity.Capacity = request.Capacity;
        entity.Latitude = request.Latitude;
        entity.Longitude = request.Longitude;
        await _context.SaveChangesAsync(cancellationToken);
    }
}
