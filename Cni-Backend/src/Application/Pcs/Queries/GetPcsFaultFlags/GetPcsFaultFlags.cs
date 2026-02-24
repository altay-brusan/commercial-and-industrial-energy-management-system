using System.Text.Json;
using Backend.Application.Common.Interfaces;
using Backend.Application.Common.Models.Monitoring;
using Backend.Application.Common.Security;
using Backend.Application.Common.Services;

namespace Backend.Application.Pcs.Queries.GetPcsFaultFlags;

[Authorize(Roles = "Administrator,Tenant.Admin")]
[RequirePermission(Backend.Domain.Constants.Permissions.PcsView)]
public record GetPcsFaultFlagsQuery : IRequest<FaultFlagsResponseDto>
{
    public Guid UnitId { get; init; }
}

public class GetPcsFaultFlagsQueryHandler : IRequestHandler<GetPcsFaultFlagsQuery, FaultFlagsResponseDto>
{
    private readonly IApplicationDbContext _context;

    public GetPcsFaultFlagsQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<FaultFlagsResponseDto> Handle(GetPcsFaultFlagsQuery request, CancellationToken cancellationToken)
    {
        var device = await _context.Devices
            .Include(d => d.DeviceType)
            .Include(d => d.SpecificationTemplate)
            .Where(d => d.Id == request.UnitId && d.DeviceType != null && d.DeviceType.Name == "PCS")
            .FirstOrDefaultAsync(cancellationToken);

        Guard.Against.NotFound(request.UnitId, device);

        var latestData = await _context.DevicePcsData
            .Where(dd => dd.DeviceId == request.UnitId)
            .OrderByDescending(dd => dd.DeviceLocalTime)
            .FirstOrDefaultAsync(cancellationToken);

        var payload = PayloadDeserializer.GetElement(latestData?.Payload);
        var specs = PayloadDeserializer.GetElement(device.SpecificationTemplate?.SpecificationsTemplate);

        return BuildFaultFlags(specs, payload);
    }

    private static FaultFlagsResponseDto BuildFaultFlags(JsonElement? specs, JsonElement? payload)
    {
        var flags = new List<FaultFlagDto>();
        var categories = new List<FaultCategoryDto>();

        if (specs.HasValue && specs.Value.TryGetProperty("faultFlags", out var faultFlagsSpec) &&
            faultFlagsSpec.ValueKind == JsonValueKind.Object)
        {
            if (faultFlagsSpec.TryGetProperty("flags", out var flagsElement) &&
                flagsElement.ValueKind == JsonValueKind.Array)
            {
                foreach (var flag in flagsElement.EnumerateArray())
                {
                    var key = flag.TryGetProperty("key", out var k) ? k.GetString() ?? "" : "";
                    var label = flag.TryGetProperty("label", out var l) ? l.GetString() ?? key : key;
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
                        Category = category, Severity = severity
                    });
                }
            }
        }
        else if (payload.HasValue)
        {
            flags = ExtractDefaultFaultFlags(payload.Value);
        }

        // Build categories (PCS: no source grouping, only categories)
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
            Sources = null, // PCS has no source grouping
            Categories = categories,
            Summary = new FaultFlagSummaryDto
            {
                TotalFlags = flags.Count,
                ActiveFlags = flags.Count(f => f.IsActive),
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
                    Category = "hardware", Severity = "Critical"
                });
            }

            if (status.TryGetProperty("warningStatus", out var warningStatus))
            {
                flags.Add(new FaultFlagDto
                {
                    Key = "warningStatus", Label = "Warning Status",
                    IsActive = warningStatus.ValueKind == JsonValueKind.True,
                    Category = "hardware", Severity = "Warning"
                });
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
