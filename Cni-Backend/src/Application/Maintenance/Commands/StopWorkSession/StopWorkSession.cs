using Backend.Application.Common.Interfaces;
using Backend.Application.Common.Security;
using Backend.Domain.Entities;

namespace Backend.Application.Maintenance.Commands.StopWorkSession;

[Authorize(Roles = "Administrator,Tenant.Admin")]
[RequirePermission(Backend.Domain.Constants.Permissions.MaintenanceManage)]
public record StopWorkSessionCommand : IRequest
{
    public Guid TaskId { get; init; }
}

public class StopWorkSessionCommandHandler : IRequestHandler<StopWorkSessionCommand>
{
    private readonly IApplicationDbContext _context;
    private readonly IUser _user;

    public StopWorkSessionCommandHandler(IApplicationDbContext context, IUser user)
    {
        _context = context;
        _user = user;
    }

    public async Task Handle(StopWorkSessionCommand request, CancellationToken cancellationToken)
    {
        var session = await _context.TaskWorkSessions
            .FirstOrDefaultAsync(w => w.MaintenanceTaskId == request.TaskId && w.UserId == _user.Id && w.EndedAt == null, cancellationToken);

        if (session == null)
            throw new NotFoundException(nameof(TaskWorkSession), $"open session for task {request.TaskId}");

        session.EndedAt = DateTimeOffset.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);
    }
}
