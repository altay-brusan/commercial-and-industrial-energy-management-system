using Backend.Application.Common.Interfaces;
using Backend.Application.Common.Security;
using Backend.Domain.Entities;

namespace Backend.Application.MaintenanceTasks.Commands.AddTaskNote;

[Authorize(Roles = "Administrator,Tenant.Admin")]
[RequirePermission(Backend.Domain.Constants.Permissions.MaintenanceManage)]
public record AddTaskNoteCommand : IRequest<Guid>
{
    public Guid TaskId { get; init; }
    public string Content { get; init; } = string.Empty;
}

public class AddTaskNoteCommandHandler : IRequestHandler<AddTaskNoteCommand, Guid>
{
    private readonly IApplicationDbContext _context;
    private readonly IUser _user;

    public AddTaskNoteCommandHandler(IApplicationDbContext context, IUser user)
    {
        _context = context;
        _user = user;
    }

    public async Task<Guid> Handle(AddTaskNoteCommand request, CancellationToken cancellationToken)
    {
        var task = await _context.MaintenanceTasks
            .AnyAsync(t => t.Id == request.TaskId, cancellationToken);

        if (!task)
            throw new NotFoundException(nameof(MaintenanceTask), request.TaskId.ToString());

        var note = new TaskNote
        {
            MaintenanceTaskId = request.TaskId,
            Content = request.Content,
            AuthorId = _user.Id!
        };

        _context.TaskNotes.Add(note);
        await _context.SaveChangesAsync(cancellationToken);

        return note.Id;
    }
}
