using Backend.Application.Common.Interfaces;
using Backend.Application.Common.Security;
using Backend.Domain.Events;

namespace Backend.Application.Plans.Commands.DeletePlan;
[Authorize(Roles = "Administrator")]
public record DeletePlanCommand(int Id) : IRequest;

public class DeletePlanCommandHandler : IRequestHandler<DeletePlanCommand>
{
    private readonly IApplicationDbContext _context;

    public DeletePlanCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task Handle(DeletePlanCommand request, CancellationToken cancellationToken)
    {
        var entity = await _context.Plans
            .FindAsync(new object[] { request.Id }, cancellationToken);

        Guard.Against.NotFound(request.Id, entity);

        _context.Plans.Remove(entity);

        entity.AddDomainEvent(new PlanDeletedEvent(entity));

        await _context.SaveChangesAsync(cancellationToken);
    }

}
