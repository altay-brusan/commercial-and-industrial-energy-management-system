using Backend.Application.Common.Interfaces;
using Backend.Application.Common.Security;

namespace Backend.Application.DeviceSpecificationTemplates.Queries;

[Authorize(Roles = "Administrator,Tenant.Admin")]
[RequirePermission(Backend.Domain.Constants.Permissions.DeviceView)]
public record GetDeviceSpecificationTemplateQuery : IRequest<DeviceSpecificationTemplateDto>
{
    public Guid Id { get; init; }
}

public class GetDeviceSpecificationTemplateQueryHandler : IRequestHandler<GetDeviceSpecificationTemplateQuery, DeviceSpecificationTemplateDto>
{
    private readonly IApplicationDbContext _context;

    public GetDeviceSpecificationTemplateQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<DeviceSpecificationTemplateDto> Handle(GetDeviceSpecificationTemplateQuery request, CancellationToken cancellationToken)
    {
        var entity = await _context.DeviceSpecificationTemplates
            .Include(t => t.DeviceType)
            .FirstOrDefaultAsync(t => t.Id == request.Id, cancellationToken);

        Guard.Against.NotFound(request.Id, entity);

        return new DeviceSpecificationTemplateDto
        {
            Id = entity.Id,
            DeviceTypeId = entity.DeviceTypeId,
            DeviceTypeName = entity.DeviceType?.Name ?? string.Empty,
            Model = entity.Model,
            Manufacturer = entity.Manufacturer,
            SpecificationsTemplate = entity.SpecificationsTemplate,
            MetadataTemplate = entity.MetadataTemplate,
            HierarchyTemplate = entity.HierarchyTemplate,
            IsDefault = entity.IsDefault
        };
    }
}
