using Backend.Application.Common.Interfaces;
using Backend.Application.Common.Security;

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

    public GetPowerGeneralGraphQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<PowerGeneralGraphDto>> Handle(GetPowerGeneralGraphQuery request, CancellationToken cancellationToken)
    {
        var endDate = request.EndDate ?? DateTime.UtcNow;

        // Get facility devices to aggregate power data
        var devices = await _context.Devices
            .Where(d => d.FacilityId == request.Id)
            .ToListAsync(cancellationToken);

        // For now, return empty list - this needs proper implementation
        // to aggregate power data from multiple device types' JSON payloads
        var result = new List<PowerGeneralGraphDto>();

        // TODO: Implement aggregation from DeviceSolarData, DeviceBessData, DevicePcsData, etc.
        // This requires querying each device data table and extracting power values from JSON payloads

        return result;
    }
}
