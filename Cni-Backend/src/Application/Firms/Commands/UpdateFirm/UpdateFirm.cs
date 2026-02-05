using Backend.Application.Common.Interfaces;
using Backend.Application.Common.Security;

namespace Backend.Application.Firms.Commands.UpdateFirm;
[Authorize(Roles = "Administrator,Tenant.Admin")]
public record UpdateFirmCommand : IRequest
{
    public int Id { get; set; }
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

public class UpdateFirmCommandHandler : IRequestHandler<UpdateFirmCommand>
{
    private readonly IApplicationDbContext _context;

    public UpdateFirmCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task Handle(UpdateFirmCommand request, CancellationToken cancellationToken)
    {
        var entity = await _context.Firms
           .FindAsync(new object[] { request.Id }, cancellationToken);

        Guard.Against.NotFound(request.Id, entity);

        entity.Name = request.Name ?? entity.Name;
        entity.LogoUrl = request.LogoUrl ?? entity.LogoUrl;
        entity.Type = request.Type ?? entity.Type;
        entity.Country = request.Country ?? entity.Country;
        entity.State = request.State ?? entity.State;
        entity.City = request.City ?? entity.City;
        entity.District = request.District ?? entity.District;
        entity.PostalCode = request.PostalCode ?? entity.PostalCode;
        entity.Address = request.Address ?? entity.Address;
        entity.ContactEmail = request.ContactEmail ?? entity.ContactEmail;
        entity.ContactPhone = request.ContactPhone ?? entity.ContactPhone;
        entity.Fax = request.Fax ?? entity.Fax;
        entity.TaxNumber = request.TaxNumber ?? entity.TaxNumber;
        entity.TaxOffice = request.TaxOffice ?? entity.TaxOffice;

        await _context.SaveChangesAsync(cancellationToken);
    }
}
