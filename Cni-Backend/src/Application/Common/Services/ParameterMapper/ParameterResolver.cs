using System.Text.Json;

namespace Backend.Application.Common.Services.ParameterMapper;

public sealed class ParameterResolver
{
    private readonly Dictionary<string, ParameterDefinition> _parameterMap;
    private readonly Dictionary<string, JsonElement?> _payloadsByKind;
    private readonly JsonElement? _metadata;
    private readonly Dictionary<string, object?> _resolved = new();
    private readonly HashSet<string> _resolving = new();

    public ParameterResolver(
        Dictionary<string, ParameterDefinition> parameterMap,
        Dictionary<string, JsonElement?> payloadsByKind,
        JsonElement? metadata = null)
    {
        _parameterMap = parameterMap;
        _payloadsByKind = payloadsByKind;
        _metadata = metadata;
    }

    public object? Resolve(string parameterKey)
    {
        if (_resolved.TryGetValue(parameterKey, out var cached))
            return cached;

        // Cycle detection
        if (!_resolving.Add(parameterKey))
            return null;

        try
        {
            var result = ResolveInternal(parameterKey);
            _resolved[parameterKey] = result;
            return result;
        }
        finally
        {
            _resolving.Remove(parameterKey);
        }
    }

    public double? ResolveNumber(string parameterKey)
    {
        return FormulaEvaluator.ToDouble(Resolve(parameterKey));
    }

    public string? ResolveString(string parameterKey)
    {
        var value = Resolve(parameterKey);
        return value?.ToString();
    }

    public bool? ResolveBool(string parameterKey)
    {
        var value = Resolve(parameterKey);
        return value switch
        {
            bool b => b,
            double d => d != 0,
            int i => i != 0,
            long l => l != 0,
            _ => null
        };
    }

    public Dictionary<string, object?> ResolveAll()
    {
        var result = new Dictionary<string, object?>();
        foreach (var key in _parameterMap.Keys)
        {
            result[key] = Resolve(key);
        }
        return result;
    }

    /// <summary>
    /// Legacy compat: resolve a bare sourcePath across all payloads.
    /// </summary>
    public object? ResolveSourcePath(string sourcePath, string? kind = null)
    {
        if (kind != null)
        {
            if (_payloadsByKind.TryGetValue(kind, out var payload) && payload.HasValue)
                return PayloadDeserializer.ExtractValue(payload.Value, sourcePath);
            return null;
        }

        foreach (var (_, payload) in _payloadsByKind)
        {
            if (!payload.HasValue) continue;
            var value = PayloadDeserializer.ExtractValue(payload.Value, sourcePath);
            if (value != null) return value;
        }

        return null;
    }

    private object? ResolveInternal(string parameterKey)
    {
        if (!_parameterMap.TryGetValue(parameterKey, out var definition))
            return null;

        // 1. Try formula
        if (definition.Formula != null)
        {
            var result = EvaluateFormula(definition.Formula);
            if (result != null)
                return result;
        }

        // 2. Try source
        if (definition.Source != null)
        {
            var result = ExtractFromSource(definition.Source);
            if (result != null)
                return result;
        }

        // 3. Try fallback
        if (definition.Fallback != null)
        {
            var result = ExtractFromSource(definition.Fallback);
            if (result != null)
                return result;
        }

        return null;
    }

    private object? EvaluateFormula(ParameterFormula formula)
    {
        return FormulaEvaluator.Evaluate(formula.Expression, inputName =>
        {
            if (!formula.Inputs.TryGetValue(inputName, out var input))
                return null;

            // $ref → resolve another parameter
            if (input.Ref != null)
                return Resolve(input.Ref);

            // Inline source
            if (input.Source != null)
                return ExtractFromSource(input.Source);

            return null;
        });
    }

    private object? ExtractFromSource(ParameterSource source)
    {
        JsonElement? targetPayload = null;

        if (source.Origin == "metadata")
        {
            targetPayload = _metadata;
        }
        else if (source.Kind != null)
        {
            _payloadsByKind.TryGetValue(source.Kind, out targetPayload);
        }
        else
        {
            // No Kind specified — search all payloads
            foreach (var (_, payload) in _payloadsByKind)
            {
                if (!payload.HasValue) continue;
                foreach (var path in source.Paths)
                {
                    var value = PayloadDeserializer.ExtractValue(payload.Value, path);
                    if (value != null) return value;
                }
            }
            return source.Default;
        }

        if (!targetPayload.HasValue)
            return source.Default;

        foreach (var path in source.Paths)
        {
            var value = PayloadDeserializer.ExtractValue(targetPayload.Value, path);
            if (value != null) return value;
        }

        return source.Default;
    }
}
