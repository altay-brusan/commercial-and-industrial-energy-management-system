using System.Text.Json;
using Backend.Application.Common.Interfaces;
using Backend.Application.Common.Models.Monitoring;
using Backend.Application.Common.Security;

namespace Backend.Application.Pcs.Queries.GetPcsSchema;

[Authorize(Roles = "Administrator,Tenant.Admin")]
[RequirePermission(Backend.Domain.Constants.Permissions.PcsView)]
public record GetPcsSchemaQuery : IRequest<SchemaResponseDto>;

public class GetPcsSchemaQueryHandler : IRequestHandler<GetPcsSchemaQuery, SchemaResponseDto>
{
    private readonly IApplicationDbContext _context;

    public GetPcsSchemaQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<SchemaResponseDto> Handle(GetPcsSchemaQuery request, CancellationToken cancellationToken)
    {
        var pcsModels = await _context.DeviceModels
            .Include(dm => dm.DeviceType)
            .Where(dm => dm.DeviceType != null && dm.DeviceType.Name == "PCS")
            .ToListAsync(cancellationToken);

        var productTypes = pcsModels.Select(dm =>
        {
            object? specs = null;
            if (!string.IsNullOrWhiteSpace(dm.Specifications))
            {
                try { specs = JsonSerializer.Deserialize<object>(dm.Specifications); }
                catch { /* ignore invalid JSON */ }
            }

            return new ProductTypeSchemaDto
            {
                DeviceModelId = dm.Id,
                ModelName = dm.Name,
                Manufacturer = dm.Manufacturer,
                Specifications = specs
            };
        }).ToList();

        return new SchemaResponseDto { ProductTypes = productTypes };
    }
}
