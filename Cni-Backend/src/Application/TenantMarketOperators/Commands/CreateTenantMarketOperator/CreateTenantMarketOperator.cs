using Backend.Application.Common.Interfaces;
using Backend.Application.Common.Security;
using Backend.Domain.Entities;
using Backend.Domain.Events;

namespace Backend.Application.TenantMarketOperators.Commands.CreateTenantMarketOperator;

[Authorize(Roles = "Administrator,Tenant.Admin")]
public record CreateTenantMarketOperatorCommand : IRequest<int>
{
    public int TenantId { get; set; }
    public int MarketOperatorId { get; set; }
    public string AreaCode { get; set; } = string.Empty;
    public int SamplingFrequencyMinutes { get; set; } = 60;
}

public class CreateTenantMarketOperatorCommandValidator : AbstractValidator<CreateTenantMarketOperatorCommand>
{
    private readonly IApplicationDbContext _context;

    public CreateTenantMarketOperatorCommandValidator(IApplicationDbContext context)
    {
        _context = context;

        RuleFor(v => v.TenantId)
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

        RuleFor(v => v)
            .MustAsync(BeUniqueAssignment)
            .WithMessage("This tenant already has a Market Operator assignment.");
    }

    private async Task<bool> BeValidAreaCode(CreateTenantMarketOperatorCommand command, string areaCode, CancellationToken cancellationToken)
    {
        var mo = await _context.MarketOperators
            .FindAsync(new object[] { command.MarketOperatorId }, cancellationToken);

        return mo != null && mo.AreaCodes.Contains(areaCode);
    }

    private async Task<bool> BeValidResolution(CreateTenantMarketOperatorCommand command, int resolution, CancellationToken cancellationToken)
    {
        var mo = await _context.MarketOperators
            .FindAsync(new object[] { command.MarketOperatorId }, cancellationToken);

        return mo != null && mo.SupportedResolutions.Contains(resolution);
    }

    private async Task<bool> BeUniqueAssignment(CreateTenantMarketOperatorCommand command, CancellationToken cancellationToken)
    {
        return !await _context.TenantMarketOperators
            .IgnoreQueryFilters()
            .AnyAsync(x => x.TenantId == command.TenantId && x.MarketOperatorId == command.MarketOperatorId, cancellationToken);
    }
}

public class CreateTenantMarketOperatorCommandHandler : IRequestHandler<CreateTenantMarketOperatorCommand, int>
{
    private readonly IApplicationDbContext _context;
    private readonly int? _tenantId;
    private readonly bool _isSuperAdmin;

    public CreateTenantMarketOperatorCommandHandler(IApplicationDbContext context, IMultiTenantService multiTenantService)
    {
        _context = context;
        _tenantId = multiTenantService.GetCurrentTenantId();
        _isSuperAdmin = multiTenantService.IsSuperAdmin();
    }

    public async Task<int> Handle(CreateTenantMarketOperatorCommand request, CancellationToken cancellationToken)
    {
        var tenantId = _isSuperAdmin ? request.TenantId : _tenantId;

        if (tenantId is null)
        {
            throw new ArgumentNullException(nameof(tenantId));
        }

        var entity = new TenantMarketOperator
        {
            TenantId = (int)tenantId,
            MarketOperatorId = request.MarketOperatorId,
            AreaCode = request.AreaCode,
            SamplingFrequencyMinutes = request.SamplingFrequencyMinutes,
            IsActive = true
        };

        entity.AddDomainEvent(new TenantMarketOperatorCreatedEvent(entity));

        _context.TenantMarketOperators.Add(entity);

        await _context.SaveChangesAsync(cancellationToken);
        return entity.Id;
    }
}
