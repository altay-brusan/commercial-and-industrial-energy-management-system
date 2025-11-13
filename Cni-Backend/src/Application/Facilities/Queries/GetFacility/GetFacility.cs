using Backend.Application.Common.Interfaces;
using Backend.Application.Common.Security;

namespace Backend.Application.Facilities.Queries.GetFacility;
[Authorize(Roles = "Administrator,Tenant.Admin")]
public record GetFacilityQuery : IRequest<FacilityDto?>
{
    public int Id { get; set; }
}

public class GetFacilityQueryValidator : AbstractValidator<GetFacilityQuery>
{
    public GetFacilityQueryValidator()
    {
    }
}

public class GetFacilityQueryHandler : IRequestHandler<GetFacilityQuery, FacilityDto?>
{
    private readonly IApplicationDbContext _context; 
    private readonly IMapper _mapper;

    public GetFacilityQueryHandler(IApplicationDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<FacilityDto?> Handle(GetFacilityQuery request, CancellationToken cancellationToken)
    {
        return await _context.Facilities.Where(x => x.Id == request.Id)
            .ProjectTo<FacilityDto>(_mapper.ConfigurationProvider)
            .FirstOrDefaultAsync(cancellationToken); 
    }
}
