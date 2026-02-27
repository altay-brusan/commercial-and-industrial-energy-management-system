using System.Text.Json;
using Backend.Application.Common.Interfaces;
using Backend.Application.Common.Security;
using Backend.Application.Common.Services;

namespace Backend.Application.BatteryStorage.Queries.GetBessCells;

[Authorize(Roles = "Administrator,Tenant.Admin")]
[RequirePermission(Backend.Domain.Constants.Permissions.BessView)]
public record GetBessCellsQuery : IRequest<BessCellsResponseDto>
{
    public Guid UnitId { get; init; }
}

public class GetBessCellsQueryHandler : IRequestHandler<GetBessCellsQuery, BessCellsResponseDto>
{
    private readonly IApplicationDbContext _context;
    private readonly ContainerTelemetryLoader _telemetryLoader;

    public GetBessCellsQueryHandler(IApplicationDbContext context)
    {
        _context = context;
        _telemetryLoader = new ContainerTelemetryLoader(context);
    }

    public async Task<BessCellsResponseDto> Handle(GetBessCellsQuery request, CancellationToken cancellationToken)
    {
        var device = await _context.Devices
            .Include(d => d.DeviceType)
            .Include(d => d.SpecificationTemplate)
            .FirstOrDefaultAsync(d => d.Id == request.UnitId, cancellationToken);

        Guard.Against.NotFound(request.UnitId, device);

        // Walk up parent chain to find root container and inherit template
        var rootId = device.Id;
        var currentId = device.ParentDeviceId;
        var inheritedTemplate = device.SpecificationTemplate;

        while (currentId.HasValue)
        {
            var parent = await _context.Devices
                .Include(d => d.DeviceType)
                .Include(d => d.SpecificationTemplate)
                .FirstOrDefaultAsync(d => d.Id == currentId.Value, cancellationToken);

            if (parent == null) break;

            inheritedTemplate ??= parent.SpecificationTemplate;
            rootId = parent.Id;
            currentId = parent.ParentDeviceId;
        }

        // Load telemetry from root container
        var telemetry = await _telemetryLoader.LoadLatest(rootId, cancellationToken);
        var payloads = telemetry.GetPayloadsByKind();
        var batt = payloads.GetValueOrDefault("ContainerBatteryState");

        var cells = new List<CellDto>();

        if (batt is not { ValueKind: JsonValueKind.Object } battPayload)
            return new BessCellsResponseDto { Cells = cells };

        // Extract cell arrays from payload
        var cellVoltages = GetDoubleArray(battPayload, "cellVoltages");
        var cellTemperatures = GetDoubleArray(battPayload, "cellTemperatures");
        var cellCurrents = GetDoubleArray(battPayload, "cellCurrents");
        var cellSocs = GetDoubleArray(battPayload, "cellSocs");
        var cellSohs = GetDoubleArray(battPayload, "cellSohs");
        var cellResistances = GetDoubleArray(battPayload, "cellResistances");
        var cellBalancing = GetBoolArray(battPayload, "cellBalancing");
        var cellCycleCounts = GetIntArray(battPayload, "cellCycleCounts");

        // Cell count = length of the longest array found
        var cellCount = new[]
        {
            cellVoltages?.Length ?? 0,
            cellTemperatures?.Length ?? 0,
            cellCurrents?.Length ?? 0,
            cellSocs?.Length ?? 0,
            cellSohs?.Length ?? 0,
            cellResistances?.Length ?? 0,
            cellBalancing?.Length ?? 0,
            cellCycleCounts?.Length ?? 0
        }.Max();

        var deviceName = device.Name ?? device.SerialNumber ?? "cell";

        for (var i = 0; i < cellCount; i++)
        {
            var cellNumber = i + 1;
            cells.Add(new CellDto
            {
                Id = $"{deviceName}-C{cellNumber}",
                CellNumber = cellNumber,
                Status = "normal",
                Voltage = GetAt(cellVoltages, i),
                Current = GetAt(cellCurrents, i),
                Temperature = GetAt(cellTemperatures, i),
                Soc = GetAt(cellSocs, i),
                Soh = GetAt(cellSohs, i),
                InternalResistance = GetAt(cellResistances, i),
                Balancing = cellBalancing != null && i < cellBalancing.Length ? cellBalancing[i] : null,
                CycleCount = cellCycleCounts != null && i < cellCycleCounts.Length ? cellCycleCounts[i] : null
            });
        }

        return new BessCellsResponseDto { Cells = cells };
    }

    private static double? GetAt(double[]? array, int index)
        => array != null && index < array.Length ? array[index] : null;

    private static double[]? GetDoubleArray(JsonElement payload, string key)
    {
        if (!payload.TryGetProperty(key, out var arr) || arr.ValueKind != JsonValueKind.Array)
            return null;

        var result = new double[arr.GetArrayLength()];
        for (var i = 0; i < result.Length; i++)
        {
            result[i] = arr[i].TryGetDouble(out var v) ? v : 0;
        }
        return result;
    }

    private static bool[]? GetBoolArray(JsonElement payload, string key)
    {
        if (!payload.TryGetProperty(key, out var arr) || arr.ValueKind != JsonValueKind.Array)
            return null;

        var result = new bool[arr.GetArrayLength()];
        for (var i = 0; i < result.Length; i++)
        {
            result[i] = arr[i].ValueKind == JsonValueKind.True;
        }
        return result;
    }

    private static int[]? GetIntArray(JsonElement payload, string key)
    {
        if (!payload.TryGetProperty(key, out var arr) || arr.ValueKind != JsonValueKind.Array)
            return null;

        var result = new int[arr.GetArrayLength()];
        for (var i = 0; i < result.Length; i++)
        {
            result[i] = arr[i].TryGetInt32(out var v) ? v : 0;
        }
        return result;
    }
}
