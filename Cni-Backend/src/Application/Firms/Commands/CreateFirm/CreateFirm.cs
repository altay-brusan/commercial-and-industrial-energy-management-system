using Backend.Application.Common.Interfaces;
using Backend.Application.Common.Security;
using Backend.Domain.Entities;
using Backend.Domain.Events;

namespace Backend.Application.Firms.Commands.CreateFirm;
[Authorize(Roles = "Administrator,Tenant.Admin")]
[RequirePermission(Backend.Domain.Constants.Permissions.FirmCreate)]
public record CreateFirmCommand : IRequest<Guid>
{
    public Guid TenantId { get; set; }
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

public class CreateFirmCommandHandler : IRequestHandler<CreateFirmCommand, Guid>
{
    private readonly IApplicationDbContext _context;
    private readonly IMultiTenantService _multiTenantService;

    public CreateFirmCommandHandler(IApplicationDbContext context, IMultiTenantService multiTenantService)
    {
        _context = context;
        _multiTenantService = multiTenantService;
    }

    public async Task<Guid> Handle(CreateFirmCommand request, CancellationToken cancellationToken)
    {
        var tenantId = _multiTenantService.GetTenantIdForCreation(request.TenantId != Guid.Empty ? request.TenantId : null);

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
