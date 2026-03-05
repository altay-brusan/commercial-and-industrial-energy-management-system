using Backend.Application.Common.Exceptions;
using Backend.Application.Common.Interfaces;
using Backend.Application.Common.Security;
using Backend.Domain.Constants;
using Backend.Domain.Entities;

namespace Backend.Application.Alarms.Queries.GetAlarm;

[Authorize(Roles = "Administrator,Tenant.Admin")]
[RequirePermission(Backend.Domain.Constants.Permissions.AlarmView)]
public record GetAlarmQuery : IRequest<AlarmDetailDto>
{
    public Guid Id { get; init; }
}

public class GetAlarmQueryHandler : IRequestHandler<GetAlarmQuery, AlarmDetailDto>
{
    private readonly IApplicationDbContext _context;

    public GetAlarmQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<AlarmDetailDto> Handle(GetAlarmQuery request, CancellationToken cancellationToken)
    {
        var alarm = await _context.DeviceAlarms
            .Include(a => a.AlarmType)
            .Include(a => a.Device)
                .ThenInclude(d => d!.Rtu)
                    .ThenInclude(r => r!.Facility)
            .Include(a => a.AcknowledgedBy)
            .Include(a => a.ResolvedBy)
            .Where(a => a.Id == request.Id)
            .Select(a => new AlarmDetailDto
            {
                Id = a.Id,
                DeviceId = a.DeviceId,
                DeviceAlarmTypeId = a.DeviceAlarmTypeId,
                Code = a.AlarmType!.Code,
                Name = a.AlarmType.Name,
                Message = a.AlarmType.Description,
                Severity = a.AlarmType.Severity,
                Status = !a.IsActive ? "Resolved" : (a.AcknowledgedAt != null ? "Acknowledged" : "Active"),
                Source = a.AlarmType.Source,
                Category = a.AlarmType.Category,
                DeviceName = a.Device != null ? a.Device.Name : null,
                DeviceModel = a.Device != null ? a.Device.Model : null,
                FacilityName = a.Device != null && a.Device.Rtu != null && a.Device.Rtu.Facility != null
                    ? a.Device.Rtu.Facility.Name : null,
                FacilityId = a.Device != null && a.Device.Rtu != null
                    ? a.Device.Rtu.FacilityId : null,
                OccurredAt = a.OccurredAt,
                RecoveredAt = a.RecoveredAt,
                AcknowledgedAt = a.AcknowledgedAt,
                ResolvedAt = a.ResolvedAt,
                AcknowledgedByName = a.AcknowledgedBy != null ? a.AcknowledgedBy.Name : null,
                ResolvedByName = a.ResolvedBy != null ? a.ResolvedBy.Name : null,
                MaintenanceTaskId = a.MaintenanceTaskId,
                HasMaintenanceTask = a.MaintenanceTaskId != null,
                Details = a.Details,
                IsActive = a.IsActive,
                IsRead = a.IsRead,
                AcknowledgedById = a.AcknowledgedById,
                ResolvedById = a.ResolvedById,
                Created = a.Created
            })
            .FirstOrDefaultAsync(cancellationToken);

        if (alarm == null)
            throw new NotFoundException(nameof(DeviceAlarm), request.Id.ToString());

        return alarm;
    }
}
