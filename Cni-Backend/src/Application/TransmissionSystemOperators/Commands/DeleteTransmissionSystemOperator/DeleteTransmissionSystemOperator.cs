using Backend.Application.Common.Interfaces;
using Backend.Application.Common.Security;
using Backend.Domain.Events;

namespace Backend.Application.TransmissionSystemOperators.Commands.DeleteTransmissionSystemOperator;

[Authorize(Roles = "Administrator,Tenant.Admin")]
public record DeleteTransmissionSystemOperatorCommand(int Id) : IRequest;

public class DeleteTransmissionSystemOperatorCommandHandler : IRequestHandler<DeleteTransmissionSystemOperatorCommand>
{
    private readonly IApplicationDbContext _context;

    public DeleteTransmissionSystemOperatorCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task Handle(DeleteTransmissionSystemOperatorCommand request, CancellationToken cancellationToken)
    {
        var entity = await _context.TransmissionSystemOperators
            .FindAsync(new object[] { request.Id }, cancellationToken);

        Guard.Against.NotFound(request.Id, entity);

        _context.TransmissionSystemOperators.Remove(entity);

        entity.AddDomainEvent(new TransmissionSystemOperatorDeletedEvent(entity));

        await _context.SaveChangesAsync(cancellationToken);
    }
}
