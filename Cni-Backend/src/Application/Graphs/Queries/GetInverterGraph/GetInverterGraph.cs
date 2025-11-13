using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Backend.Application.Common.Interfaces;
using Backend.Application.Common.Security;

namespace Backend.Application.Graphs.Queries.GetInverterGraph;

[Authorize(Roles = "Administrator,Tenant.Admin")]
public record GetInverterGraphQuery : IRequest<List<InverterGraphDto>>
{
    public int Id { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime? EndDate { get; set; }
}

public class GetInverterGraphQueryValidator : AbstractValidator<GetInverterGraphQuery>
{
    public GetInverterGraphQueryValidator()
    {
    }
}

public class GetInverterGraphQueryHandler : IRequestHandler<GetInverterGraphQuery, List<InverterGraphDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;
    private readonly IExternalDataService _externalDataService;
    public GetInverterGraphQueryHandler(IApplicationDbContext context, IMapper mapper, IExternalDataService externalDataService)
    {
        _context = context;
        _mapper = mapper;
        _externalDataService = externalDataService;
    }

    public async Task<List<InverterGraphDto>> Handle(GetInverterGraphQuery request, CancellationToken cancellationToken)
    {

        // EndDate null ise şu anki zamanı ata
        var endDate = request.EndDate ?? DateTime.UtcNow;

        // Veritabanından filtreli verileri çek
        var pcsData = await _context.PCSDatas
            .Where(f => f.PCSId == request.Id &&
                       f.Created >= request.StartDate &&
                       f.Created <= endDate &&
                (f.Created.Minute % 5) == 0)
            .OrderBy(f => f.Created) // Zaman sıralı olsun
            .AsNoTracking() // Performans için
            .ToListAsync(cancellationToken);

     
        var result = _mapper.Map<List<InverterGraphDto>>(pcsData); // Değişti

        return result;
    }
}
