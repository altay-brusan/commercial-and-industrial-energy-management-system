using Backend.Application.Common.Interfaces;
using Backend.Application.Common.Security;

namespace Backend.Application.FirmMarketOperators.Commands.UpdateFirmMarketOperator;

[Authorize(Roles = "Administrator,Tenant.Admin")]
public record UpdateFirmMarketOperatorCommand : IRequest
{
    public int Id { get; set; }
    public int MarketOperatorId { get; set; }
    public string AreaCode { get; set; } = string.Empty;
    public int SamplingPeriodMinutes { get; set; } = 60;
    public bool IsActive { get; set; } = true;
}

public class UpdateFirmMarketOperatorCommandHandler : IRequestHandler<UpdateFirmMarketOperatorCommand>
{
    private readonly IApplicationDbContext _context;

    public UpdateFirmMarketOperatorCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task Handle(UpdateFirmMarketOperatorCommand request, CancellationToken cancellationToken)
    {
        var entity = await _context.FirmMarketOperators
            .FindAsync(new object[] { request.Id }, cancellationToken);

        Guard.Against.NotFound(request.Id, entity);

        entity.MarketOperatorId = request.MarketOperatorId;
        entity.AreaCode = request.AreaCode;
        entity.SamplingPeriodMinutes = request.SamplingPeriodMinutes;
        entity.IsActive = request.IsActive;

        await _context.SaveChangesAsync(cancellationToken);
    }
}
