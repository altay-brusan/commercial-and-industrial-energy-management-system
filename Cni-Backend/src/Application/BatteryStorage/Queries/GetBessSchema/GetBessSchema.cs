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
        var batteryDevices = await _context.Devices
            .Include(d => d.DeviceType)
            .Where(d => d.DeviceType != null && d.DeviceType.Name == "Battery" && d.Specifications != null)
            .ToListAsync(cancellationToken);

        var productTypes = batteryDevices.Select(d =>
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
