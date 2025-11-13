using Backend.Application.Common.Interfaces;
using Backend.Application.Common.Security;
using Backend.Domain.Entities;
using Backend.Domain.Events;

namespace Backend.Application.Tenants.Commands.CreateTenant;

[Authorize(Roles = "Administrator")]
public record CreateTenantCommand : IRequest<int>
{
    public string? Name { get; init; }
    public string? ConnectionString { get; init; }
    public string? Items { get; init; }
}

public class CreateTenantCommandValidator : AbstractValidator<CreateTenantCommand>
{
    public CreateTenantCommandValidator()
    {
    }
}

public class CreateTenantCommandHandler : IRequestHandler<CreateTenantCommand, int>
{
    private readonly IApplicationDbContext _context;
    private readonly IEmailService _emailService;
    public CreateTenantCommandHandler(IApplicationDbContext context, IEmailService emailService)
    {
        _context = context;
        _emailService = emailService;
    }

    public async Task<int> Handle(CreateTenantCommand request, CancellationToken cancellationToken)
    {
        var entity = new Tenant
        {
            Name = request.Name,
            Identifier = Guid.NewGuid().ToString(),
            ConnectionString = request.ConnectionString,
            Items = request.Items,
        };

        entity.AddDomainEvent(new TenantCreatedEvent(entity));

        _context.Tenants.Add(entity);

        await _context.SaveChangesAsync(cancellationToken);
        await _emailService.SendEmailAsync("ilhami.aydogdu@pentaunity.com", "Welcome!", "Thank you for signing up!");
        return entity.Id;
    }
}
