using Backend.Application.Common;
using Backend.Application.Common.Interfaces;
using Backend.Application.Common.Security;
using Backend.Domain.Entities;
using Backend.Domain.Events;
using Microsoft.EntityFrameworkCore;

namespace Backend.Application.EnergyPrices.Commands.CreateEnergyPriceHistory;

[Authorize(Roles = "Administrator,Tenant.Admin")]
public record CreateEnergyPriceHistoryCommand : IRequest<int>
{
    public int TenantId { get; set; }
    public int TsoId { get; set; }
    public decimal RawPrice { get; set; }
    public DateTimeOffset PriceTimestamp { get; set; }

    /// <summary>
    /// Country name for tariff lookup. If not provided, defaults will be used.
    /// </summary>
    public string? CountryName { get; set; }

    /// <summary>
    /// Override tariff values. If provided, these will be used instead of looking up from the database.
    /// </summary>
    public decimal? DistributionFee { get; set; }
    public decimal? TransmissionFee { get; set; }
    public decimal? TaxRate { get; set; }
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

        // Get tariff values - use provided values, or look up from database, or fall back to defaults
        var (distributionFee, transmissionFee, taxRate) = await GetTariffValuesAsync(
            (int)tenantId,
            request.CountryName,
            request.DistributionFee,
            request.TransmissionFee,
            request.TaxRate,
            cancellationToken);

        // Calculate total price with new formula: (RawPrice + DistributionFee + TransmissionFee) × (1 + TaxRate)
        var totalPrice = (request.RawPrice + distributionFee + transmissionFee) * (1 + taxRate);

        var entity = new EnergyPriceHistory
        {
            TenantId = (int)tenantId,
            TsoId = request.TsoId,
            RawPrice = request.RawPrice,
            AppliedTaxRate = taxRate,
            AppliedDistributionFee = distributionFee,
            AppliedTransmissionFee = transmissionFee,
            TotalPrice = totalPrice,
            PriceTimestamp = request.PriceTimestamp,
            Currency = tso.Currency
        };

        entity.AddDomainEvent(new EnergyPriceHistoryCreatedEvent(entity));

        _context.EnergyPriceHistories.Add(entity);

        await _context.SaveChangesAsync(cancellationToken);
        return entity.Id;
    }

    private async Task<(decimal distributionFee, decimal transmissionFee, decimal taxRate)> GetTariffValuesAsync(
        int tenantId,
        string? countryName,
        decimal? overrideDistributionFee,
        decimal? overrideTransmissionFee,
        decimal? overrideTaxRate,
        CancellationToken cancellationToken)
    {
        // If all values are provided as overrides, use them
        if (overrideDistributionFee.HasValue && overrideTransmissionFee.HasValue && overrideTaxRate.HasValue)
        {
            return (overrideDistributionFee.Value, overrideTransmissionFee.Value, overrideTaxRate.Value);
        }

        // Try to look up from database by country name
        if (!string.IsNullOrWhiteSpace(countryName))
        {
            var tariff = await _context.CountryTariffs
                .Where(t => t.TenantId == tenantId && t.IsActive)
                .Where(t => t.CountryName.ToLower() == countryName.ToLower())
                .FirstOrDefaultAsync(cancellationToken);

            if (tariff != null)
            {
                return (
                    overrideDistributionFee ?? tariff.DistributionFee,
                    overrideTransmissionFee ?? tariff.TransmissionFee,
                    overrideTaxRate ?? tariff.TaxRate
                );
            }
        }

        // Fall back to defaults
        return (
            overrideDistributionFee ?? TariffDefaults.DistributionFee,
            overrideTransmissionFee ?? TariffDefaults.TransmissionFee,
            overrideTaxRate ?? TariffDefaults.TaxRate
        );
    }
}
