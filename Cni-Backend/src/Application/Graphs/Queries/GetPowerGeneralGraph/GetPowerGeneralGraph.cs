using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Intrinsics.X86;
using System.Text;
using System.Threading.Tasks;
using Backend.Application.Common.Interfaces;
using Backend.Application.Common.Security;
using Backend.Application.Graphs.Queries.GetPowerGeneralGraph;

namespace Backend.Application.Graphs.Queries.GetPowerGeneralGraph;

[Authorize(Roles = "Administrator,Tenant.Admin")]
public record GetPowerGeneralGraphQuery : IRequest<List<PowerGeneralGraphDto>>
{
    public int Id { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime? EndDate { get; set; }
}

public class GetPowerGeneralGraphQueryValidator : AbstractValidator<GetPowerGeneralGraphQuery>
{
    public GetPowerGeneralGraphQueryValidator()
    {
    }
}

public class GetPowerGeneralGraphQueryHandler : IRequestHandler<GetPowerGeneralGraphQuery, List<PowerGeneralGraphDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;
    private readonly IExternalDataService _externalDataService;
    public GetPowerGeneralGraphQueryHandler(IApplicationDbContext context, IMapper mapper, IExternalDataService externalDataService)
    {
        _context = context;
        _mapper = mapper;
        _externalDataService = externalDataService;
    }

    public async Task<List<PowerGeneralGraphDto>> Handle(GetPowerGeneralGraphQuery request, CancellationToken cancellationToken)
    {
        var endDate = request.EndDate ?? DateTime.UtcNow;
        var powerGeneralData = await _context.GrpcFacilityDatas
            .Where(f => f.FacilityId == request.Id &&
                       f.Created >= request.StartDate &&
                       f.Created <= endDate &&
                (f.Created.Minute % 5) == 0)
            .OrderBy(f => f.Created) // Zaman sıralı olsun
            .AsNoTracking() // Performans için
            .ToListAsync(cancellationToken);


        var result = _mapper.Map<List<PowerGeneralGraphDto>>(powerGeneralData); // Değişti

        return result;


    }
}
