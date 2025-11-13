using Backend.Application.Common.Interfaces;
using Backend.Application.Common.Security;
using Backend.Domain.Entities;
using Backend.Domain.Events;

namespace Backend.Application.Plans.Commands.CreatePlan;

[Authorize(Roles = "Administrator")]
public record CreatePlanCommand : IRequest<int>
{
    public string Name { get; set; } = string.Empty;
    public string Modules { get; set; } = string.Empty; // JSON veya CSV formatında tutulabilir
    public bool ApiAccess { get; set; }
    public int MaxFacilities { get; set; }
    public int MaxUsers { get; set; }
    public decimal Price { get; set; }
}

public class CreatePlanCommandValidator : AbstractValidator<CreatePlanCommand>
{
    public CreatePlanCommandValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Modules).MaximumLength(500);
        RuleFor(x => x.MaxFacilities).GreaterThanOrEqualTo(0);
        RuleFor(x => x.MaxUsers).GreaterThanOrEqualTo(0);
        RuleFor(x => x.Price).GreaterThanOrEqualTo(0);
    }
}

public class CreatePlanCommandHandler : IRequestHandler<CreatePlanCommand, int>
{
    private readonly IApplicationDbContext _context;
    public CreatePlanCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<int> Handle(CreatePlanCommand request, CancellationToken cancellationToken)
    {
        var entity = new Plan
        {
            Name = request.Name,
            Modules = request.Modules,
            ApiAccess = request.ApiAccess,
            MaxFacilities = request.MaxFacilities,
            MaxUsers = request.MaxUsers,
            Price = request.Price
        };

        entity.AddDomainEvent(new PlanCreatedEvent(entity));

        _context.Plans.Add(entity);

        await _context.SaveChangesAsync(cancellationToken);
        return entity.Id;
    }
}
