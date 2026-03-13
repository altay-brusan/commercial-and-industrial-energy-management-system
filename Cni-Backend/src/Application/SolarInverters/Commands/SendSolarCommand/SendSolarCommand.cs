using Backend.Application.Common.Interfaces;
using Backend.Application.Common.Security;
using Backend.Domain.Constants;

namespace Backend.Application.SolarInverters.Commands.SendSolarCommand;

[Authorize(Roles = "Administrator,Tenant.Admin")]
[RequirePermission(Backend.Domain.Constants.Permissions.SolarControl)]
public record SendSolarCommandCommand : IRequest<SolarCommandResultDto>
{
    public Guid UnitId { get; init; }
    public string Action { get; init; } = string.Empty;
}

public class SendSolarCommandCommandValidator : AbstractValidator<SendSolarCommandCommand>
{
    private static readonly string[] ValidActions =
        { "TurnOn", "TurnOff", "Reset", "Shutdown", "Startup", "ClearFaults" };

    public SendSolarCommandCommandValidator()
    {
        RuleFor(x => x.UnitId).NotEmpty();
        RuleFor(x => x.Action).NotEmpty()
            .Must(a => ValidActions.Contains(a, StringComparer.OrdinalIgnoreCase))
            .WithMessage($"Action must be one of: {string.Join(", ", ValidActions)}");
    }
}

public class SendSolarCommandCommandHandler : IRequestHandler<SendSolarCommandCommand, SolarCommandResultDto>
{
    private readonly IApplicationDbContext _context;

    public SendSolarCommandCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<SolarCommandResultDto> Handle(SendSolarCommandCommand request, CancellationToken cancellationToken)
    {
        var device = await _context.Devices
            .Include(d => d.DeviceType)
            .Include(d => d.LogicalDevice)
            .Where(d => d.Id == request.UnitId &&
                ((d.DeviceType != null && (d.DeviceType.Name == DeviceTypeNames.Solar || d.DeviceType.Name == DeviceTypeNames.Inverter)) ||
                 (d.LogicalDevice != null && d.LogicalDevice.Category == "Solar")))
            .FirstOrDefaultAsync(cancellationToken);

        Guard.Against.NotFound(request.UnitId, device);

        // Command is accepted and queued. Actual device control depends on
        // SCADA/MQTT infrastructure which will be implemented separately.
        return new SolarCommandResultDto
        {
            DeviceId = device.Id,
            DeviceName = device.Name,
            Action = request.Action,
            Status = "accepted",
            Message = $"Command '{request.Action}' accepted for device '{device.Name}'"
        };
    }
}

public record SolarCommandResultDto
{
    public Guid DeviceId { get; init; }
    public string DeviceName { get; init; } = string.Empty;
    public string Action { get; init; } = string.Empty;
    public string Status { get; init; } = string.Empty;
    public string? Message { get; init; }
}
