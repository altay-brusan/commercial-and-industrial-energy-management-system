using Backend.Application.Common.Interfaces;
using Backend.Application.Common.Security;

namespace Backend.Application.Tariffs.Queries.GetTariff;

/// <summary>
/// Gets a single tariff by ID.
/// </summary>
[Authorize(Roles = "Administrator")]
public record GetTariffQuery : IRequest<TariffDto?>
{
    public int Id { get; init; }
}

public class GetTariffQueryHandler : IRequestHandler<GetTariffQuery, TariffDto?>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;

    public GetTariffQueryHandler(IApplicationDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<TariffDto?> Handle(GetTariffQuery request, CancellationToken cancellationToken)
    {
        var tariff = await _context.Tariffs
            .Where(t => t.Id == request.Id)
            .ProjectTo<TariffDto>(_mapper.ConfigurationProvider)
            .FirstOrDefaultAsync(cancellationToken);

        return tariff;
    }
}
