using Backend.Application.Common.Interfaces;
using Backend.Application.Common.Security;

namespace Backend.Application.Scada.Commands.ChangeTime;

public record ChangeTimeCommand : IRequest<bool>
{
    public DateTime Time { get; set; }
}

public class ChangeTimeCommandHandler : IRequestHandler<ChangeTimeCommand, bool>
{
    private readonly IApplicationDbContext _context;
    private readonly IVirtualClockService _virtualClockService;

    public ChangeTimeCommandHandler(IApplicationDbContext context, IVirtualClockService virtualClockService)
    {
        _context = context;
        _virtualClockService = virtualClockService;
    }

    public async Task<bool> Handle(ChangeTimeCommand request, CancellationToken cancellationToken)
    {
        _virtualClockService.SetInitialTime(request.Time);
        return await Task.FromResult(true);
    }
}
