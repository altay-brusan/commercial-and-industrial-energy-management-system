using Backend.Application.Common.Interfaces;
using Backend.Application.Common.Security;
using Backend.Domain.Entities;
using Backend.Domain.Events;

namespace Backend.Application.FirmMarketOperators.Commands.CreateFirmMarketOperator;

[Authorize(Roles = "Administrator,Tenant.Admin")]
public record CreateFirmMarketOperatorCommand : IRequest<int>
{
    public int FirmId { get; set; }
    public int MarketOperatorId { get; set; }
    public string AreaCode { get; set; } = string.Empty;
    public int SamplingPeriodMinutes { get; set; } = 60;
}

public class CreateFirmMarketOperatorCommandValidator : AbstractValidator<CreateFirmMarketOperatorCommand>
{
    private readonly IApplicationDbContext _context;

    public CreateFirmMarketOperatorCommandValidator(IApplicationDbContext context)
    {
        _context = context;

        RuleFor(v => v.FirmId)
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

public class CreateFirmMarketOperatorCommandHandler : IRequestHandler<CreateFirmMarketOperatorCommand, int>
{
    private readonly IApplicationDbContext _context;

    public CreateFirmMarketOperatorCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<int> Handle(CreateFirmMarketOperatorCommand request, CancellationToken cancellationToken)
    {
        var entity = new FirmMarketOperator
        {
            FirmId = request.FirmId,
            MarketOperatorId = request.MarketOperatorId,
            AreaCode = request.AreaCode,
            SamplingPeriodMinutes = request.SamplingPeriodMinutes,
            IsActive = true
        };

        entity.AddDomainEvent(new FirmMarketOperatorCreatedEvent(entity));

        _context.FirmMarketOperators.Add(entity);

        await _context.SaveChangesAsync(cancellationToken);
        return entity.Id;
    }
}
