using Backend.Application.Common.Interfaces;
using Backend.Application.Common.Security;
using Backend.Domain.Entities;

namespace Backend.Application.Alarms.Queries.GetAlarm;

[Authorize(Roles = "Administrator,Tenant.Admin")]
[RequirePermission(Backend.Domain.Constants.Permissions.AlarmView)]
public record GetAlarmQuery : IRequest<AlarmDto>
{
    public Guid Id { get; init; }
}

public class GetAlarmQueryHandler : IRequestHandler<GetAlarmQuery, AlarmDto>
{
    private readonly IApplicationDbContext _context;

    public GetAlarmQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<AlarmDto> Handle(GetAlarmQuery request, CancellationToken cancellationToken)
    {
        var alarm = await _context.DeviceAlarms
            .Include(a => a.AlarmType)
            .Include(a => a.Device)
                .ThenInclude(d => d!.Rtu)
                    .ThenInclude(r => r!.Facility)
            .Include(a => a.AcknowledgedBy)
            .Include(a => a.ResolvedBy)
            .Where(a => a.Id == request.Id)
            .Select(a => new AlarmDto
            {
                Id = a.Id,
                Code = a.AlarmType!.Code,
                Message = a.AlarmType.Description,
                Severity = a.AlarmType.Severity.ToLower(),
                Status = !a.IsActive ? "resolved" : (a.AcknowledgedAt != null ? "acknowledged" : "active"),
                Source = a.Device != null ? a.Device.Name : a.AlarmType.Source,
                SourceType = a.AlarmType.Category != null ? a.AlarmType.Category.ToLower() : null,
                TriggeredAt = a.OccurredAt,
                Value = null,
                Threshold = null,
                AcknowledgedAt = a.AcknowledgedAt,
                AcknowledgedBy = a.AcknowledgedBy != null ? new AlarmUserDto
                {
                    Id = a.AcknowledgedBy.Id,
                    Name = a.AcknowledgedBy.Name,
                    Email = a.AcknowledgedBy.Email,
                    Initials = a.AcknowledgedBy.Name != null && a.AcknowledgedBy.Surname != null
                        ? (a.AcknowledgedBy.Name.Substring(0, 1) + a.AcknowledgedBy.Surname.Substring(0, 1)).ToUpper()
                        : null,
                    Role = a.AcknowledgedBy.Title
                } : null,
                ResolvedAt = a.ResolvedAt,
                ResolvedBy = a.ResolvedBy != null ? new AlarmUserDto
                {
                    Id = a.ResolvedBy.Id,
                    Name = a.ResolvedBy.Name,
                    Email = a.ResolvedBy.Email,
                    Initials = a.ResolvedBy.Name != null && a.ResolvedBy.Surname != null
                        ? (a.ResolvedBy.Name.Substring(0, 1) + a.ResolvedBy.Surname.Substring(0, 1)).ToUpper()
                        : null,
                    Role = a.ResolvedBy.Title
                } : null,
                RelatedAsset = a.Device != null ? new AlarmRelatedAssetDto
                {
                    Id = a.DeviceId,
                    Name = a.Device.Name,
                    Type = a.AlarmType.Category != null ? a.AlarmType.Category.ToLower() : null,
                    Location = a.Device.Rtu != null && a.Device.Rtu.Facility != null
                        ? a.Device.Rtu.Facility.Name : null,
                    Status = a.Device.Status,
                    SerialNumber = a.Device.SerialNumber,
                    Model = a.Device.Model
                } : null,
                RelatedMaintenanceTaskId = a.MaintenanceTaskId,
                Location = a.Device != null && a.Device.Rtu != null && a.Device.Rtu.Facility != null
                    ? a.Device.Rtu.Facility.Name : null
            })
            .FirstOrDefaultAsync(cancellationToken);

        if (alarm == null)
            throw new NotFoundException(nameof(DeviceAlarm), request.Id.ToString());

        return alarm;
    }
}
