using Backend.Application.Common.Interfaces;
using Backend.Application.Common.Security;
using Backend.Domain.Entities;
using Backend.Domain.Events;

namespace Backend.Application.Firms.Commands.CreateFirm;
[Authorize(Roles = "Administrator,Tenant.Admin")]
public record CreateFirmCommand : IRequest<int>
{
    public int TenantId { get; set; }
    public string? Name { get; set; }
    public string? LogoUrl { get; set; }
    public string? Type { get; set; }
    public string? Country { get; set; }
    public string? State { get; set; }
    public string? City { get; set; }
    public string? District { get; set; }
    public string? PostalCode { get; set; }
    public string? Address { get; set; }
    public string? ContactEmail { get; set; }
    public string? ContactPhone { get; set; }
    public string? Fax { get; set; }
    public string? TaxNumber { get; set; }
    public string? TaxOffice { get; set; }
}

public class CreateFirmCommandValidator : AbstractValidator<CreateFirmCommand>
{
    public CreateFirmCommandValidator()
    {
        RuleFor(v => v.Name)
             .NotEmpty()
             .MaximumLength(500);

        RuleFor(v => v.LogoUrl)
            .MaximumLength(1000);

        RuleFor(v => v.Type)
            .MaximumLength(100);

        RuleFor(v => v.Country)
            .MaximumLength(100)
            .NotEmpty();

        RuleFor(v => v.State)
            .MaximumLength(100)
            .NotEmpty();

        RuleFor(v => v.City)
            .MaximumLength(100)
            .NotEmpty();

        RuleFor(v => v.District)
            .MaximumLength(100);

        RuleFor(v => v.PostalCode)
            .MaximumLength(20);

        RuleFor(v => v.Address)
            .NotEmpty()
            .MaximumLength(1000);

        RuleFor(v => v.ContactEmail)
            .NotEmpty()
            .MaximumLength(255)
            .EmailAddress();

        RuleFor(v => v.ContactPhone)
            .NotEmpty()
            .MaximumLength(50);

        RuleFor(v => v.Fax)
            .MaximumLength(50);

        RuleFor(v => v.TaxNumber)
            .NotEmpty()
            .MaximumLength(50);

        RuleFor(v => v.TaxOffice)
            .NotEmpty()
            .MaximumLength(100);
    }
}

public class CreateFirmCommandHandler : IRequestHandler<CreateFirmCommand, int>
{
    private readonly IApplicationDbContext _context;
    private readonly IMultiTenantService _multiTenantService;

    public CreateFirmCommandHandler(IApplicationDbContext context, IMultiTenantService multiTenantService)
    {
        _context = context;
        _multiTenantService = multiTenantService;
    }

    public async Task<int> Handle(CreateFirmCommand request, CancellationToken cancellationToken)
    {
        var tenantId = _multiTenantService.GetTenantIdForCreation(request.TenantId > 0 ? request.TenantId : null);

        var entity = new Firm
        {
            TenantId = tenantId,
            Name = request.Name,
            LogoUrl = request.LogoUrl,
            Type = request.Type,
            Country = request.Country,
            State = request.State,
            City = request.City,
            District = request.District,
            PostalCode = request.PostalCode,
            Address = request.Address,
            ContactEmail = request.ContactEmail,
            ContactPhone = request.ContactPhone,
            Fax = request.Fax,
            TaxNumber = request.TaxNumber,
            TaxOffice = request.TaxOffice
        };
        entity.AddDomainEvent(new FirmCreatedEvent(entity));

        _context.Firms.Add(entity);

        await _context.SaveChangesAsync(cancellationToken);
        return entity.Id;
    }
}
