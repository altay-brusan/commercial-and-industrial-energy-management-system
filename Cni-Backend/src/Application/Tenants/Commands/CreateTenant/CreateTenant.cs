using Backend.Application.Common.Interfaces;
using Backend.Application.Common.Security;
using Backend.Domain.Entities;
using Backend.Domain.Events;

namespace Backend.Application.Tenants.Commands.CreateTenant;

[Authorize(Roles = "Administrator")]
public record CreateTenantCommand : IRequest<int>
{
    public string? Name { get; init; }
    public string? Identifier { get; init; }
}

public class CreateTenantCommandHandler : IRequestHandler<CreateTenantCommand, int>
{
    private readonly IApplicationDbContext _context;

    public CreateTenantCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<int> Handle(CreateTenantCommand request, CancellationToken cancellationToken)
    {
        var entity = new Tenant
        {
            Name = request.Name,
            Identifier = request.Identifier ?? Guid.NewGuid().ToString()
        };

        entity.AddDomainEvent(new TenantCreatedEvent(entity));

        _context.Tenants.Add(entity);

        await _context.SaveChangesAsync(cancellationToken);
        return entity.Id;
    }
}
