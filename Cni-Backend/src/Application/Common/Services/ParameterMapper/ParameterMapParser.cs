using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Caching.Memory;

namespace Backend.Application.Common.Services.ParameterMapper;

public static class ParameterMapParser
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    private static readonly TimeSpan CacheDuration = TimeSpan.FromMinutes(30);

    /// <summary>
    /// Parse without cache (for unit tests or one-off usage).
    /// </summary>
    public static Dictionary<string, ParameterDefinition> Parse(string? specifications)
    {
        if (string.IsNullOrWhiteSpace(specifications))
            return new Dictionary<string, ParameterDefinition>();

        try
        {
            using var doc = JsonDocument.Parse(specifications);
            var root = doc.RootElement;

            if (!root.TryGetProperty("parameterMap", out var parameterMap))
                return new Dictionary<string, ParameterDefinition>();

            if (!parameterMap.TryGetProperty("parameters", out var parameters))
                return new Dictionary<string, ParameterDefinition>();

            if (parameters.ValueKind != JsonValueKind.Object)
                return new Dictionary<string, ParameterDefinition>();

            var result = new Dictionary<string, ParameterDefinition>();

            foreach (var prop in parameters.EnumerateObject())
            {
                var definition = ParseDefinition(prop.Name, prop.Value);
                if (definition != null)
                    result[prop.Name] = definition;
            }

            return result;
        }
        catch (JsonException)
        {
            return new Dictionary<string, ParameterDefinition>();
        }
    }

    /// <summary>
    /// Parse with IMemoryCache. Uses hash of the specifications string as cache key.
    /// Cached for 30 minutes (sliding). Since devices of the same model share identical
    /// Specifications JSON, the map is parsed once per template.
    /// </summary>
    public static Dictionary<string, ParameterDefinition> ParseCached(
        string? specifications, IMemoryCache cache)
    {
        if (string.IsNullOrWhiteSpace(specifications))
            return new Dictionary<string, ParameterDefinition>();

        var cacheKey = $"paramMap:{ComputeHash(specifications)}";

        return cache.GetOrCreate(cacheKey, entry =>
        {
            entry.SlidingExpiration = CacheDuration;
            return Parse(specifications);
        })!;
    }

    private static ParameterDefinition? ParseDefinition(string key, JsonElement element)
    {
        if (element.ValueKind != JsonValueKind.Object)
            return null;

        var definition = new ParameterDefinition
        {
            Key = key,
            Type = GetString(element, "type") ?? "number",
            Label = GetString(element, "label"),
            Unit = GetString(element, "unit"),
            Source = ParseSource(element, "source"),
            Formula = ParseFormula(element),
            Fallback = ParseSource(element, "fallback")
        };

        return definition;
    }

    private static ParameterSource? ParseSource(JsonElement parent, string propertyName)
    {
        if (!parent.TryGetProperty(propertyName, out var sourceElement) ||
            sourceElement.ValueKind != JsonValueKind.Object)
            return null;

        return ParseSourceElement(sourceElement);
    }

    private static ParameterSource ParseSourceElement(JsonElement sourceElement)
    {
        var paths = Array.Empty<string>();
        if (sourceElement.TryGetProperty("paths", out var pathsElement) &&
            pathsElement.ValueKind == JsonValueKind.Array)
        {
            paths = pathsElement.EnumerateArray()
                .Where(p => p.ValueKind == JsonValueKind.String)
                .Select(p => p.GetString()!)
                .ToArray();
        }

        object? defaultValue = null;
        if (sourceElement.TryGetProperty("default", out var defaultElement))
        {
            defaultValue = defaultElement.ValueKind switch
            {
                JsonValueKind.Number => defaultElement.TryGetDouble(out var d) ? d : null,
                JsonValueKind.String => defaultElement.GetString(),
                JsonValueKind.True => true,
                JsonValueKind.False => false,
                _ => null
            };
        }

        return new ParameterSource
        {
            Kind = GetString(sourceElement, "kind"),
            Origin = GetString(sourceElement, "origin"),
            Paths = paths,
            Default = defaultValue
        };
    }

    private static ParameterFormula? ParseFormula(JsonElement parent)
    {
        if (!parent.TryGetProperty("formula", out var formulaElement) ||
            formulaElement.ValueKind != JsonValueKind.Object)
            return null;

        var expression = GetString(formulaElement, "expression");
        if (string.IsNullOrEmpty(expression))
            return null;

        var inputs = new Dictionary<string, FormulaInput>();
        if (formulaElement.TryGetProperty("inputs", out var inputsElement) &&
            inputsElement.ValueKind == JsonValueKind.Object)
        {
            foreach (var prop in inputsElement.EnumerateObject())
            {
                inputs[prop.Name] = ParseFormulaInput(prop.Value);
            }
        }

        return new ParameterFormula
        {
            Expression = expression,
            Inputs = inputs
        };
    }

    private static FormulaInput ParseFormulaInput(JsonElement element)
    {
        if (element.ValueKind != JsonValueKind.Object)
            return new FormulaInput();

        // Check for $ref shorthand
        if (element.TryGetProperty("$ref", out var refElement) &&
            refElement.ValueKind == JsonValueKind.String)
        {
            return new FormulaInput { Ref = refElement.GetString() };
        }

        // Check for inline source
        ParameterSource? source = null;
        if (element.TryGetProperty("source", out var sourceElement) &&
            sourceElement.ValueKind == JsonValueKind.Object)
        {
            source = ParseSourceElement(sourceElement);
        }

        return new FormulaInput { Source = source };
    }

    private static string? GetString(JsonElement element, string propertyName)
    {
        return element.TryGetProperty(propertyName, out var value) && value.ValueKind == JsonValueKind.String
            ? value.GetString()
            : null;
    }

    private static string ComputeHash(string input)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(input));
        return Convert.ToHexString(bytes, 0, 8);
    }
}
