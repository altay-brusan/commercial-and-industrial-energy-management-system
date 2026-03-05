using Backend.Application.Common.Interfaces;
using Backend.Application.Common.Security;
using Backend.Domain.Entities;

namespace Backend.Application.MaintenanceTasks.Queries.GetMaintenanceTask;

[Authorize(Roles = "Administrator,Tenant.Admin")]
[RequirePermission(Backend.Domain.Constants.Permissions.MaintenanceView)]
public record GetMaintenanceTaskQuery : IRequest<MaintenanceTaskDetailDto>
{
    public Guid Id { get; init; }
}

public class GetMaintenanceTaskQueryHandler : IRequestHandler<GetMaintenanceTaskQuery, MaintenanceTaskDetailDto>
{
    private readonly IApplicationDbContext _context;

    public GetMaintenanceTaskQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<MaintenanceTaskDetailDto> Handle(GetMaintenanceTaskQuery request, CancellationToken cancellationToken)
    {
        var task = await _context.MaintenanceTasks
            .Include(t => t.AssignedTo)
            .Include(t => t.AssignedBy)
            .Include(t => t.CompletedBy)
            .Include(t => t.ClosedBy)
            .Include(t => t.Device)
            .Include(t => t.Facility)
            .Include(t => t.DeviceAlarm)
                .ThenInclude(a => a!.AlarmType)
            .Include(t => t.Notes)
                .ThenInclude(n => n.Author)
            .Include(t => t.ChecklistItems)
                .ThenInclude(c => c.CompletedBy)
            .Include(t => t.StatusChanges)
                .ThenInclude(s => s.ChangedBy)
            .Include(t => t.WorkSessions)
                .ThenInclude(w => w.User)
            .Where(t => t.Id == request.Id)
            .FirstOrDefaultAsync(cancellationToken);

        if (task == null)
            throw new NotFoundException(nameof(MaintenanceTask), request.Id.ToString());

        return new MaintenanceTaskDetailDto
        {
            Id = task.Id,
            Title = task.Title,
            Status = task.Status,
            Priority = task.Priority,
            AssignedToId = task.AssignedToId,
            AssignedToName = task.AssignedTo?.Name,
            AssignedById = task.AssignedById,
            AssignedByName = task.AssignedBy?.Name,
            DueDate = task.DueDate,
            Created = task.Created,
            DeviceName = task.Device?.Name,
            FacilityName = task.Facility?.Name,
            DeviceId = task.DeviceId,
            DeviceAlarmId = task.DeviceAlarmId,
            FacilityId = task.FacilityId,
            HasRelatedAlarm = task.DeviceAlarmId != null,
            Description = task.Description,
            Resolution = task.Resolution,
            RootCause = task.RootCause,
            Recommendations = task.Recommendations,
            PartsUsed = task.PartsUsed,
            LaborHours = task.LaborHours,
            CloseReason = task.CloseReason,
            CloseExplanation = task.CloseExplanation,
            CompletedAt = task.CompletedAt,
            ClosedAt = task.ClosedAt,
            CompletedByName = task.CompletedBy?.Name,
            ClosedByName = task.ClosedBy?.Name,
            RelatedAlarm = task.DeviceAlarm != null ? new RelatedAlarmDto
            {
                Id = task.DeviceAlarm.Id,
                Code = task.DeviceAlarm.AlarmType?.Code ?? string.Empty,
                Name = task.DeviceAlarm.AlarmType?.Name ?? string.Empty,
                Severity = task.DeviceAlarm.AlarmType?.Severity ?? string.Empty,
                IsActive = task.DeviceAlarm.IsActive,
                OccurredAt = task.DeviceAlarm.OccurredAt
            } : null,
            Notes = task.Notes.OrderByDescending(n => n.Created).Select(n => new TaskNoteDto
            {
                Id = n.Id,
                Content = n.Content,
                AuthorName = n.Author?.Name,
                Created = n.Created
            }).ToList(),
            Checklist = task.ChecklistItems.OrderBy(c => c.SortOrder).Select(c => new TaskChecklistItemDto
            {
                Id = c.Id,
                Label = c.Label,
                SortOrder = c.SortOrder,
                IsCompleted = c.IsCompleted,
                CompletedAt = c.CompletedAt,
                CompletedByName = c.CompletedBy?.Name
            }).ToList(),
            Timeline = task.StatusChanges.OrderByDescending(s => s.ChangedAt).Select(s => new TaskStatusChangeDto
            {
                Id = s.Id,
                FromStatus = s.FromStatus,
                ToStatus = s.ToStatus,
                ChangedByName = s.ChangedBy?.Name,
                ChangedAt = s.ChangedAt
            }).ToList(),
            WorkSessions = task.WorkSessions.OrderByDescending(w => w.StartedAt).Select(w => new TaskWorkSessionDto
            {
                Id = w.Id,
                UserName = w.User?.Name,
                StartedAt = w.StartedAt,
                EndedAt = w.EndedAt
            }).ToList()
        };
    }
}
