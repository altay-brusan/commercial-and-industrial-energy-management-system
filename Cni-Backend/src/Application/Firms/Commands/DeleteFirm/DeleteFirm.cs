using Backend.Application.Common.Interfaces;
using Backend.Application.Common.Security;
using Backend.Domain.Events;

namespace Backend.Application.Firms.Commands.DeleteFirm;
[Authorize(Roles = "Administrator,Tenant.Admin")]
public record DeleteFirmCommand(int Id) : IRequest;

public class DeleteFirmCommandHandler : IRequestHandler<DeleteFirmCommand>
{
    private readonly IApplicationDbContext _context;

    public DeleteFirmCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task Handle(DeleteFirmCommand request, CancellationToken cancellationToken)
    {
        var entity = await _context.Firms
            .FindAsync(new object[] { request.Id }, cancellationToken);

        Guard.Against.NotFound(request.Id, entity);

        _context.Firms.Remove(entity);

        entity.AddDomainEvent(new FirmDeletedEvent(entity));

        await _context.SaveChangesAsync(cancellationToken);
    }

}
