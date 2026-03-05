using Backend.Application.Common.Interfaces;
using Backend.Application.Common.Models;
using Backend.Application.Common.Security;
using Backend.Domain.Constants;

namespace Backend.Application.Alarms.Queries.GetAlarms;

[Authorize(Roles = "Administrator,Tenant.Admin")]
[RequirePermission(Backend.Domain.Constants.Permissions.AlarmView)]
public record GetAlarmsQuery : IRequest<PaginatedList<AlarmDto>>
{
    public Guid? FacilityId { get; init; }
    public string? Severity { get; init; }
    public string? Status { get; init; } // "Active", "Acknowledged", "Resolved"
    public Guid? DeviceId { get; init; }
    public bool? IsActive { get; init; }
    public DateTimeOffset? DateFrom { get; init; }
    public DateTimeOffset? DateTo { get; init; }
    public string? SearchTerm { get; init; }
    public int PageNumber { get; init; } = 1;
    public int PageSize { get; init; } = 20;
}

public class GetAlarmsQueryHandler : IRequestHandler<GetAlarmsQuery, PaginatedList<AlarmDto>>
{
    private readonly IApplicationDbContext _context;

    public GetAlarmsQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<PaginatedList<AlarmDto>> Handle(GetAlarmsQuery request, CancellationToken cancellationToken)
    {
        var query = _context.DeviceAlarms
            .Include(a => a.AlarmType)
            .Include(a => a.Device)
                .ThenInclude(d => d!.Rtu)
                    .ThenInclude(r => r!.Facility)
            .Include(a => a.AcknowledgedBy)
            .Include(a => a.ResolvedBy)
            .AsQueryable();

        if (request.FacilityId.HasValue)
            query = query.Where(a => a.Device!.Rtu!.FacilityId == request.FacilityId.Value);

        if (!string.IsNullOrEmpty(request.Severity))
            query = query.Where(a => a.AlarmType!.Severity == request.Severity);

        if (request.DeviceId.HasValue)
            query = query.Where(a => a.DeviceId == request.DeviceId.Value);

        if (request.IsActive.HasValue)
            query = query.Where(a => a.IsActive == request.IsActive.Value);

        if (request.DateFrom.HasValue)
            query = query.Where(a => a.OccurredAt >= request.DateFrom.Value);

        if (request.DateTo.HasValue)
            query = query.Where(a => a.OccurredAt <= request.DateTo.Value);

        // Status filter: Active = IsActive && not acknowledged, Acknowledged = IsActive && acknowledged, Resolved = !IsActive
        if (!string.IsNullOrEmpty(request.Status))
        {
            query = request.Status switch
            {
                "Active" => query.Where(a => a.IsActive && a.AcknowledgedAt == null),
                "Acknowledged" => query.Where(a => a.IsActive && a.AcknowledgedAt != null),
                "Resolved" => query.Where(a => !a.IsActive),
                _ => query
            };
        }

        if (!string.IsNullOrEmpty(request.SearchTerm))
        {
            var term = request.SearchTerm.ToLower();
            query = query.Where(a =>
                a.AlarmType!.Name.ToLower().Contains(term) ||
                a.AlarmType.Code.ToLower().Contains(term) ||
                (a.Device != null && a.Device.Name.ToLower().Contains(term)));
        }

        var projected = query
            .OrderByDescending(a => a.OccurredAt)
            .Select(a => new AlarmDto
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
                HasMaintenanceTask = a.MaintenanceTaskId != null
            });

        return await PaginatedList<AlarmDto>.CreateAsync(projected, request.PageNumber, request.PageSize);
    }
}
