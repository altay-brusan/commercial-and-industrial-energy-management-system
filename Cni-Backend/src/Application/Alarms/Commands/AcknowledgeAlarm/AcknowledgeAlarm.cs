using Backend.Application.Alarms.Queries;
using Backend.Application.Common.Interfaces;
using Backend.Application.Common.Security;
using Backend.Domain.Entities;
using FluentValidation.Results;

namespace Backend.Application.Alarms.Commands.AcknowledgeAlarm;

[Authorize(Roles = "Administrator,Tenant.Admin")]
[RequirePermission(Backend.Domain.Constants.Permissions.AlarmManage)]
public record AcknowledgeAlarmCommand : IRequest<AlarmDto>
{
    public Guid Id { get; init; }
}

public class AcknowledgeAlarmCommandHandler : IRequestHandler<AcknowledgeAlarmCommand, AlarmDto>
{
    private readonly IApplicationDbContext _context;
    private readonly IUser _user;

    public AcknowledgeAlarmCommandHandler(IApplicationDbContext context, IUser user)
    {
        _context = context;
        _user = user;
    }

    public async Task<AlarmDto> Handle(AcknowledgeAlarmCommand request, CancellationToken cancellationToken)
    {
        var alarm = await _context.DeviceAlarms
            .FirstOrDefaultAsync(a => a.Id == request.Id, cancellationToken);

        if (alarm == null)
            throw new NotFoundException(nameof(DeviceAlarm), request.Id.ToString());

        if (!alarm.IsActive)
            throw new Common.Exceptions.ValidationException(new[] { new ValidationFailure("Id", "Alarm is not active.") });

        if (alarm.AcknowledgedAt != null)
            throw new Common.Exceptions.ValidationException(new[] { new ValidationFailure("Id", "Alarm is already acknowledged.") });

        alarm.AcknowledgedAt = DateTimeOffset.UtcNow;
        alarm.AcknowledgedById = _user.Id;

        await _context.SaveChangesAsync(cancellationToken);

        // Re-query with full includes to build the response DTO
        var updated = await _context.DeviceAlarms
            .Include(a => a.AlarmType)
            .Include(a => a.Device)
                .ThenInclude(d => d!.DeviceType)
            .Include(a => a.Device)
                .ThenInclude(d => d!.Rtu)
                    .ThenInclude(r => r!.Facility)
            .Include(a => a.AcknowledgedBy)
            .Include(a => a.ResolvedBy)
            .FirstAsync(a => a.Id == request.Id, cancellationToken);

        return AlarmDtoMapper.ToDto(updated);
    }
}
