namespace Backend.Application.Common.Services;

/// <summary>
/// Parses structured MQTT topics in the format: {prefix}/{facilityId}/{rtuId}/{channel}
/// Example: cni/3fa85f64-5717-4562-b3fc-2c963f66afa6/7c9e6679-7425-40de-944b-e07fc1f90ae7/telemetry
/// </summary>
public static class MqttTopicParser
{
    public static ParsedMqttTopic? Parse(string topic)
    {
        if (string.IsNullOrEmpty(topic))
            return null;

        var segments = topic.Split('/');

        if (segments.Length < 4)
            return null;

        if (!Guid.TryParse(segments[1], out var facilityId))
            return null;

        if (!Guid.TryParse(segments[2], out var rtuId))
            return null;

        return new ParsedMqttTopic(segments[0], facilityId, rtuId, segments[3]);
    }
}

public readonly record struct ParsedMqttTopic(string Prefix, Guid FacilityId, Guid RtuId, string Channel);
