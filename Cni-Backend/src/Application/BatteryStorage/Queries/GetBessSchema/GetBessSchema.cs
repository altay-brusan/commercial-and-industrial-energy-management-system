using System.Text.Json;
using Backend.Application.Common.Interfaces;
using Backend.Application.Common.Models.Monitoring;
using Backend.Application.Common.Security;

namespace Backend.Application.BatteryStorage.Queries.GetBessSchema;

[Authorize(Roles = "Administrator,Tenant.Admin")]
[RequirePermission(Backend.Domain.Constants.Permissions.BessView)]
public record GetBessSchemaQuery : IRequest<SchemaResponseDto>;

public class GetBessSchemaQueryHandler : IRequestHandler<GetBessSchemaQuery, SchemaResponseDto>
{
    private readonly IApplicationDbContext _context;

    public GetBessSchemaQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<SchemaResponseDto> Handle(GetBessSchemaQuery request, CancellationToken cancellationToken)
    {
        var batteryModels = await _context.DeviceModels
            .Include(dm => dm.DeviceType)
            .Where(dm => dm.DeviceType != null && dm.DeviceType.Name == "Battery")
            .ToListAsync(cancellationToken);

        var productTypes = batteryModels.Select(dm =>
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
