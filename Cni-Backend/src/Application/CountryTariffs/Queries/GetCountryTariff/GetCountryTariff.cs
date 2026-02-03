using Backend.Application.Common.Interfaces;
using Backend.Application.Common.Security;
using Backend.Application.CountryTariffs.Queries.GetCountryTariffs;

namespace Backend.Application.CountryTariffs.Queries.GetCountryTariff;

[Authorize(Roles = "Administrator,Tenant.Admin")]
public record GetCountryTariffQuery : IRequest<CountryTariffDto>
{
    public int Id { get; set; }
}

public class GetCountryTariffQueryHandler : IRequestHandler<GetCountryTariffQuery, CountryTariffDto>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;

    public GetCountryTariffQueryHandler(IApplicationDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<CountryTariffDto> Handle(GetCountryTariffQuery request, CancellationToken cancellationToken)
    {
        var entity = await _context.CountryTariffs
            .FindAsync(new object[] { request.Id }, cancellationToken);

        Guard.Against.NotFound(request.Id, entity);

        return _mapper.Map<CountryTariffDto>(entity);
    }
}
