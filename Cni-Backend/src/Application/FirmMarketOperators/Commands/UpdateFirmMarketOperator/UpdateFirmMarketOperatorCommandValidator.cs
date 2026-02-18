using Backend.Application.Common.Interfaces;

namespace Backend.Application.FirmMarketOperators.Commands.UpdateFirmMarketOperator;

public class UpdateFirmMarketOperatorCommandValidator : AbstractValidator<UpdateFirmMarketOperatorCommand>
{
    private readonly IApplicationDbContext _context;

    public UpdateFirmMarketOperatorCommandValidator(IApplicationDbContext context)
    {
        _context = context;

        RuleFor(v => v.Id)
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
    }

    private async Task<bool> BeValidAreaCode(UpdateFirmMarketOperatorCommand command, string areaCode, CancellationToken cancellationToken)
    {
        var mo = await _context.MarketOperators
            .FindAsync(new object[] { command.MarketOperatorId }, cancellationToken);

        return mo != null && mo.AreaCodes.Contains(areaCode);
    }

    private async Task<bool> BeValidResolution(UpdateFirmMarketOperatorCommand command, int resolution, CancellationToken cancellationToken)
    {
        var mo = await _context.MarketOperators
            .FindAsync(new object[] { command.MarketOperatorId }, cancellationToken);

        return mo != null && mo.SupportedResolutions.Contains(resolution);
    }
}
