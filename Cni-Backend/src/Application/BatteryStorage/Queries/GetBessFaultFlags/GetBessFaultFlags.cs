using System.Text.Json;
using Backend.Application.Common.Interfaces;
using Backend.Application.Common.Models.Monitoring;
using Backend.Application.Common.Security;
using Backend.Application.Common.Services;
using Backend.Domain.Constants;

namespace Backend.Application.BatteryStorage.Queries.GetBessFaultFlags;

[Authorize(Roles = "Administrator,Tenant.Admin")]
[RequirePermission(Backend.Domain.Constants.Permissions.BessView)]
public record GetBessFaultFlagsQuery : IRequest<FaultFlagsResponseDto>
{
    public Guid UnitId { get; init; }
}

public class GetBessFaultFlagsQueryHandler : IRequestHandler<GetBessFaultFlagsQuery, FaultFlagsResponseDto>
{
    private readonly IApplicationDbContext _context;

    public GetBessFaultFlagsQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<FaultFlagsResponseDto> Handle(GetBessFaultFlagsQuery request, CancellationToken cancellationToken)
    {
        var device = await _context.Devices
            .Include(d => d.DeviceType)
            .Where(d => d.Id == request.UnitId && d.DeviceType != null && d.DeviceType.Name == DeviceTypeNames.EnergyStorage)
            .FirstOrDefaultAsync(cancellationToken);

        Guard.Against.NotFound(request.UnitId, device);

        var latestData = await _context.DeviceBatteryData
            .Where(dd => dd.DeviceId == request.UnitId)
            .OrderByDescending(dd => dd.DeviceLocalTime)
            .FirstOrDefaultAsync(cancellationToken);

        var payload = PayloadDeserializer.GetElement(latestData?.Payload);
        var specs = PayloadDeserializer.GetElement(device.Specifications);

        return BuildFaultFlags(specs, payload);
    }

    private static FaultFlagsResponseDto BuildFaultFlags(JsonElement? specs, JsonElement? payload)
    {
        var flags = new List<FaultFlagDto>();
        var sources = new List<FaultSourceDto>();
        var categories = new List<FaultCategoryDto>();

        if (specs.HasValue && specs.Value.TryGetProperty("faultFlags", out var faultFlagsSpec) &&
            faultFlagsSpec.ValueKind == JsonValueKind.Object)
        {
            // Parse flag definitions from schema
            if (faultFlagsSpec.TryGetProperty("flags", out var flagsElement) &&
                flagsElement.ValueKind == JsonValueKind.Array)
            {
                foreach (var flag in flagsElement.EnumerateArray())
                {
                    var key = flag.TryGetProperty("key", out var k) ? k.GetString() ?? "" : "";
                    var label = flag.TryGetProperty("label", out var l) ? l.GetString() ?? key : key;
                    var source = flag.TryGetProperty("source", out var s) ? s.GetString() : null;
                    var category = flag.TryGetProperty("category", out var c) ? c.GetString() : null;
                    var severity = flag.TryGetProperty("severity", out var sev) ? sev.GetString() ?? "Warning" : "Warning";
                    var sourcePath = flag.TryGetProperty("sourcePath", out var sp) ? sp.GetString() : null;

                    var isActive = false;
                    if (payload.HasValue && !string.IsNullOrEmpty(sourcePath))
                    {
                        var value = PayloadDeserializer.ExtractValue(payload.Value, sourcePath);
                        isActive = value is true || value is "true";
                    }

                    flags.Add(new FaultFlagDto
                    {
                        Key = key, Label = label, IsActive = isActive,
                        Source = source, Category = category, Severity = severity
                    });
                }
            }

            // Parse sources
            if (faultFlagsSpec.TryGetProperty("sources", out var sourcesElement) &&
                sourcesElement.ValueKind == JsonValueKind.Array)
            {
                foreach (var src in sourcesElement.EnumerateArray())
                {
                    var key = src.TryGetProperty("key", out var k) ? k.GetString() ?? "" : "";
                    var label = src.TryGetProperty("label", out var l) ? l.GetString() ?? key : key;
                    var sourceFlags = flags.Where(f => f.Source == key).ToList();

                    sources.Add(new FaultSourceDto
                    {
                        Key = key, Label = label,
                        ActiveCount = sourceFlags.Count(f => f.IsActive),
                        TotalCount = sourceFlags.Count
                    });
                }
            }
        }
        else if (payload.HasValue)
        {
            // Try to extract fault flags from status in payload
            flags = ExtractDefaultFaultFlags(payload.Value);
        }

        // Build categories
        var categoryGroups = flags.GroupBy(f => f.Category ?? "general");
        foreach (var group in categoryGroups)
        {
            categories.Add(new FaultCategoryDto
            {
                Key = group.Key,
                Label = FormatLabel(group.Key),
                ActiveCount = group.Count(f => f.IsActive),
                TotalCount = group.Count()
            });
        }

        return new FaultFlagsResponseDto
        {
            Flags = flags,
            Sources = sources.Count > 0 ? sources : null,
            Categories = categories,
            Summary = new FaultFlagSummaryDto
            {
                TotalFlags = flags.Count,
                ActiveFlags = flags.Count(f => f.IsActive),
                ActiveBySource = sources.Count > 0
                    ? sources.Where(s => s.ActiveCount > 0).ToDictionary(s => s.Key, s => s.ActiveCount)
                    : null,
                ActiveByCategory = categories.Where(c => c.ActiveCount > 0)
                    .ToDictionary(c => c.Key, c => c.ActiveCount)
            }
        };
    }

    private static List<FaultFlagDto> ExtractDefaultFaultFlags(JsonElement payload)
    {
        var flags = new List<FaultFlagDto>();

        if (payload.TryGetProperty("status", out var status) && status.ValueKind == JsonValueKind.Object)
        {
            if (status.TryGetProperty("faultStatus", out var faultStatus))
            {
                flags.Add(new FaultFlagDto
                {
                    Key = "faultStatus", Label = "Fault Status",
                    IsActive = faultStatus.ValueKind == JsonValueKind.True,
                    Category = "general", Severity = "Critical"
                });
            }

            if (status.TryGetProperty("warningStatus", out var warningStatus))
            {
                flags.Add(new FaultFlagDto
                {
                    Key = "warningStatus", Label = "Warning Status",
                    IsActive = warningStatus.ValueKind == JsonValueKind.True,
                    Category = "general", Severity = "Warning"
                });
            }

            if (status.TryGetProperty("faultMessages", out var faultMessages) &&
                faultMessages.ValueKind == JsonValueKind.Array)
            {
                var index = 0;
                foreach (var msg in faultMessages.EnumerateArray())
                {
                    flags.Add(new FaultFlagDto
                    {
                        Key = $"fault_{index}", Label = msg.GetString() ?? $"Fault {index}",
                        IsActive = true, Category = "general", Severity = "Critical"
                    });
                    index++;
                }
            }
        }

        return flags;
    }

    private static string FormatLabel(string key)
    {
        if (string.IsNullOrEmpty(key)) return "General";
        return string.Concat(
            key[0].ToString().ToUpperInvariant(),
            key.AsSpan(1)).Replace("_", " ");
    }
}
