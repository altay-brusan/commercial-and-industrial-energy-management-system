using Backend.Application.Common.Interfaces;
using Backend.Application.Common.Security;

namespace Backend.Application.TransmissionSystemOperators.Queries.GetTransmissionSystemOperator;

[Authorize(Roles = "Administrator,Tenant.Admin")]
public record GetTransmissionSystemOperatorQuery : IRequest<TransmissionSystemOperatorDto?>
{
    public int Id { get; set; }
}

public class GetTransmissionSystemOperatorQueryValidator : AbstractValidator<GetTransmissionSystemOperatorQuery>
{
    public GetTransmissionSystemOperatorQueryValidator()
    {
    }
}

public class GetTransmissionSystemOperatorQueryHandler : IRequestHandler<GetTransmissionSystemOperatorQuery, TransmissionSystemOperatorDto?>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;

    public GetTransmissionSystemOperatorQueryHandler(IApplicationDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<TransmissionSystemOperatorDto?> Handle(GetTransmissionSystemOperatorQuery request, CancellationToken cancellationToken)
    {
        return await _context.TransmissionSystemOperators
            .Where(x => x.Id == request.Id)
            .ProjectTo<TransmissionSystemOperatorDto>(_mapper.ConfigurationProvider)
            .FirstOrDefaultAsync(cancellationToken);
    }
}
