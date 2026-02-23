using Backend.Application.Common.Interfaces;
using Backend.Application.Common.Security;

namespace Backend.Application.DeviceSpecificationTemplates.Commands;

[Authorize(Roles = "Administrator")]
[RequirePermission(Backend.Domain.Constants.Permissions.DeviceCreate)]
public record UpdateDeviceSpecificationTemplateCommand : IRequest
{
    public Guid Id { get; init; }
    public string Model { get; init; } = string.Empty;
    public string? Manufacturer { get; init; }
    public string SpecificationsTemplate { get; init; } = "{}";
    public string MetadataTemplate { get; init; } = "{}";
    public string? HierarchyTemplate { get; init; }
    public bool IsDefault { get; init; }
}

public class UpdateDeviceSpecificationTemplateCommandHandler : IRequestHandler<UpdateDeviceSpecificationTemplateCommand>
{
    private readonly IApplicationDbContext _context;

    public UpdateDeviceSpecificationTemplateCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task Handle(UpdateDeviceSpecificationTemplateCommand request, CancellationToken cancellationToken)
    {
        var entity = await _context.DeviceSpecificationTemplates
            .FindAsync(new object[] { request.Id }, cancellationToken);

        Guard.Against.NotFound(request.Id, entity);

        // If marking as default, unset any existing defaults for the same DeviceType
        if (request.IsDefault && !entity.IsDefault)
        {
            var existingDefaults = await _context.DeviceSpecificationTemplates
                .Where(t => t.DeviceTypeId == entity.DeviceTypeId && t.IsDefault && t.Id != entity.Id)
                .ToListAsync(cancellationToken);

            foreach (var existing in existingDefaults)
                existing.IsDefault = false;
        }

        entity.Model = request.Model;
        entity.Manufacturer = request.Manufacturer;
        entity.SpecificationsTemplate = request.SpecificationsTemplate;
        entity.MetadataTemplate = request.MetadataTemplate;
        entity.HierarchyTemplate = request.HierarchyTemplate;
        entity.IsDefault = request.IsDefault;

        await _context.SaveChangesAsync(cancellationToken);
    }
}
