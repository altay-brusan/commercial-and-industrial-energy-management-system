using Backend.Application.Common.Interfaces;
using Backend.Application.Common.Security;
using Backend.Domain.Entities;
using Backend.Domain.Events;

namespace Backend.Application.TransmissionSystemOperators.Commands.CreateTransmissionSystemOperator;

[Authorize(Roles = "Administrator,Tenant.Admin")]
public record CreateTransmissionSystemOperatorCommand : IRequest<int>
{
    public int TenantId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public string BaseUrl { get; set; } = string.Empty;
    public string? ApiKey { get; set; }
    public decimal TaxRate { get; set; }
    public decimal DistributionFee { get; set; }
    public int TimeResolutionMinutes { get; set; } = 60;
    public string Currency { get; set; } = "EUR";
    public string? AreaCode { get; set; }
}

public class CreateTransmissionSystemOperatorCommandValidator : AbstractValidator<CreateTransmissionSystemOperatorCommand>
{
    public CreateTransmissionSystemOperatorCommandValidator()
    {
        RuleFor(v => v.Name)
            .NotEmpty()
            .MaximumLength(200);

        RuleFor(v => v.Code)
            .NotEmpty()
            .MaximumLength(50);

        RuleFor(v => v.BaseUrl)
            .NotEmpty()
            .MaximumLength(500);

        RuleFor(v => v.TaxRate)
            .GreaterThanOrEqualTo(0)
            .LessThanOrEqualTo(1);

        RuleFor(v => v.DistributionFee)
            .GreaterThanOrEqualTo(0);

        RuleFor(v => v.TimeResolutionMinutes)
            .GreaterThan(0);

        RuleFor(v => v.Currency)
            .NotEmpty()
            .MaximumLength(10);
    }
}

public class CreateTransmissionSystemOperatorCommandHandler : IRequestHandler<CreateTransmissionSystemOperatorCommand, int>
{
    private readonly IApplicationDbContext _context;
    private readonly int? _tenantId;
    private readonly bool _isSuperAdmin;

    public CreateTransmissionSystemOperatorCommandHandler(IApplicationDbContext context, IMultiTenantService multiTenantService)
    {
        _context = context;
        _tenantId = multiTenantService.GetCurrentTenantId();
        _isSuperAdmin = multiTenantService.IsSuperAdmin();
    }

    public async Task<int> Handle(CreateTransmissionSystemOperatorCommand request, CancellationToken cancellationToken)
    {
        var tenantId = _isSuperAdmin ? request.TenantId : _tenantId;

        if (tenantId is null)
        {
            throw new ArgumentNullException(nameof(tenantId));
        }

        var entity = new TransmissionSystemOperator
        {
            TenantId = (int)tenantId,
            Name = request.Name,
            Code = request.Code,
            BaseUrl = request.BaseUrl,
            ApiKey = request.ApiKey,
            TaxRate = request.TaxRate,
            DistributionFee = request.DistributionFee,
            TimeResolutionMinutes = request.TimeResolutionMinutes,
            Currency = request.Currency,
            AreaCode = request.AreaCode,
            IsActive = true
        };

        entity.AddDomainEvent(new TransmissionSystemOperatorCreatedEvent(entity));

        _context.TransmissionSystemOperators.Add(entity);

        await _context.SaveChangesAsync(cancellationToken);
        return entity.Id;
    }
}
