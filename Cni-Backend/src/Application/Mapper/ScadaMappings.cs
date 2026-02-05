using System.Text.Json;
using Backend.Domain.Entities.DeviceData;

namespace Backend.Application.Mapper;

/// <summary>
/// Mappings for SCADA device data.
/// Device telemetry is now stored as JSON payloads.
/// </summary>
public class ScadaMappings : Profile
{
    public ScadaMappings()
    {
        // Map device data entities to DTOs by deserializing JSON payload
        // The actual payload structure depends on device type and configuration
    }
}

/// <summary>
/// Helper class for working with device data JSON payloads
/// </summary>
public static class DeviceDataPayloadHelper
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    /// <summary>
    /// Deserialize payload to a specific type
    /// </summary>
    public static T? DeserializePayload<T>(string payload) where T : class
    {
        if (string.IsNullOrWhiteSpace(payload) || payload == "{}")
            return null;

        return JsonSerializer.Deserialize<T>(payload, JsonOptions);
    }

    /// <summary>
    /// Serialize data to JSON payload
    /// </summary>
    public static string SerializePayload<T>(T data)
    {
        return JsonSerializer.Serialize(data, JsonOptions);
    }

    /// <summary>
    /// Get a specific value from payload using JsonDocument
    /// </summary>
    public static T? GetPayloadValue<T>(string payload, string propertyName)
    {
        if (string.IsNullOrWhiteSpace(payload) || payload == "{}")
            return default;

        using var doc = JsonDocument.Parse(payload);
        if (doc.RootElement.TryGetProperty(propertyName, out var element))
        {
            return JsonSerializer.Deserialize<T>(element.GetRawText(), JsonOptions);
        }
        return default;
    }
}
