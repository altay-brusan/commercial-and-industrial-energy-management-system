namespace Backend.Application.Common.Services.ParameterMapper;

public sealed class ParameterDefinition
{
    public string Key { get; init; } = string.Empty;
    public string Type { get; init; } = "number";
    public string? Label { get; init; }
    public string? Unit { get; init; }
    public ParameterSource? Source { get; init; }
    public ParameterFormula? Formula { get; init; }
    public ParameterSource? Fallback { get; init; }
}

public sealed class ParameterSource
{
    public string? Kind { get; init; }
    public string? Origin { get; init; }
    public string[] Paths { get; init; } = Array.Empty<string>();
    public object? Default { get; init; }
}

public sealed class ParameterFormula
{
    public string Expression { get; init; } = string.Empty;
    public Dictionary<string, FormulaInput> Inputs { get; init; } = new();
}

public sealed class FormulaInput
{
    public string? Ref { get; init; }
    public ParameterSource? Source { get; init; }
}
