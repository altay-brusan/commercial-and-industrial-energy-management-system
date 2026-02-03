using Backend.Application.Common.Interfaces;
using Backend.Application.Common.Security;

namespace Backend.Application.CountryTariffs.Commands.UpdateCountryTariff;

[Authorize(Roles = "Administrator,Tenant.Admin")]
public record UpdateCountryTariffCommand : IRequest
{
    public int Id { get; set; }
    public string CountryCode { get; set; } = string.Empty;
    public string CountryName { get; set; } = string.Empty;
    public decimal DistributionFee { get; set; }
    public decimal TransmissionFee { get; set; }
    public decimal TaxRate { get; set; }
    public bool IsActive { get; set; } = true;
}

public class UpdateCountryTariffCommandValidator : AbstractValidator<UpdateCountryTariffCommand>
{
    public UpdateCountryTariffCommandValidator()
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

public class UpdateCountryTariffCommandHandler : IRequestHandler<UpdateCountryTariffCommand>
{
    private readonly IApplicationDbContext _context;

    public UpdateCountryTariffCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task Handle(UpdateCountryTariffCommand request, CancellationToken cancellationToken)
    {
        var entity = await _context.CountryTariffs
            .FindAsync(new object[] { request.Id }, cancellationToken);

        Guard.Against.NotFound(request.Id, entity);

        entity.CountryCode = request.CountryCode.ToUpperInvariant();
        entity.CountryName = request.CountryName;
        entity.DistributionFee = request.DistributionFee;
        entity.TransmissionFee = request.TransmissionFee;
        entity.TaxRate = request.TaxRate;
        entity.IsActive = request.IsActive;

        await _context.SaveChangesAsync(cancellationToken);
    }
}
