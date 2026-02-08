using Backend.Application.Common.Interfaces;
using Backend.Application.Common.Security;
using Backend.Domain.Events;

namespace Backend.Application.Facilities.Commands.DeleteFacility;
[Authorize(Roles = "Administrator,Tenant.Admin")]
[RequirePermission(Backend.Domain.Constants.Permissions.FacilityDelete)]
public record DeleteFacilityCommand(int Id) : IRequest;

public class DeleteFacilityCommandHandler : IRequestHandler<DeleteFacilityCommand>
{
    private readonly IApplicationDbContext _context;

    public DeleteFacilityCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task Handle(DeleteFacilityCommand request, CancellationToken cancellationToken)
    {
        var entity = await _context.Facilities
            .FindAsync(new object[] { request.Id }, cancellationToken);

        Guard.Against.NotFound(request.Id, entity);

        _context.Facilities.Remove(entity);

        entity.AddDomainEvent(new FacilityDeletedEvent(entity));

        await _context.SaveChangesAsync(cancellationToken);
    }

}
