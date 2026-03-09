using Backend.Application.Common.Interfaces;
using Backend.Application.Common.Security;
using Backend.Application.Notifications.Queries;
using Backend.Domain.Entities;

namespace Backend.Application.Notifications.Commands.MarkNotificationRead;

[Authorize]
public record MarkNotificationReadCommand : IRequest<NotificationDto>
{
    public Guid Id { get; init; }
}

public class MarkNotificationReadCommandHandler : IRequestHandler<MarkNotificationReadCommand, NotificationDto>
{
    private readonly IApplicationDbContext _context;
    private readonly IUser _user;

    public MarkNotificationReadCommandHandler(IApplicationDbContext context, IUser user)
    {
        _context = context;
        _user = user;
    }

    public async Task<NotificationDto> Handle(MarkNotificationReadCommand request, CancellationToken cancellationToken)
    {
        var notification = await _context.Notifications
            .FirstOrDefaultAsync(n => n.Id == request.Id && n.UserId == _user.Id, cancellationToken);

        if (notification == null)
            throw new NotFoundException(nameof(Notification), request.Id.ToString());

        notification.IsRead = true;
        await _context.SaveChangesAsync(cancellationToken);

        return new NotificationDto
        {
            Id = notification.Id,
            Type = notification.Type,
            Title = notification.Title,
            Message = notification.Message,
            Timestamp = notification.Timestamp,
            Read = notification.IsRead,
            RelatedAlarmId = notification.RelatedAlarmId,
            RelatedTaskId = notification.RelatedTaskId
        };
    }
}
