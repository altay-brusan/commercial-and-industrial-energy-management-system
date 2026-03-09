using Backend.Application.Common.Interfaces;
using Backend.Application.Common.Security;
using Backend.Application.Notifications.Queries;

namespace Backend.Application.Notifications.Commands.MarkAllNotificationsRead;

[Authorize]
public record MarkAllNotificationsReadCommand : IRequest<MarkAllReadResponseDto>;

public class MarkAllNotificationsReadCommandHandler : IRequestHandler<MarkAllNotificationsReadCommand, MarkAllReadResponseDto>
{
    private readonly IApplicationDbContext _context;
    private readonly IUser _user;

    public MarkAllNotificationsReadCommandHandler(IApplicationDbContext context, IUser user)
    {
        _context = context;
        _user = user;
    }

    public async Task<MarkAllReadResponseDto> Handle(MarkAllNotificationsReadCommand request, CancellationToken cancellationToken)
    {
        var unread = await _context.Notifications
            .Where(n => n.UserId == _user.Id && !n.IsRead)
            .ToListAsync(cancellationToken);

        foreach (var notification in unread)
            notification.IsRead = true;

        await _context.SaveChangesAsync(cancellationToken);

        return new MarkAllReadResponseDto
        {
            Message = "All notifications marked as read",
            UpdatedCount = unread.Count
        };
    }
}
