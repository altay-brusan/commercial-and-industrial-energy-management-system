using Backend.Application.Common.Interfaces;
using Backend.Application.Common.Security;
using Backend.Domain.Entities;
using Backend.Domain.Events;

namespace Backend.Application.Facilities.Commands.CreateFacility;
[Authorize(Roles = "Administrator,Tenant.Admin")]
public record CreateFacilityCommand : IRequest<int>
{
    public int TenantId { get; set; }
    public int FirmId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public int Capacity { get; set; }
    public double Longitude { get; set; }
    public double Latitude { get; set; }

}

public class CreateFacilityCommandValidator : AbstractValidator<CreateFacilityCommand>
{
    public CreateFacilityCommandValidator()
    {
        RuleFor(v => v.Name)
             .NotEmpty()
             .MaximumLength(500);
        RuleFor(v => v.Address).MaximumLength(1000);
        RuleFor(v=> v.Capacity).GreaterThanOrEqualTo(0);
    }
}

public class CreateFacilityCommandHandler : IRequestHandler<CreateFacilityCommand, int>
{
    private readonly IApplicationDbContext _context;
    private readonly IMultiTenantService _multiTenantService;

    public CreateFacilityCommandHandler(IApplicationDbContext context, IMultiTenantService multiTenantService)
    {
        _context = context;
        _multiTenantService = multiTenantService;
    }

    public async Task<int> Handle(CreateFacilityCommand request, CancellationToken cancellationToken)
    {
        var tenantId = _multiTenantService.GetTenantIdForCreation(request.TenantId > 0 ? request.TenantId : null);

        var entity = new Facility
        {
            TenantId = tenantId,
            FirmId = request.FirmId,
            Name = request.Name,
            Address = request.Address,
            Capacity = request.Capacity,
            Longitude = request.Longitude,
            Latitude = request.Latitude
        };
        entity.AddDomainEvent(new FacilityCreatedEvent(entity));

        _context.Facilities.Add(entity);

        await _context.SaveChangesAsync(cancellationToken);
        return entity.Id;
    }
}
