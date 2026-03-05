using Backend.Application.Common.Exceptions;
using Backend.Application.Common.Interfaces;
using Backend.Application.Common.Security;
using Backend.Domain.Entities;
using FluentValidation.Results;

namespace Backend.Application.Alarms.Commands.ResolveAlarm;

[Authorize(Roles = "Administrator,Tenant.Admin")]
[RequirePermission(Backend.Domain.Constants.Permissions.AlarmManage)]
public record ResolveAlarmCommand : IRequest
{
    public Guid Id { get; init; }
}

public class ResolveAlarmCommandHandler : IRequestHandler<ResolveAlarmCommand>
{
    private readonly IApplicationDbContext _context;
    private readonly IUser _user;

    public ResolveAlarmCommandHandler(IApplicationDbContext context, IUser user)
    {
        _context = context;
        _user = user;
    }

    public async Task Handle(ResolveAlarmCommand request, CancellationToken cancellationToken)
    {
        var alarm = await _context.DeviceAlarms
            .FirstOrDefaultAsync(a => a.Id == request.Id, cancellationToken);

        if (alarm == null)
            throw new NotFoundException(nameof(DeviceAlarm), request.Id.ToString());

        if (!alarm.IsActive)
            throw new Common.Exceptions.ValidationException(new[] { new ValidationFailure("Id", "Alarm is not active.") });

        alarm.IsActive = false;
        alarm.ResolvedAt = DateTimeOffset.UtcNow;
        alarm.ResolvedById = _user.Id;

        await _context.SaveChangesAsync(cancellationToken);
    }
}
