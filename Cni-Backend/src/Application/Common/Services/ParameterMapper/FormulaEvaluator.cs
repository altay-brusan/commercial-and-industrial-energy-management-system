using System.Globalization;

namespace Backend.Application.Common.Services.ParameterMapper;

/// <summary>
/// Evaluates formula expressions using function-call notation.
/// Supported: add, subtract, multiply, divide, negate, abs, avg, min, max, round,
/// gt, lt, eq, cond, coalesce. Literals: numbers, 'strings', null.
/// </summary>
public static class FormulaEvaluator
{
    private const int MaxDepth = 10;

    public static object? Evaluate(string expression, Func<string, object?> resolveInput)
    {
        var pos = 0;
        return ParseExpression(expression.AsSpan(), ref pos, resolveInput, 0);
    }

    private static object? ParseExpression(ReadOnlySpan<char> expr, ref int pos,
        Func<string, object?> resolveInput, int depth)
    {
        if (depth > MaxDepth)
            return null;

        SkipWhitespace(expr, ref pos);

        if (pos >= expr.Length)
            return null;

        // String literal: 'text'
        if (expr[pos] == '\'')
            return ParseStringLiteral(expr, ref pos);

        // null literal
        if (TryConsume(expr, ref pos, "null"))
            return null;

        // Negative number: -123.45
        if (expr[pos] == '-' && pos + 1 < expr.Length && (char.IsDigit(expr[pos + 1]) || expr[pos + 1] == '.'))
        {
            // Check this isn't actually a function call (shouldn't be, but be safe)
            return ParseNumberLiteral(expr, ref pos);
        }

        // Number literal: 123.45
        if (char.IsDigit(expr[pos]) || expr[pos] == '.')
            return ParseNumberLiteral(expr, ref pos);

        // Function call or input reference: name(...)  or  name
        var name = ParseIdentifier(expr, ref pos);
        if (name.Length == 0)
            return null;

        SkipWhitespace(expr, ref pos);

        // Function call
        if (pos < expr.Length && expr[pos] == '(')
        {
            pos++; // consume '('
            var args = ParseArgumentList(expr, ref pos, resolveInput, depth);
            return EvaluateFunction(name, args);
        }

        // Input variable reference
        return resolveInput(name);
    }

    private static List<object?> ParseArgumentList(ReadOnlySpan<char> expr, ref int pos,
        Func<string, object?> resolveInput, int depth)
    {
        var args = new List<object?>();

        SkipWhitespace(expr, ref pos);
        if (pos < expr.Length && expr[pos] == ')')
        {
            pos++;
            return args;
        }

        while (pos < expr.Length)
        {
            args.Add(ParseExpression(expr, ref pos, resolveInput, depth + 1));

            SkipWhitespace(expr, ref pos);
            if (pos >= expr.Length) break;

            if (expr[pos] == ')')
            {
                pos++;
                break;
            }

            if (expr[pos] == ',')
            {
                pos++;
                continue;
            }

            // Unexpected char — bail
            break;
        }

        return args;
    }

    private static object? EvaluateFunction(string name, List<object?> args)
    {
        return name.ToLowerInvariant() switch
        {
            "add" => BinaryArithmetic(args, (a, b) => a + b),
            "subtract" => BinaryArithmetic(args, (a, b) => a - b),
            "multiply" => MultiplyAll(args),
            "divide" => Divide(args),
            "negate" => Negate(args),
            "abs" => Abs(args),
            "avg" => Aggregate(args, vals => vals.Average()),
            "min" => Aggregate(args, vals => vals.Min()),
            "max" => Aggregate(args, vals => vals.Max()),
            "round" => Round(args),
            "gt" => Compare(args, (a, b) => a > b),
            "lt" => Compare(args, (a, b) => a < b),
            "eq" => Eq(args),
            "cond" => Cond(args),
            "coalesce" => Coalesce(args),
            _ => null
        };
    }

    // ── Arithmetic ───────────────────────────────────────────────────────

    private static object? BinaryArithmetic(List<object?> args, Func<double, double, double> op)
    {
        if (args.Count < 2) return null;
        var a = ToDouble(args[0]);
        var b = ToDouble(args[1]);
        if (!a.HasValue || !b.HasValue) return null;
        return op(a.Value, b.Value);
    }

    private static object? MultiplyAll(List<object?> args)
    {
        if (args.Count < 2) return null;
        double result = 1;
        foreach (var arg in args)
        {
            var val = ToDouble(arg);
            if (!val.HasValue) return null;
            result *= val.Value;
        }
        return result;
    }

    private static object? Divide(List<object?> args)
    {
        if (args.Count < 2) return null;
        var a = ToDouble(args[0]);
        var b = ToDouble(args[1]);
        if (!a.HasValue || !b.HasValue) return null;
        if (b.Value == 0) return null;
        return a.Value / b.Value;
    }

    private static object? Negate(List<object?> args)
    {
        if (args.Count < 1) return null;
        var a = ToDouble(args[0]);
        return a.HasValue ? -a.Value : null;
    }

    private static object? Abs(List<object?> args)
    {
        if (args.Count < 1) return null;
        var a = ToDouble(args[0]);
        return a.HasValue ? Math.Abs(a.Value) : null;
    }

    // ── Aggregation ──────────────────────────────────────────────────────

    private static object? Aggregate(List<object?> args, Func<IEnumerable<double>, double> op)
    {
        var values = args.Select(ToDouble).Where(v => v.HasValue).Select(v => v!.Value).ToList();
        return values.Count > 0 ? op(values) : null;
    }

    private static object? Round(List<object?> args)
    {
        if (args.Count < 1) return null;
        var value = ToDouble(args[0]);
        if (!value.HasValue) return null;
        var decimals = args.Count > 1 ? (int)(ToDouble(args[1]) ?? 0) : 0;
        return Math.Round(value.Value, decimals, MidpointRounding.AwayFromZero);
    }

    // ── Comparison ───────────────────────────────────────────────────────

    private static object? Compare(List<object?> args, Func<double, double, bool> op)
    {
        if (args.Count < 2) return null;
        var a = ToDouble(args[0]);
        var b = ToDouble(args[1]);
        if (!a.HasValue || !b.HasValue) return null;
        return op(a.Value, b.Value);
    }

    private static object? Eq(List<object?> args)
    {
        if (args.Count < 2) return null;
        var a = args[0];
        var b = args[1];
        if (a == null && b == null) return true;
        if (a == null || b == null) return false;

        var da = ToDouble(a);
        var db = ToDouble(b);
        if (da.HasValue && db.HasValue) return da.Value == db.Value;

        return string.Equals(a.ToString(), b.ToString(), StringComparison.Ordinal);
    }

    // ── Conditional ──────────────────────────────────────────────────────

    private static object? Cond(List<object?> args)
    {
        if (args.Count < 3) return null;
        var test = args[0];
        var isTruthy = test switch
        {
            bool b => b,
            double d => d != 0,
            int i => i != 0,
            null => false,
            _ => true
        };
        return isTruthy ? args[1] : args[2];
    }

    private static object? Coalesce(List<object?> args)
    {
        foreach (var arg in args)
        {
            if (arg != null) return arg;
        }
        return null;
    }

    // ── Parsing helpers ──────────────────────────────────────────────────

    private static string ParseIdentifier(ReadOnlySpan<char> expr, ref int pos)
    {
        var start = pos;
        while (pos < expr.Length && (char.IsLetterOrDigit(expr[pos]) || expr[pos] == '_'))
            pos++;
        return expr[start..pos].ToString();
    }

    private static double? ParseNumberLiteral(ReadOnlySpan<char> expr, ref int pos)
    {
        var start = pos;
        if (pos < expr.Length && expr[pos] == '-') pos++;
        while (pos < expr.Length && (char.IsDigit(expr[pos]) || expr[pos] == '.'))
            pos++;

        var numberSpan = expr[start..pos];
        return double.TryParse(numberSpan, NumberStyles.Float, CultureInfo.InvariantCulture, out var result)
            ? result
            : null;
    }

    private static string ParseStringLiteral(ReadOnlySpan<char> expr, ref int pos)
    {
        pos++; // skip opening '
        var start = pos;
        while (pos < expr.Length && expr[pos] != '\'')
            pos++;
        var value = expr[start..pos].ToString();
        if (pos < expr.Length) pos++; // skip closing '
        return value;
    }

    private static void SkipWhitespace(ReadOnlySpan<char> expr, ref int pos)
    {
        while (pos < expr.Length && char.IsWhiteSpace(expr[pos]))
            pos++;
    }

    private static bool TryConsume(ReadOnlySpan<char> expr, ref int pos, string keyword)
    {
        if (pos + keyword.Length > expr.Length)
            return false;

        var slice = expr.Slice(pos, keyword.Length);
        if (!slice.SequenceEqual(keyword.AsSpan()))
            return false;

        // Make sure keyword isn't a prefix of a longer identifier
        if (pos + keyword.Length < expr.Length && char.IsLetterOrDigit(expr[pos + keyword.Length]))
            return false;

        pos += keyword.Length;
        return true;
    }

    internal static double? ToDouble(object? value)
    {
        return value switch
        {
            double d => d,
            int i => i,
            long l => l,
            float f => f,
            decimal dec => (double)dec,
            string s => double.TryParse(s, NumberStyles.Float, CultureInfo.InvariantCulture, out var d) ? d : null,
            _ => null
        };
    }
}
