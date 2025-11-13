using Backend.Application.Common.Interfaces;
using Backend.Application.Common.Security;

namespace Backend.Application.Topologies.Queries.GetTopology;
[Authorize(Roles = "Administrator,Tenant.Admin")]
public record GetTopologyQuery : IRequest<TopologyDto?>
{
    public int FacilityId { get; set; }
}

public class GetTopologyQueryValidator : AbstractValidator<GetTopologyQuery>
{
    public GetTopologyQueryValidator()
    {
        RuleFor(x => x.FacilityId).NotEmpty().WithMessage("FacilityId is required.");
    }
}

public class GetTopologyQueryHandler : IRequestHandler<GetTopologyQuery, TopologyDto?>
{
    private readonly IApplicationDbContext _context; 
    private readonly IMapper _mapper;

    public GetTopologyQueryHandler(IApplicationDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<TopologyDto?> Handle(GetTopologyQuery request, CancellationToken cancellationToken)
    {
        var topologyDto = new TopologyDto();

        var nodes = await _context.TopologyNodes
            .Where(x => x.FacilityId == request.FacilityId)
            .ProjectTo<TopologyNodeDto>(_mapper.ConfigurationProvider)
            .ToListAsync(cancellationToken);

        var edges = await _context.TopologyEdges
            .Where(x => x.FacilityId == request.FacilityId)
            .ProjectTo<TopologyEdgeDto>(_mapper.ConfigurationProvider)
            .ToListAsync(cancellationToken);

        // DTO'ya ekle
        topologyDto.Nodes = nodes;
        topologyDto.Edges = edges;
        return topologyDto;
    }
}
