using Backend.Application.Common.Interfaces;
using Backend.Application.Common.Security;

namespace Backend.Application.Plans.Commands.UpdatePlan;
[Authorize(Roles = "Administrator")]
public record UpdatePlanCommand : IRequest
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Modules { get; set; } = string.Empty; // JSON veya CSV formatında tutulabilir
    public bool ApiAccess { get; set; }
    public int MaxFacilities { get; set; }
    public int MaxUsers { get; set; }
    public decimal Price { get; set; }
}

public class UpdatePlanCommandValidator : AbstractValidator<UpdatePlanCommand>
{
    public UpdatePlanCommandValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Modules).MaximumLength(500);
        RuleFor(x => x.MaxFacilities).GreaterThanOrEqualTo(0);
        RuleFor(x => x.MaxUsers).GreaterThanOrEqualTo(0);
        RuleFor(x => x.Price).GreaterThanOrEqualTo(0);
    }
}

public class UpdatePlanCommandHandler : IRequestHandler<UpdatePlanCommand>
{
    private readonly IApplicationDbContext _context;

    public UpdatePlanCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task Handle(UpdatePlanCommand request, CancellationToken cancellationToken)
    {
        var entity = await _context.Plans
           .FindAsync(new object[] { request.Id }, cancellationToken);

        Guard.Against.NotFound(request.Id, entity);

        entity.Name = request.Name;

        entity.Modules = request.Modules;
        entity.ApiAccess = request.ApiAccess;
        entity.MaxFacilities = request.MaxFacilities;
        entity.MaxUsers = request.MaxUsers;
        entity.Price = request.Price;
        await _context.SaveChangesAsync(cancellationToken);
    }
}
