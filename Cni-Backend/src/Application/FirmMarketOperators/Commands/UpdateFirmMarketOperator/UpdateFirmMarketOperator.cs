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

public class UpdateFirmMarketOperatorCommandValidator : AbstractValidator<UpdateFirmMarketOperatorCommand>
{
    private readonly IApplicationDbContext _context;

    public UpdateFirmMarketOperatorCommandValidator(IApplicationDbContext context)
    {
        _context = context;

        RuleFor(v => v.Id)
            .GreaterThan(0);

        RuleFor(v => v.MarketOperatorId)
            .GreaterThan(0);

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
