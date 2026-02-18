using System.Text.Json;

namespace Backend.Application.Common.Services;

public static class PayloadDeserializer
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public static T? Deserialize<T>(string? payload) where T : class
    {
        if (string.IsNullOrWhiteSpace(payload) || payload == "{}")
            return null;

        try
        {
            return JsonSerializer.Deserialize<T>(payload, JsonOptions);
        }
        catch
        {
            return null;
        }
    }

    public static JsonElement? GetElement(string? payload)
    {
        if (string.IsNullOrWhiteSpace(payload) || payload == "{}")
            return null;

        try
        {
            using var doc = JsonDocument.Parse(payload);
            return doc.RootElement.Clone();
        }
        catch
        {
            return null;
        }
    }

    public static object? ExtractValue(JsonElement root, string dotPath)
    {
        if (string.IsNullOrWhiteSpace(dotPath))
            return null;

        var segments = dotPath.Split('.');
        var current = root;

        foreach (var segment in segments)
        {
            if (current.ValueKind != JsonValueKind.Object)
                return null;

            if (!current.TryGetProperty(segment, out var next))
            {
                // Try case-insensitive match
                var found = false;
                foreach (var prop in current.EnumerateObject())
                {
                    if (string.Equals(prop.Name, segment, StringComparison.OrdinalIgnoreCase))
                    {
                        next = prop.Value;
                        found = true;
                        break;
                    }
                }
                if (!found) return null;
            }

            current = next;
        }

        return current.ValueKind switch
        {
            JsonValueKind.Number => current.TryGetDouble(out var d) ? d : current.GetRawText(),
            JsonValueKind.String => current.GetString(),
            JsonValueKind.True => true,
            JsonValueKind.False => false,
            JsonValueKind.Null => null,
            JsonValueKind.Undefined => null,
            _ => current.GetRawText()
        };
    }

    public static JsonElement? GetNestedElement(JsonElement root, string dotPath)
    {
        if (string.IsNullOrWhiteSpace(dotPath))
            return root;

        var segments = dotPath.Split('.');
        var current = root;

        foreach (var segment in segments)
        {
            if (current.ValueKind != JsonValueKind.Object)
                return null;

            if (!current.TryGetProperty(segment, out var next))
            {
                var found = false;
                foreach (var prop in current.EnumerateObject())
                {
                    if (string.Equals(prop.Name, segment, StringComparison.OrdinalIgnoreCase))
                    {
                        next = prop.Value;
                        found = true;
                        break;
                    }
                }
                if (!found) return null;
            }

            current = next;
        }

        return current;
    }
}
