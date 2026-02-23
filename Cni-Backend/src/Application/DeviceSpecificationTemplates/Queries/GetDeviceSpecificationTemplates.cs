using Backend.Application.Common.Interfaces;
using Backend.Application.Common.Security;

namespace Backend.Application.DeviceSpecificationTemplates.Queries;

[Authorize(Roles = "Administrator,Tenant.Admin")]
[RequirePermission(Backend.Domain.Constants.Permissions.DeviceView)]
public record GetDeviceSpecificationTemplatesQuery : IRequest<List<DeviceSpecificationTemplateDto>>
{
    public Guid? DeviceTypeId { get; init; }
}

public class GetDeviceSpecificationTemplatesQueryHandler : IRequestHandler<GetDeviceSpecificationTemplatesQuery, List<DeviceSpecificationTemplateDto>>
{
    private readonly IApplicationDbContext _context;

    public GetDeviceSpecificationTemplatesQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<DeviceSpecificationTemplateDto>> Handle(GetDeviceSpecificationTemplatesQuery request, CancellationToken cancellationToken)
    {
        var query = _context.DeviceSpecificationTemplates
            .Include(t => t.DeviceType)
            .AsQueryable();

        if (request.DeviceTypeId.HasValue)
            query = query.Where(t => t.DeviceTypeId == request.DeviceTypeId.Value);

        return await query
            .OrderBy(t => t.DeviceType!.Name)
            .ThenBy(t => t.Model)
            .Select(t => new DeviceSpecificationTemplateDto
            {
                Id = t.Id,
                DeviceTypeId = t.DeviceTypeId,
                DeviceTypeName = t.DeviceType!.Name,
                Model = t.Model,
                Manufacturer = t.Manufacturer,
                SpecificationsTemplate = t.SpecificationsTemplate,
                MetadataTemplate = t.MetadataTemplate,
                HierarchyTemplate = t.HierarchyTemplate,
                IsDefault = t.IsDefault
            })
            .ToListAsync(cancellationToken);
    }
}
