using Backend.Application.Common.Interfaces;
using Backend.Application.Common.Security;

namespace Backend.Application.Maintenance.Queries.GetScheduledMaintenances;

[Authorize(Roles = "Administrator,Tenant.Admin")]
[RequirePermission(Backend.Domain.Constants.Permissions.MaintenanceView)]
public record GetScheduledMaintenancesQuery : IRequest<List<MaintenanceTaskBriefDto>>
{
    public int Month { get; init; }
    public int Year { get; init; }
    public Guid? FacilityId { get; init; }
}

public class GetScheduledMaintenancesQueryHandler : IRequestHandler<GetScheduledMaintenancesQuery, List<MaintenanceTaskBriefDto>>
{
    private readonly IApplicationDbContext _context;

    public GetScheduledMaintenancesQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<MaintenanceTaskBriefDto>> Handle(GetScheduledMaintenancesQuery request, CancellationToken cancellationToken)
    {
        var startDate = new DateTimeOffset(request.Year, request.Month, 1, 0, 0, 0, TimeSpan.Zero);
        var endDate = startDate.AddMonths(1);

        var query = _context.MaintenanceTasks
            .Include(t => t.AssignedTo)
            .Include(t => t.AssignedBy)
            .Include(t => t.Device)
            .Include(t => t.Facility)
            .Where(t => t.DueDate >= startDate && t.DueDate < endDate);

        if (request.FacilityId.HasValue)
            query = query.Where(t => t.FacilityId == request.FacilityId.Value);

        return await query
            .OrderBy(t => t.DueDate)
            .Select(t => new MaintenanceTaskBriefDto
            {
                Id = t.Id,
                Title = t.Title,
                Status = t.Status,
                Priority = t.Priority,
                AssignedToId = t.AssignedToId,
                AssignedToName = t.AssignedTo != null ? t.AssignedTo.Name : null,
                AssignedById = t.AssignedById,
                AssignedByName = t.AssignedBy != null ? t.AssignedBy.Name : null,
                DueDate = t.DueDate,
                Created = t.Created,
                DeviceName = t.Device != null ? t.Device.Name : null,
                FacilityName = t.Facility != null ? t.Facility.Name : null,
                DeviceId = t.DeviceId,
                DeviceAlarmId = t.DeviceAlarmId,
                FacilityId = t.FacilityId,
                HasRelatedAlarm = t.DeviceAlarmId != null
            })
            .ToListAsync(cancellationToken);
    }
}
