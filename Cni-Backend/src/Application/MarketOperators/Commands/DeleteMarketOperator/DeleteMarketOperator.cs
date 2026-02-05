using Backend.Application.Common.Interfaces;
using Backend.Application.Common.Security;
using Backend.Domain.Events;

namespace Backend.Application.MarketOperators.Commands.DeleteMarketOperator;

[Authorize(Roles = "Administrator")]
public record DeleteMarketOperatorCommand(int Id) : IRequest;

public class DeleteMarketOperatorCommandHandler : IRequestHandler<DeleteMarketOperatorCommand>
{
    private readonly IApplicationDbContext _context;

    public DeleteMarketOperatorCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task Handle(DeleteMarketOperatorCommand request, CancellationToken cancellationToken)
    {
        var entity = await _context.MarketOperators
            .FindAsync(new object[] { request.Id }, cancellationToken);

        Guard.Against.NotFound(request.Id, entity);

        entity.AddDomainEvent(new MarketOperatorDeletedEvent(entity));

        _context.MarketOperators.Remove(entity);

        await _context.SaveChangesAsync(cancellationToken);
    }
}
