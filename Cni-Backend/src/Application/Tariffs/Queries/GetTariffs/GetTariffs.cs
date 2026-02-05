using Backend.Application.Common.Interfaces;
using Backend.Application.Common.Mappings;
using Backend.Application.Common.Models;
using Backend.Application.Common.Security;

namespace Backend.Application.Tariffs.Queries.GetTariffs;

/// <summary>
/// Gets a paginated list of tariffs. Can filter by active status.
/// </summary>
[Authorize(Roles = "Administrator")]
public record GetTariffsQuery : IRequest<PaginatedList<TariffBriefDto>>
{
    public int PageNumber { get; init; } = 1;
    public int PageSize { get; init; } = 10;

    /// <summary>
    /// Filter by active status. If null, returns all tariffs.
    /// </summary>
    public bool? IsActive { get; init; }

    /// <summary>
    /// Filter by country name (case-insensitive partial match).
    /// </summary>
    public string? CountryName { get; init; }
}

public class GetTariffsQueryHandler : IRequestHandler<GetTariffsQuery, PaginatedList<TariffBriefDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;

    public GetTariffsQueryHandler(IApplicationDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<PaginatedList<TariffBriefDto>> Handle(GetTariffsQuery request, CancellationToken cancellationToken)
    {
        var query = _context.Tariffs.AsQueryable();

        // Apply filters
        if (request.IsActive.HasValue)
        {
            query = query.Where(t => t.IsActive == request.IsActive.Value);
        }

        if (!string.IsNullOrWhiteSpace(request.CountryName))
        {
            query = query.Where(t => t.CountryName.ToLower().Contains(request.CountryName.ToLower()));
        }

        // Order by country name, then by created date descending (newest first)
        query = query.OrderBy(t => t.CountryName).ThenByDescending(t => t.Created);

        return await query
            .ProjectTo<TariffBriefDto>(_mapper.ConfigurationProvider)
            .PaginatedListAsync(request.PageNumber, request.PageSize);
    }
}
