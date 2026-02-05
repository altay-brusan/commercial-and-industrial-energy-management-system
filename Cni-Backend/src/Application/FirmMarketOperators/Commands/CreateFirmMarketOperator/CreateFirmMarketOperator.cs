using Backend.Application.Common.Interfaces;
using Backend.Application.Common.Security;
using Backend.Domain.Entities;
using Backend.Domain.Events;

namespace Backend.Application.FirmMarketOperators.Commands.CreateFirmMarketOperator;

[Authorize(Roles = "Administrator,Tenant.Admin")]
public record CreateFirmMarketOperatorCommand : IRequest<int>
{
    public int FirmId { get; set; }
    public int MarketOperatorId { get; set; }
    public string AreaCode { get; set; } = string.Empty;
    public int SamplingPeriodMinutes { get; set; } = 60;
}

public class CreateFirmMarketOperatorCommandHandler : IRequestHandler<CreateFirmMarketOperatorCommand, int>
{
    private readonly IApplicationDbContext _context;

    public CreateFirmMarketOperatorCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<int> Handle(CreateFirmMarketOperatorCommand request, CancellationToken cancellationToken)
    {
        var entity = new FirmMarketOperator
        {
            FirmId = request.FirmId,
            MarketOperatorId = request.MarketOperatorId,
            AreaCode = request.AreaCode,
            SamplingPeriodMinutes = request.SamplingPeriodMinutes,
            IsActive = true
        };

        entity.AddDomainEvent(new FirmMarketOperatorCreatedEvent(entity));

        _context.FirmMarketOperators.Add(entity);

        await _context.SaveChangesAsync(cancellationToken);
        return entity.Id;
    }
}
