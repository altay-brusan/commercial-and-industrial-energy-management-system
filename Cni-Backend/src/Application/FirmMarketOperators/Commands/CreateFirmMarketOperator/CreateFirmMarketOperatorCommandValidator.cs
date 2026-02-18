using Backend.Application.Common.Interfaces;

namespace Backend.Application.FirmMarketOperators.Commands.CreateFirmMarketOperator;

public class CreateFirmMarketOperatorCommandValidator : AbstractValidator<CreateFirmMarketOperatorCommand>
{
    private readonly IApplicationDbContext _context;

    public CreateFirmMarketOperatorCommandValidator(IApplicationDbContext context)
    {
        _context = context;

        RuleFor(v => v.FirmId)
            .NotEmpty();

        RuleFor(v => v.MarketOperatorId)
            .NotEmpty();

        RuleFor(v => v.AreaCode)
            .NotEmpty()
            .MaximumLength(50)
            .MustAsync(BeValidAreaCode)
            .WithMessage("Area code must be one of the supported area codes for the selected Market Operator.");

        RuleFor(v => v.SamplingPeriodMinutes)
            .GreaterThan(0)
            .MustAsync(BeValidResolution)
            .WithMessage("Sampling period must be one of the supported resolutions for the selected Market Operator.");

        RuleFor(v => v)
            .MustAsync(BeUniqueAssignment)
            .WithMessage("This firm already has a Market Operator assignment.");
    }

    private async Task<bool> BeValidAreaCode(CreateFirmMarketOperatorCommand command, string areaCode, CancellationToken cancellationToken)
    {
        var mo = await _context.MarketOperators
            .FindAsync(new object[] { command.MarketOperatorId }, cancellationToken);

        return mo != null && mo.AreaCodes.Contains(areaCode);
    }

    private async Task<bool> BeValidResolution(CreateFirmMarketOperatorCommand command, int resolution, CancellationToken cancellationToken)
    {
        var mo = await _context.MarketOperators
            .FindAsync(new object[] { command.MarketOperatorId }, cancellationToken);

        return mo != null && mo.SupportedResolutions.Contains(resolution);
    }

    private async Task<bool> BeUniqueAssignment(CreateFirmMarketOperatorCommand command, CancellationToken cancellationToken)
    {
        return !await _context.FirmMarketOperators
            .AnyAsync(x => x.FirmId == command.FirmId && x.MarketOperatorId == command.MarketOperatorId, cancellationToken);
    }
}
