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
        return new AlarmDto
        {
            Id = alarm.Id,
            Code = alarm.AlarmType?.Code ?? string.Empty,
            Message = alarm.AlarmType?.Description,
            Severity = alarm.AlarmType?.Severity.ToLower() ?? string.Empty,
            Status = !alarm.IsActive ? "resolved" : (alarm.AcknowledgedAt != null ? "acknowledged" : "active"),
            Source = alarm.Device?.Name ?? alarm.AlarmType?.Source,
            SourceType = alarm.AlarmType?.Category?.ToLower(),
            TriggeredAt = alarm.OccurredAt,
            Value = null,
            Threshold = null,
            AcknowledgedAt = alarm.AcknowledgedAt,
            AcknowledgedBy = MapUser(alarm.AcknowledgedBy),
            ResolvedAt = alarm.ResolvedAt,
            ResolvedBy = MapUser(alarm.ResolvedBy),
            RelatedAsset = alarm.Device != null ? new AlarmRelatedAssetDto
            {
                Id = alarm.DeviceId,
                Name = alarm.Device.Name,
                Type = alarm.AlarmType?.Category?.ToLower(),
                Location = alarm.Device.Rtu?.Facility?.Name,
                Status = alarm.Device.Status,
                SerialNumber = alarm.Device.SerialNumber,
                Model = alarm.Device.Model
            } : null,
            RelatedMaintenanceTaskId = alarm.MaintenanceTaskId,
            Location = alarm.Device?.Rtu?.Facility?.Name
        };
    }

    private static AlarmUserDto? MapUser(ApplicationUser? user)
    {
        if (user == null) return null;

        string? initials = null;
        if (!string.IsNullOrEmpty(user.Name) && !string.IsNullOrEmpty(user.Surname))
            initials = $"{user.Name[0]}{user.Surname[0]}".ToUpper();

        return new AlarmUserDto
        {
            Id = user.Id,
            Name = user.Name,
            Email = user.Email,
            Initials = initials,
            Role = user.Title,
            Department = user.Department,
            Phone = user.PhoneNumber
        };
    }
}
