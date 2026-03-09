using System.Text.Json;
using Backend.Domain.Entities;

namespace Backend.Application.Alarms.Queries;

/// <summary>
/// Maps a loaded DeviceAlarm entity (with navigation props) to AlarmDto.
/// Used by commands that need to return the updated alarm after mutation.
/// </summary>
public static class AlarmDtoMapper
{
    public static AlarmDto ToDto(DeviceAlarm alarm)
    {
        var (value, threshold, message) = ExtractDetails(alarm.Details);

        return new AlarmDto
        {
            Id = alarm.Id,
            Code = alarm.AlarmType?.Code ?? string.Empty,
            Message = message ?? alarm.AlarmType?.Description,
            Severity = MapSeverity(alarm.AlarmType?.Severity),
            Status = !alarm.IsActive ? "resolved" : (alarm.AcknowledgedAt != null ? "acknowledged" : "active"),
            Source = alarm.Device?.Name ?? alarm.DeviceName ?? alarm.AlarmType?.Source,
            SourceType = alarm.Device?.DeviceType?.Name?.ToLower() ?? alarm.AlarmType?.Category?.ToLower(),
            TriggeredAt = alarm.OccurredAt,
            Value = value,
            Threshold = threshold,
            AcknowledgedAt = alarm.AcknowledgedAt,
            AcknowledgedBy = MapUser(alarm.AcknowledgedBy),
            ResolvedAt = alarm.ResolvedAt,
            ResolvedBy = MapUser(alarm.ResolvedBy),
            RelatedAsset = MapAsset(alarm),
            RelatedMaintenanceTaskId = alarm.MaintenanceTaskId,
            Location = alarm.Device?.Rtu?.Facility?.Name
        };
    }

    public static AlarmUserDto? MapUser(ApplicationUser? user)
    {
        if (user == null) return null;

        string? initials = null;
        if (!string.IsNullOrEmpty(user.Name) && !string.IsNullOrEmpty(user.Surname))
            initials = $"{user.Name[0]}{user.Surname[0]}".ToUpper();

        return new AlarmUserDto
        {
            Id = user.Id,
            Name = !string.IsNullOrEmpty(user.Surname) ? $"{user.Name} {user.Surname}" : user.Name,
            Email = user.Email,
            Initials = initials,
            Role = user.Title,
            Department = user.Department,
            Phone = user.PhoneNumber
        };
    }

    /// <summary>
    /// Maps internal severity names to the frontend contract.
    /// Internal: Critical, Major, Minor, Warning
    /// Frontend: critical, high, medium, low
    /// </summary>
    public static string MapSeverity(string? severity)
    {
        if (string.IsNullOrEmpty(severity))
            return "low";

        return severity.ToLower() switch
        {
            "critical" => "critical",
            "major" or "high" => "high",
            "minor" or "medium" => "medium",
            "warning" or "low" => "low",
            _ => severity.ToLower()
        };
    }

    public static AlarmRelatedAssetDto? MapAsset(DeviceAlarm alarm)
    {
        if (alarm.Device == null) return null;

        string? capacity = null;
        DateTimeOffset? lastMaintenance = null;
        if (!string.IsNullOrEmpty(alarm.Device.Metadata))
        {
            try
            {
                using var doc = JsonDocument.Parse(alarm.Device.Metadata);
                if (doc.RootElement.TryGetProperty("capacity", out var cap))
                    capacity = cap.GetString();
                if (doc.RootElement.TryGetProperty("lastMaintenance", out var maint)
                    && maint.ValueKind == JsonValueKind.String
                    && DateTimeOffset.TryParse(maint.GetString(), out var maintDate))
                    lastMaintenance = maintDate;
            }
            catch { /* ignore malformed metadata */ }
        }

        return new AlarmRelatedAssetDto
        {
            Id = alarm.DeviceId,
            Name = alarm.Device.Name,
            Type = alarm.Device.DeviceType?.Name?.ToLower() ?? alarm.AlarmType?.Category?.ToLower(),
            Location = alarm.Device.Rtu?.Facility?.Name,
            Capacity = capacity,
            Status = alarm.Device.Status,
            LastMaintenance = lastMaintenance,
            SerialNumber = alarm.Device.SerialNumber,
            Model = alarm.Device.Model
        };
    }

    /// <summary>
    /// Extracts "value", "threshold", and "message" from the alarm's Details JSON.
    /// Details is a JSON string like: {"value":"812V","threshold":"Normal range","message":"..."}
    /// </summary>
    private static (string? Value, string? Threshold, string? Message) ExtractDetails(string? details)
    {
        if (string.IsNullOrEmpty(details))
            return (null, null, null);

        try
        {
            using var doc = JsonDocument.Parse(details);
            string? value = null;
            string? threshold = null;
            string? message = null;

            if (doc.RootElement.TryGetProperty("value", out var v))
                value = v.ValueKind == JsonValueKind.String ? v.GetString() : v.ToString();

            if (doc.RootElement.TryGetProperty("threshold", out var t))
                threshold = t.ValueKind == JsonValueKind.String ? t.GetString() : t.ToString();

            if (doc.RootElement.TryGetProperty("message", out var m))
                message = m.GetString();

            return (value, threshold, message);
        }
        catch
        {
            return (null, null, null);
        }
    }
}
