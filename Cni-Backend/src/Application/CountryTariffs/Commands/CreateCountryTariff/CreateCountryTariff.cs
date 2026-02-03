using Backend.Application.Common.Interfaces;
using Backend.Application.Common.Security;
using Backend.Domain.Entities;

namespace Backend.Application.CountryTariffs.Commands.CreateCountryTariff;

[Authorize(Roles = "Administrator,Tenant.Admin")]
public record CreateCountryTariffCommand : IRequest<int>
{
    public int TenantId { get; set; }
    public string CountryCode { get; set; } = string.Empty;
    public string CountryName { get; set; } = string.Empty;
    public decimal DistributionFee { get; set; } = 20m;
    public decimal TransmissionFee { get; set; } = 5m;
    public decimal TaxRate { get; set; } = 0.21m;
}

public class CreateCountryTariffCommandValidator : AbstractValidator<CreateCountryTariffCommand>
{
    public CreateCountryTariffCommandValidator()
    {
        RuleFor(v => v.CountryCode)
            .NotEmpty()
            .MaximumLength(10);

        RuleFor(v => v.CountryName)
            .NotEmpty()
            .MaximumLength(100);

        RuleFor(v => v.DistributionFee)
            .GreaterThanOrEqualTo(0)
            .WithMessage("Distribution fee must be a non-negative value.");

        RuleFor(v => v.TransmissionFee)
            .GreaterThanOrEqualTo(0)
            .WithMessage("Transmission fee must be a non-negative value.");

        RuleFor(v => v.TaxRate)
            .GreaterThanOrEqualTo(0)
            .LessThanOrEqualTo(1)
            .WithMessage("Tax rate must be between 0 and 1 (e.g., 0.21 for 21%).");
    }
}

public class CreateCountryTariffCommandHandler : IRequestHandler<CreateCountryTariffCommand, int>
{
    private readonly IApplicationDbContext _context;
    private readonly int? _tenantId;
    private readonly bool _isSuperAdmin;

    public CreateCountryTariffCommandHandler(IApplicationDbContext context, IMultiTenantService multiTenantService)
    {
        _context = context;
        _tenantId = multiTenantService.GetCurrentTenantId();
        _isSuperAdmin = multiTenantService.IsSuperAdmin();
    }

    public async Task<int> Handle(CreateCountryTariffCommand request, CancellationToken cancellationToken)
    {
        var tenantId = _isSuperAdmin ? request.TenantId : _tenantId;

        if (tenantId is null)
        {
            throw new ArgumentNullException(nameof(tenantId));
        }

        var entity = new CountryTariff
        {
            TenantId = (int)tenantId,
            CountryCode = request.CountryCode.ToUpperInvariant(),
            CountryName = request.CountryName,
            DistributionFee = request.DistributionFee,
            TransmissionFee = request.TransmissionFee,
            TaxRate = request.TaxRate,
            IsActive = true
        };

        _context.CountryTariffs.Add(entity);

        await _context.SaveChangesAsync(cancellationToken);
        return entity.Id;
    }
}
