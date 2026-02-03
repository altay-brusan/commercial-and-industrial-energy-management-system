using Backend.Application.Common.Interfaces;
using Backend.Application.Common.Mappings;
using Backend.Application.Common.Models;
using Backend.Application.Common.Security;

namespace Backend.Application.CountryTariffs.Queries.GetCountryTariffs;

[Authorize(Roles = "Administrator,Tenant.Admin")]
public record GetCountryTariffsQuery : IRequest<PaginatedList<CountryTariffDto>>
{
    public int PageNumber { get; init; } = 1;
    public int PageSize { get; init; } = 10;
    public bool? IsActive { get; set; }
    public string? CountryCode { get; set; }
}

public class GetCountryTariffsQueryHandler : IRequestHandler<GetCountryTariffsQuery, PaginatedList<CountryTariffDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;

    public GetCountryTariffsQueryHandler(IApplicationDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<PaginatedList<CountryTariffDto>> Handle(GetCountryTariffsQuery request, CancellationToken cancellationToken)
    {
        var query = _context.CountryTariffs.AsQueryable();

        if (request.IsActive.HasValue)
        {
            query = query.Where(x => x.IsActive == request.IsActive.Value);
        }

        if (!string.IsNullOrWhiteSpace(request.CountryCode))
        {
            var countryCode = request.CountryCode.ToUpperInvariant();
            query = query.Where(x => x.CountryCode == countryCode);
        }

        return await query
            .OrderBy(x => x.CountryName)
            .ProjectTo<CountryTariffDto>(_mapper.ConfigurationProvider)
            .PaginatedListAsync(request.PageNumber, request.PageSize);
    }
}
