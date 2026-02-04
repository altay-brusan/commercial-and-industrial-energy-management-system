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

public class UpdateMarketOperatorCommandValidator : AbstractValidator<UpdateMarketOperatorCommand>
{
    private readonly IApplicationDbContext _context;

    public UpdateMarketOperatorCommandValidator(IApplicationDbContext context)
    {
        _context = context;

        RuleFor(v => v.Id)
            .GreaterThan(0);

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

    private async Task<bool> BeUniqueCode(UpdateMarketOperatorCommand command, string code, CancellationToken cancellationToken)
    {
        return !await _context.MarketOperators
            .AnyAsync(x => x.Code == code && x.Id != command.Id, cancellationToken);
    }
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
