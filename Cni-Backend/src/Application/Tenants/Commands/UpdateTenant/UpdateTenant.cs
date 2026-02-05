using Backend.Application.Common.Interfaces;
using Backend.Application.Common.Security;

namespace Backend.Application.Tenants.Commands.UpdateTenant;

[Authorize(Roles = "Administrator")]
public record UpdateTenantCommand : IRequest
{
    public int Id { get; set; }
    public string? Name { get; init; }
    public string? Identifier { get; init; }
}

public class UpdateTenantCommandValidator : AbstractValidator<UpdateTenantCommand>
{
    public UpdateTenantCommandValidator()
    {
        RuleFor(v => v.Name)
            .MaximumLength(500)
            .NotEmpty();
        RuleFor(v => v.Identifier)
            .MaximumLength(250);
    }
}

public class UpdateTenantCommandHandler : IRequestHandler<UpdateTenantCommand>
{
    private readonly IApplicationDbContext _context;

    public UpdateTenantCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task Handle(UpdateTenantCommand request, CancellationToken cancellationToken)
    {
        var entity = await _context.Tenants
            .FindAsync(new object[] { request.Id }, cancellationToken);

        Guard.Against.NotFound(request.Id, entity);

        entity.Name = request.Name;
        if (!string.IsNullOrEmpty(request.Identifier))
        {
            entity.Identifier = request.Identifier;
        }

        await _context.SaveChangesAsync(cancellationToken);
    }
}
