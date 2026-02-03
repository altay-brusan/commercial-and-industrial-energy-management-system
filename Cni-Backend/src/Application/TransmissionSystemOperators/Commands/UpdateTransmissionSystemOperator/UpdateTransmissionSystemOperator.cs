using Backend.Application.Common.Interfaces;
using Backend.Application.Common.Security;

namespace Backend.Application.TransmissionSystemOperators.Commands.UpdateTransmissionSystemOperator;

[Authorize(Roles = "Administrator,Tenant.Admin")]
public record UpdateTransmissionSystemOperatorCommand : IRequest
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public string BaseUrl { get; set; } = string.Empty;
    public string? ApiKey { get; set; }
    public int TimeResolutionMinutes { get; set; } = 60;
    public string Currency { get; set; } = "EUR";
    public string? AreaCode { get; set; }
    public bool IsActive { get; set; } = true;
}

public class UpdateTransmissionSystemOperatorCommandValidator : AbstractValidator<UpdateTransmissionSystemOperatorCommand>
{
    public UpdateTransmissionSystemOperatorCommandValidator()
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

        RuleFor(v => v.TimeResolutionMinutes)
            .GreaterThan(0);

        RuleFor(v => v.Currency)
            .NotEmpty()
            .MaximumLength(10);
    }
}

public class UpdateTransmissionSystemOperatorCommandHandler : IRequestHandler<UpdateTransmissionSystemOperatorCommand>
{
    private readonly IApplicationDbContext _context;

    public UpdateTransmissionSystemOperatorCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task Handle(UpdateTransmissionSystemOperatorCommand request, CancellationToken cancellationToken)
    {
        var entity = await _context.TransmissionSystemOperators
            .FindAsync(new object[] { request.Id }, cancellationToken);

        Guard.Against.NotFound(request.Id, entity);

        entity.Name = request.Name;
        entity.Code = request.Code;
        entity.BaseUrl = request.BaseUrl;
        entity.ApiKey = request.ApiKey;
        entity.TimeResolutionMinutes = request.TimeResolutionMinutes;
        entity.Currency = request.Currency;
        entity.AreaCode = request.AreaCode;
        entity.IsActive = request.IsActive;

        await _context.SaveChangesAsync(cancellationToken);
    }
}
