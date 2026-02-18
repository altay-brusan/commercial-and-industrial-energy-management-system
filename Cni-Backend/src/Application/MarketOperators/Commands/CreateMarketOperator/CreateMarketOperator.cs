using Backend.Application.Common.Interfaces;
using Backend.Application.Common.Security;
using Backend.Domain.Entities;
using Backend.Domain.Events;

namespace Backend.Application.MarketOperators.Commands.CreateMarketOperator;

[Authorize(Roles = "Administrator")]
public record CreateMarketOperatorCommand : IRequest<Guid>
{
    public string Name { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public string BaseUrl { get; set; } = string.Empty;
    public string? ApiKey { get; set; }
    public List<int> SupportedResolutions { get; set; } = new() { 60 };
    public List<string> AreaCodes { get; set; } = new();
}

public class CreateMarketOperatorCommandHandler : IRequestHandler<CreateMarketOperatorCommand, Guid>
{
    private readonly IApplicationDbContext _context;

    public CreateMarketOperatorCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Guid> Handle(CreateMarketOperatorCommand request, CancellationToken cancellationToken)
    {
        var entity = new MarketOperator
        {
            Name = request.Name,
            Code = request.Code,
            BaseUrl = request.BaseUrl,
            ApiKey = request.ApiKey,
            SupportedResolutions = request.SupportedResolutions,
            AreaCodes = request.AreaCodes,
            IsActive = true
        };

        entity.AddDomainEvent(new MarketOperatorCreatedEvent(entity));

        _context.MarketOperators.Add(entity);

        await _context.SaveChangesAsync(cancellationToken);
        return entity.Id;
    }
}
