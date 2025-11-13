using Backend.Application.Common.Interfaces;
using Backend.Application.Common.Security;

namespace Backend.Application.Plans.Queries.GetPlan;
[Authorize(Roles = "Administrator")]
public record GetPlanQuery : IRequest<PlanDto?>
{
    public int Id { get; set; }
}

public class GetPlanQueryValidator : AbstractValidator<GetPlanQuery>
{
    public GetPlanQueryValidator()
    {
    }
}

public class GetPlanQueryHandler : IRequestHandler<GetPlanQuery, PlanDto?>
{
    private readonly IApplicationDbContext _context; 
    private readonly IMapper _mapper;

    public GetPlanQueryHandler(IApplicationDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<PlanDto?> Handle(GetPlanQuery request, CancellationToken cancellationToken)
    {
        return await _context.Plans.Where(x => x.Id == request.Id)
            .ProjectTo<PlanDto>(_mapper.ConfigurationProvider)
            .FirstOrDefaultAsync(cancellationToken); 
    }
}
