using Backend.Application.Common.Interfaces;
using Backend.Application.Common.Security;

namespace Backend.Application.Notifications.Queries.GetNotifications;

[Authorize]
public record GetNotificationsQuery : IRequest<NotificationListResponseDto>
{
    public string? ReadStatus { get; init; } // "all", "read", "unread"
    public string? Type { get; init; }
}

public class GetNotificationsQueryHandler : IRequestHandler<GetNotificationsQuery, NotificationListResponseDto>
{
    private readonly IApplicationDbContext _context;
    private readonly IUser _user;

    public GetNotificationsQueryHandler(IApplicationDbContext context, IUser user)
    {
        _context = context;
        _user = user;
    }

    public async Task<NotificationListResponseDto> Handle(GetNotificationsQuery request, CancellationToken cancellationToken)
    {
        var userId = _user.Id!;

        var query = _context.Notifications
            .Where(n => n.UserId == userId)
            .AsQueryable();

        if (!string.IsNullOrEmpty(request.ReadStatus))
        {
            query = request.ReadStatus.ToLower() switch
            {
                "read" => query.Where(n => n.IsRead),
                "unread" => query.Where(n => !n.IsRead),
                _ => query // "all" or anything else
            };
        }

        if (!string.IsNullOrEmpty(request.Type))
            query = query.Where(n => n.Type == request.Type);

        var notifications = await query
            .OrderByDescending(n => n.Timestamp)
            .Select(n => new NotificationDto
            {
                Id = n.Id,
                Type = n.Type,
                Title = n.Title,
                Message = n.Message,
                Timestamp = n.Timestamp,
                Read = n.IsRead,
                RelatedAlarmId = n.RelatedAlarmId,
                RelatedTaskId = n.RelatedTaskId
            })
            .ToListAsync(cancellationToken);

        var unreadCount = await _context.Notifications
            .Where(n => n.UserId == userId && !n.IsRead)
            .CountAsync(cancellationToken);

        return new NotificationListResponseDto
        {
            Notifications = notifications,
            UnreadCount = unreadCount
        };
    }
}
