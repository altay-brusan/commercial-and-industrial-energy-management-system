using Backend.Application.Common.Interfaces;

namespace Backend.Application.MarketOperators.Commands.UpdateMarketOperator;

public class UpdateMarketOperatorCommandValidator : AbstractValidator<UpdateMarketOperatorCommand>
{
    private readonly IApplicationDbContext _context;

    public UpdateMarketOperatorCommandValidator(IApplicationDbContext context)
    {
        _context = context;

        RuleFor(v => v.Id)
            .NotEmpty();

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
