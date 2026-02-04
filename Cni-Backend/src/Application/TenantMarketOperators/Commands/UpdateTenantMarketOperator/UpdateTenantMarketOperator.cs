using Backend.Application.Common.Interfaces;
using Backend.Application.Common.Security;

namespace Backend.Application.TenantMarketOperators.Commands.UpdateTenantMarketOperator;

[Authorize(Roles = "Administrator,Tenant.Admin")]
public record UpdateTenantMarketOperatorCommand : IRequest
{
    public int Id { get; set; }
    public int MarketOperatorId { get; set; }
    public string AreaCode { get; set; } = string.Empty;
    public int SamplingFrequencyMinutes { get; set; } = 60;
    public bool IsActive { get; set; } = true;
}

public class UpdateTenantMarketOperatorCommandValidator : AbstractValidator<UpdateTenantMarketOperatorCommand>
{
    private readonly IApplicationDbContext _context;

    public UpdateTenantMarketOperatorCommandValidator(IApplicationDbContext context)
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

        RuleFor(v => v.SamplingFrequencyMinutes)
            .GreaterThan(0)
            .MustAsync(BeValidResolution)
            .WithMessage("Sampling frequency must be one of the supported resolutions for the selected Market Operator.");
    }

    private async Task<bool> BeValidAreaCode(UpdateTenantMarketOperatorCommand command, string areaCode, CancellationToken cancellationToken)
    {
        var mo = await _context.MarketOperators
            .FindAsync(new object[] { command.MarketOperatorId }, cancellationToken);

        return mo != null && mo.AreaCodes.Contains(areaCode);
    }

    private async Task<bool> BeValidResolution(UpdateTenantMarketOperatorCommand command, int resolution, CancellationToken cancellationToken)
    {
        var mo = await _context.MarketOperators
            .FindAsync(new object[] { command.MarketOperatorId }, cancellationToken);

        return mo != null && mo.SupportedResolutions.Contains(resolution);
    }
}

public class UpdateTenantMarketOperatorCommandHandler : IRequestHandler<UpdateTenantMarketOperatorCommand>
{
    private readonly IApplicationDbContext _context;

    public UpdateTenantMarketOperatorCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task Handle(UpdateTenantMarketOperatorCommand request, CancellationToken cancellationToken)
    {
        var entity = await _context.TenantMarketOperators
            .FindAsync(new object[] { request.Id }, cancellationToken);

        Guard.Against.NotFound(request.Id, entity);

        entity.MarketOperatorId = request.MarketOperatorId;
        entity.AreaCode = request.AreaCode;
        entity.SamplingFrequencyMinutes = request.SamplingFrequencyMinutes;
        entity.IsActive = request.IsActive;

        await _context.SaveChangesAsync(cancellationToken);
    }
}
