using Backend.Application.Common.Interfaces;
using Backend.Application.Common.Security;

namespace Backend.Application.MarketOperators.Commands.UpdateMarketOperator;

[Authorize(Roles = "Administrator")]
public record UpdateMarketOperatorCommand : IRequest
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public string BaseUrl { get; set; } = string.Empty;
    public string? ApiKey { get; set; }
    public List<int> SupportedResolutions { get; set; } = new() { 60 };
    public List<string> AreaCodes { get; set; } = new();
    public bool IsActive { get; set; } = true;
}

public class UpdateMarketOperatorCommandHandler : IRequestHandler<UpdateMarketOperatorCommand>
{
    private readonly IApplicationDbContext _context;

    public UpdateMarketOperatorCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task Handle(UpdateMarketOperatorCommand request, CancellationToken cancellationToken)
    {
        var entity = await _context.MarketOperators
            .FindAsync(new object[] { request.Id }, cancellationToken);

        Guard.Against.NotFound(request.Id, entity);

        entity.Name = request.Name;
        entity.Code = request.Code;
        entity.BaseUrl = request.BaseUrl;
        entity.ApiKey = request.ApiKey;
        entity.SupportedResolutions = request.SupportedResolutions;
        entity.AreaCodes = request.AreaCodes;
        entity.IsActive = request.IsActive;

        await _context.SaveChangesAsync(cancellationToken);
    }
}
