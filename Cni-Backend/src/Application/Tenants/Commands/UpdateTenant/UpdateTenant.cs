using Backend.Application.Common.Interfaces;
using Backend.Application.Common.Security;

namespace Backend.Application.Tenants.Commands.UpdateTenant;
[Authorize(Roles = "Administrator")]
public record UpdateTenantCommand : IRequest
{
    public int Id { get; set; }
    public string? Name { get; init; }
    public string? ConnectionString { get; init; }
    public string? Items { get; init; }
}

public class UpdateTenantCommandValidator : AbstractValidator<UpdateTenantCommand>
{
    public UpdateTenantCommandValidator()
    {
        RuleFor(v => v.Name)
            .MaximumLength(500)
            .NotEmpty();
        RuleFor(v => v.ConnectionString)
            .MaximumLength(1000);
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
        entity.ConnectionString = request.ConnectionString;
        entity.Items = request.Items;

        await _context.SaveChangesAsync(cancellationToken);
    }
}
