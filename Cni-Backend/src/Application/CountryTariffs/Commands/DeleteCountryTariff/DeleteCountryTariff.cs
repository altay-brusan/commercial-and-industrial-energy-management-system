using Backend.Application.Common.Interfaces;
using Backend.Application.Common.Security;

namespace Backend.Application.CountryTariffs.Commands.DeleteCountryTariff;

[Authorize(Roles = "Administrator,Tenant.Admin")]
public record DeleteCountryTariffCommand(int Id) : IRequest;

public class DeleteCountryTariffCommandHandler : IRequestHandler<DeleteCountryTariffCommand>
{
    private readonly IApplicationDbContext _context;

    public DeleteCountryTariffCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task Handle(DeleteCountryTariffCommand request, CancellationToken cancellationToken)
    {
        var entity = await _context.CountryTariffs
            .FindAsync(new object[] { request.Id }, cancellationToken);

        Guard.Against.NotFound(request.Id, entity);

        _context.CountryTariffs.Remove(entity);

        await _context.SaveChangesAsync(cancellationToken);
    }
}
