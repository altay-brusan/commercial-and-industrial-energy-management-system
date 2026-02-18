using Backend.Application.Common.Interfaces;
using Backend.Application.Common.Security;
using Backend.Domain.Events;

namespace Backend.Application.FirmMarketOperators.Commands.DeleteFirmMarketOperator;

[Authorize(Roles = "Administrator,Tenant.Admin")]
public record DeleteFirmMarketOperatorCommand(Guid Id) : IRequest;

public class DeleteFirmMarketOperatorCommandHandler : IRequestHandler<DeleteFirmMarketOperatorCommand>
{
    private readonly IApplicationDbContext _context;

    public DeleteFirmMarketOperatorCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task Handle(DeleteFirmMarketOperatorCommand request, CancellationToken cancellationToken)
    {
        var entity = await _context.FirmMarketOperators
            .FindAsync(new object[] { request.Id }, cancellationToken);

        Guard.Against.NotFound(request.Id, entity);

        entity.AddDomainEvent(new FirmMarketOperatorDeletedEvent(entity));

        _context.FirmMarketOperators.Remove(entity);

        await _context.SaveChangesAsync(cancellationToken);
    }
}
