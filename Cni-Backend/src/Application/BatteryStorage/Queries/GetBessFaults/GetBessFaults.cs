using Backend.Application.Common.Interfaces;
using Backend.Application.Common.Models.Monitoring;
using Backend.Application.Common.Security;

namespace Backend.Application.BatteryStorage.Queries.GetBessFaults;

[Authorize(Roles = "Administrator,Tenant.Admin")]
[RequirePermission(Backend.Domain.Constants.Permissions.BessView)]
public record GetBessFaultsQuery : IRequest<FaultsPaginatedResponseDto>
{
    public Guid UnitId { get; init; }
    public int PageNumber { get; init; } = 1;
    public int PageSize { get; init; } = 20;
    public string? Severity { get; init; }
    public bool? IsActive { get; init; }
}

public class GetBessFaultsQueryHandler : IRequestHandler<GetBessFaultsQuery, FaultsPaginatedResponseDto>
{
    private readonly IApplicationDbContext _context;

    public GetBessFaultsQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<FaultsPaginatedResponseDto> Handle(GetBessFaultsQuery request, CancellationToken cancellationToken)
    {
        var query = _context.DeviceAlarms
            .Where(a => a.DeviceId == request.UnitId);

        if (!string.IsNullOrWhiteSpace(request.Severity))
        {
            query = query.Where(a => a.Severity == request.Severity);
        }

        if (request.IsActive.HasValue)
        {
            query = query.Where(a => a.IsActive == request.IsActive.Value);
        }

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .OrderByDescending(a => a.OccurredAt)
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(a => new FaultRecordDto
            {
                Id = a.Id,
                Code = a.Code,
                Name = a.Name,
                Severity = a.Severity,
                Category = a.Category,
                Source = a.Source,
                OccurredAt = a.OccurredAt,
                RecoveredAt = a.RecoveredAt,
                IsActive = a.IsActive,
                Details = a.Details
            })
            .ToListAsync(cancellationToken);

        return new FaultsPaginatedResponseDto
        {
            Items = items,
            PageNumber = request.PageNumber,
            TotalPages = (int)Math.Ceiling(totalCount / (double)request.PageSize),
            TotalCount = totalCount
        };
    }
}
