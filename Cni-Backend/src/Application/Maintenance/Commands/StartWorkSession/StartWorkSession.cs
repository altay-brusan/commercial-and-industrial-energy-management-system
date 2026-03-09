using Backend.Application.Common.Interfaces;
using Backend.Application.Common.Security;
using Backend.Domain.Entities;
using FluentValidation.Results;

namespace Backend.Application.Maintenance.Commands.StartWorkSession;

[Authorize(Roles = "Administrator,Tenant.Admin")]
[RequirePermission(Backend.Domain.Constants.Permissions.MaintenanceManage)]
public record StartWorkSessionCommand : IRequest<Guid>
{
    public Guid TaskId { get; init; }
}

public class StartWorkSessionCommandHandler : IRequestHandler<StartWorkSessionCommand, Guid>
{
    private readonly IApplicationDbContext _context;
    private readonly IUser _user;

    public StartWorkSessionCommandHandler(IApplicationDbContext context, IUser user)
    {
        _context = context;
        _user = user;
    }

    public async Task<Guid> Handle(StartWorkSessionCommand request, CancellationToken cancellationToken)
    {
        var taskExists = await _context.MaintenanceTasks
            .AnyAsync(t => t.Id == request.TaskId, cancellationToken);

        if (!taskExists)
            throw new NotFoundException(nameof(MaintenanceTask), request.TaskId.ToString());

        // Check for open session
        var hasOpenSession = await _context.TaskWorkSessions
            .AnyAsync(w => w.MaintenanceTaskId == request.TaskId && w.UserId == _user.Id && w.EndedAt == null, cancellationToken);

        if (hasOpenSession)
            throw new Common.Exceptions.ValidationException(
                new[] { new ValidationFailure("TaskId", "You already have an open work session on this task.") });

        var session = new TaskWorkSession
        {
            MaintenanceTaskId = request.TaskId,
            UserId = _user.Id!,
            StartedAt = DateTimeOffset.UtcNow
        };

        _context.TaskWorkSessions.Add(session);
        await _context.SaveChangesAsync(cancellationToken);

        return session.Id;
    }
}
