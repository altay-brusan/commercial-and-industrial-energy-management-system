using Backend.Application.Common.Interfaces;
using Backend.Application.Common.Security;
using Backend.Domain.Events;

namespace Backend.Application.Tenants.Commands.DeleteTenant;
[Authorize(Roles = "Administrator")]
public record DeleteTenantCommand(Guid Id) : IRequest;

public class DeleteTenantCommandHandler : IRequestHandler<DeleteTenantCommand>
{
    private readonly IApplicationDbContext _context;

    public DeleteTenantCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task Handle(DeleteTenantCommand request, CancellationToken cancellationToken)
    {
        var entity = await _context.Tenants
            .FindAsync(new object[] { request.Id }, cancellationToken);

        Guard.Against.NotFound(request.Id, entity);

        _context.Tenants.Remove(entity);

        entity.AddDomainEvent(new TenantDeletedEvent(entity));

        await _context.SaveChangesAsync(cancellationToken);
    }

}
