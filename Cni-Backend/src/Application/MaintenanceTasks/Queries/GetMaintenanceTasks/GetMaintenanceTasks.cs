using Backend.Application.Common.Interfaces;
using Backend.Application.Common.Models;
using Backend.Application.Common.Security;

namespace Backend.Application.MaintenanceTasks.Queries.GetMaintenanceTasks;

[Authorize(Roles = "Administrator,Tenant.Admin")]
[RequirePermission(Backend.Domain.Constants.Permissions.MaintenanceView)]
public record GetMaintenanceTasksQuery : IRequest<PaginatedList<MaintenanceTaskBriefDto>>
{
    public Guid? FacilityId { get; init; }
    public string? Status { get; init; }
    public string? Priority { get; init; }
    public string? AssignedToId { get; init; }
    public DateTimeOffset? DateFrom { get; init; }
    public DateTimeOffset? DateTo { get; init; }
    public string? SearchTerm { get; init; }
    public int PageNumber { get; init; } = 1;
    public int PageSize { get; init; } = 20;
}

public class GetMaintenanceTasksQueryHandler : IRequestHandler<GetMaintenanceTasksQuery, PaginatedList<MaintenanceTaskBriefDto>>
{
    private readonly IApplicationDbContext _context;

    public GetMaintenanceTasksQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<PaginatedList<MaintenanceTaskBriefDto>> Handle(GetMaintenanceTasksQuery request, CancellationToken cancellationToken)
    {
        var query = _context.MaintenanceTasks
            .Include(t => t.AssignedTo)
            .Include(t => t.AssignedBy)
            .Include(t => t.Device)
            .Include(t => t.Facility)
            .AsQueryable();

        if (request.FacilityId.HasValue)
            query = query.Where(t => t.FacilityId == request.FacilityId.Value);

        if (!string.IsNullOrEmpty(request.Status))
            query = query.Where(t => t.Status == request.Status);

        if (!string.IsNullOrEmpty(request.Priority))
            query = query.Where(t => t.Priority == request.Priority);

        if (!string.IsNullOrEmpty(request.AssignedToId))
            query = query.Where(t => t.AssignedToId == request.AssignedToId);

        if (request.DateFrom.HasValue)
            query = query.Where(t => t.DueDate >= request.DateFrom.Value);

        if (request.DateTo.HasValue)
            query = query.Where(t => t.DueDate <= request.DateTo.Value);

        if (!string.IsNullOrEmpty(request.SearchTerm))
        {
            var term = request.SearchTerm.ToLower();
            query = query.Where(t =>
                t.Title.ToLower().Contains(term) ||
                (t.Description != null && t.Description.ToLower().Contains(term)));
        }

        var projected = query
            .OrderByDescending(t => t.Created)
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
            });

        return await PaginatedList<MaintenanceTaskBriefDto>.CreateAsync(projected, request.PageNumber, request.PageSize);
    }
}
