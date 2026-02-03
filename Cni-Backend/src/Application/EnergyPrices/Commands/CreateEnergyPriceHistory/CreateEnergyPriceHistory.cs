using Backend.Application.Common.Interfaces;
using Backend.Application.Common.Security;
using Backend.Domain.Entities;
using Backend.Domain.Events;

namespace Backend.Application.EnergyPrices.Commands.CreateEnergyPriceHistory;

[Authorize(Roles = "Administrator,Tenant.Admin")]
public record CreateEnergyPriceHistoryCommand : IRequest<int>
{
    public int TenantId { get; set; }
    public int TsoId { get; set; }
    public decimal RawPrice { get; set; }
    public DateTimeOffset PriceTimestamp { get; set; }
}

public class CreateEnergyPriceHistoryCommandValidator : AbstractValidator<CreateEnergyPriceHistoryCommand>
{
    public CreateEnergyPriceHistoryCommandValidator()
    {
        RuleFor(v => v.TsoId)
            .GreaterThan(0);

        RuleFor(v => v.RawPrice)
            .GreaterThanOrEqualTo(0);

        RuleFor(v => v.PriceTimestamp)
            .NotEmpty();
    }
}

public class CreateEnergyPriceHistoryCommandHandler : IRequestHandler<CreateEnergyPriceHistoryCommand, int>
{
    private readonly IApplicationDbContext _context;
    private readonly int? _tenantId;
    private readonly bool _isSuperAdmin;

    public CreateEnergyPriceHistoryCommandHandler(IApplicationDbContext context, IMultiTenantService multiTenantService)
    {
        _context = context;
        _tenantId = multiTenantService.GetCurrentTenantId();
        _isSuperAdmin = multiTenantService.IsSuperAdmin();
    }

    public async Task<int> Handle(CreateEnergyPriceHistoryCommand request, CancellationToken cancellationToken)
    {
        var tenantId = _isSuperAdmin ? request.TenantId : _tenantId;

        if (tenantId is null)
        {
            throw new ArgumentNullException(nameof(tenantId));
        }

        var tso = await _context.TransmissionSystemOperators
            .FindAsync(new object[] { request.TsoId }, cancellationToken);

        Guard.Against.NotFound(request.TsoId, tso);

        // Calculate total price: RawPrice * (1 + TaxRate) + DistributionFee
        var totalPrice = request.RawPrice * (1 + tso.TaxRate) + tso.DistributionFee;

        var entity = new EnergyPriceHistory
        {
            TenantId = (int)tenantId,
            TsoId = request.TsoId,
            RawPrice = request.RawPrice,
            AppliedTaxRate = tso.TaxRate,
            AppliedDistributionFee = tso.DistributionFee,
            TotalPrice = totalPrice,
            PriceTimestamp = request.PriceTimestamp,
            Currency = tso.Currency
        };

        entity.AddDomainEvent(new EnergyPriceHistoryCreatedEvent(entity));

        _context.EnergyPriceHistories.Add(entity);

        await _context.SaveChangesAsync(cancellationToken);
        return entity.Id;
    }
}
