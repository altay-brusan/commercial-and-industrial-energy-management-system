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
        var pcsDevices = await _context.Devices
            .Include(d => d.DeviceType)
            .Where(d => d.DeviceType != null && d.DeviceType.Name == "PCS" && d.Specifications != null)
            .ToListAsync(cancellationToken);

        var productTypes = pcsDevices.Select(d =>
        {
            object? specs = null;
            if (!string.IsNullOrWhiteSpace(d.Specifications))
            {
                try { specs = JsonSerializer.Deserialize<object>(d.Specifications); }
                catch { /* ignore invalid JSON */ }
            }

            return new ProductTypeSchemaDto
            {
                DeviceId = d.Id,
                ModelName = d.Model,
                Manufacturer = d.Manufacturer,
                Specifications = specs
            };
        }).ToList();

        return new SchemaResponseDto { ProductTypes = productTypes };
    }
}
