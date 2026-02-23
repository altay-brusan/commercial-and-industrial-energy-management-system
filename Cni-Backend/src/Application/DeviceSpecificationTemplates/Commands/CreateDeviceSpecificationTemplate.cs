using Backend.Application.Common.Interfaces;
using Backend.Application.Common.Security;
using Backend.Domain.Entities;

namespace Backend.Application.DeviceSpecificationTemplates.Commands;

[Authorize(Roles = "Administrator")]
[RequirePermission(Backend.Domain.Constants.Permissions.DeviceCreate)]
public record CreateDeviceSpecificationTemplateCommand : IRequest<Guid>
{
    public Guid DeviceTypeId { get; init; }
    public string Model { get; init; } = string.Empty;
    public string? Manufacturer { get; init; }
    public string SpecificationsTemplate { get; init; } = "{}";
    public string MetadataTemplate { get; init; } = "{}";
    public string? HierarchyTemplate { get; init; }
    public bool IsDefault { get; init; }
}

public class CreateDeviceSpecificationTemplateCommandHandler : IRequestHandler<CreateDeviceSpecificationTemplateCommand, Guid>
{
    private readonly IApplicationDbContext _context;

    public CreateDeviceSpecificationTemplateCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Guid> Handle(CreateDeviceSpecificationTemplateCommand request, CancellationToken cancellationToken)
    {
        // If marking as default, unset any existing defaults for the same DeviceType
        if (request.IsDefault)
        {
            var existingDefaults = await _context.DeviceSpecificationTemplates
                .Where(t => t.DeviceTypeId == request.DeviceTypeId && t.IsDefault)
                .ToListAsync(cancellationToken);

            foreach (var existing in existingDefaults)
                existing.IsDefault = false;
        }

        var entity = new DeviceSpecificationTemplate
        {
            DeviceTypeId = request.DeviceTypeId,
            Model = request.Model,
            Manufacturer = request.Manufacturer,
            SpecificationsTemplate = request.SpecificationsTemplate,
            MetadataTemplate = request.MetadataTemplate,
            HierarchyTemplate = request.HierarchyTemplate,
            IsDefault = request.IsDefault
        };

        _context.DeviceSpecificationTemplates.Add(entity);
        await _context.SaveChangesAsync(cancellationToken);

        return entity.Id;
    }
}
