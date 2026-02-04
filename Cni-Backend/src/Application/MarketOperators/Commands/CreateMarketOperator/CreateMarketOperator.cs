using Backend.Application.Common.Interfaces;
using Backend.Application.Common.Security;
using Backend.Domain.Entities;
using Backend.Domain.Events;

namespace Backend.Application.MarketOperators.Commands.CreateMarketOperator;

[Authorize(Roles = "Administrator")]
public record CreateMarketOperatorCommand : IRequest<int>
{
    public string Name { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public string BaseUrl { get; set; } = string.Empty;
    public string? ApiKey { get; set; }
    public List<int> SupportedResolutions { get; set; } = new() { 60 };
    public List<string> AreaCodes { get; set; } = new();
}

public class CreateMarketOperatorCommandValidator : AbstractValidator<CreateMarketOperatorCommand>
{
    private readonly IApplicationDbContext _context;

    public CreateMarketOperatorCommandValidator(IApplicationDbContext context)
    {
        _context = context;

        RuleFor(v => v.Name)
            .NotEmpty()
            .MaximumLength(200);

        RuleFor(v => v.Code)
            .NotEmpty()
            .MaximumLength(50)
            .MustAsync(BeUniqueCode)
            .WithMessage("A Market Operator with this code already exists.");

        RuleFor(v => v.BaseUrl)
            .NotEmpty()
            .MaximumLength(500);

        RuleFor(v => v.SupportedResolutions)
            .NotEmpty()
            .WithMessage("At least one resolution must be specified.")
            .Must(resolutions => resolutions.All(r => r > 0))
            .WithMessage("All resolutions must be positive.");

        RuleFor(v => v.AreaCodes)
            .NotEmpty()
            .WithMessage("At least one area code must be specified.");
    }

    private async Task<bool> BeUniqueCode(string code, CancellationToken cancellationToken)
    {
        return !await _context.MarketOperators
            .AnyAsync(x => x.Code == code, cancellationToken);
    }
}

public class CreateMarketOperatorCommandHandler : IRequestHandler<CreateMarketOperatorCommand, int>
{
    private readonly IApplicationDbContext _context;

    public CreateMarketOperatorCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<int> Handle(CreateMarketOperatorCommand request, CancellationToken cancellationToken)
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
