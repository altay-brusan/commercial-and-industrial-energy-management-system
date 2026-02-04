using Backend.Application.Common.Interfaces;
using Backend.Application.Common.Security;
using Backend.Domain.Events;

namespace Backend.Application.TenantMarketOperators.Commands.DeleteTenantMarketOperator;

[Authorize(Roles = "Administrator,Tenant.Admin")]
public record DeleteTenantMarketOperatorCommand(int Id) : IRequest;

public class DeleteTenantMarketOperatorCommandHandler : IRequestHandler<DeleteTenantMarketOperatorCommand>
{
    private readonly IApplicationDbContext _context;

    public DeleteTenantMarketOperatorCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task Handle(DeleteTenantMarketOperatorCommand request, CancellationToken cancellationToken)
    {
        var entity = await _context.TenantMarketOperators
            .FindAsync(new object[] { request.Id }, cancellationToken);

        Guard.Against.NotFound(request.Id, entity);

        entity.AddDomainEvent(new TenantMarketOperatorDeletedEvent(entity));

        _context.TenantMarketOperators.Remove(entity);

        await _context.SaveChangesAsync(cancellationToken);
    }
}
