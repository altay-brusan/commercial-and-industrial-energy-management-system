using Backend.Application.Common.Services.ParameterMapper;
using FluentAssertions;
using NUnit.Framework;

namespace Backend.Application.UnitTests.Common.Services.ParameterMapper;

public class FormulaEvaluatorTests
{
    private static object? Eval(string expr, Dictionary<string, object?>? inputs = null)
    {
        inputs ??= new Dictionary<string, object?>();
        return FormulaEvaluator.Evaluate(expr, name => inputs.GetValueOrDefault(name));
    }

    // ── Arithmetic ───────────────────────────────────────────────────────

    [Test]
    public void Add_TwoNumbers_ReturnsSum()
    {
        Eval("add(a, b)", new() { ["a"] = 3.0, ["b"] = 4.0 }).Should().Be(7.0);
    }

    [Test]
    public void Subtract_TwoNumbers_ReturnsDifference()
    {
        Eval("subtract(a, b)", new() { ["a"] = 10.0, ["b"] = 3.0 }).Should().Be(7.0);
    }

    [Test]
    public void Multiply_TwoNumbers_ReturnsProduct()
    {
        Eval("multiply(a, b)", new() { ["a"] = 3.0, ["b"] = 4.0 }).Should().Be(12.0);
    }

    [Test]
    public void Multiply_ThreeNumbers_ReturnsProduct()
    {
        Eval("multiply(a, b, c)", new() { ["a"] = 2.0, ["b"] = 3.0, ["c"] = 0.001 })
            .Should().Be(0.006);
    }

    [Test]
    public void Divide_TwoNumbers_ReturnsQuotient()
    {
        Eval("divide(a, b)", new() { ["a"] = 10.0, ["b"] = 4.0 }).Should().Be(2.5);
    }

    [Test]
    public void Divide_ByZero_ReturnsNull()
    {
        Eval("divide(a, b)", new() { ["a"] = 10.0, ["b"] = 0.0 }).Should().BeNull();
    }

    [Test]
    public void Negate_ReturnsNegated()
    {
        Eval("negate(a)", new() { ["a"] = 5.0 }).Should().Be(-5.0);
    }

    [Test]
    public void Abs_NegativeNumber_ReturnsPositive()
    {
        Eval("abs(a)", new() { ["a"] = -5.0 }).Should().Be(5.0);
    }

    // ── Null propagation ─────────────────────────────────────────────────

    [Test]
    public void Add_NullInput_ReturnsNull()
    {
        Eval("add(a, b)", new() { ["a"] = 3.0, ["b"] = null }).Should().BeNull();
    }

    [Test]
    public void Multiply_NullInput_ReturnsNull()
    {
        Eval("multiply(a, b, c)", new() { ["a"] = 2.0, ["b"] = null, ["c"] = 3.0 })
            .Should().BeNull();
    }

    // ── Aggregation ──────────────────────────────────────────────────────

    [Test]
    public void Avg_SkipsNulls_ReturnsAverage()
    {
        Eval("avg(a, b, c)", new() { ["a"] = 10.0, ["b"] = null, ["c"] = 20.0 })
            .Should().Be(15.0);
    }

    [Test]
    public void Avg_AllNull_ReturnsNull()
    {
        Eval("avg(a, b)", new() { ["a"] = null, ["b"] = null }).Should().BeNull();
    }

    [Test]
    public void Min_ReturnsSmallest()
    {
        Eval("min(a, b, c)", new() { ["a"] = 10.0, ["b"] = 5.0, ["c"] = 20.0 })
            .Should().Be(5.0);
    }

    [Test]
    public void Max_ReturnsLargest()
    {
        Eval("max(a, b, c)", new() { ["a"] = 10.0, ["b"] = 5.0, ["c"] = 20.0 })
            .Should().Be(20.0);
    }

    // ── Round ────────────────────────────────────────────────────────────

    [Test]
    public void Round_WithDecimals()
    {
        Eval("round(a, 1)", new() { ["a"] = 3.456 }).Should().Be(3.5);
    }

    [Test]
    public void Round_NoDecimalsParam_DefaultsToZero()
    {
        Eval("round(a)", new() { ["a"] = 3.7 }).Should().Be(4.0);
    }

    [Test]
    public void Round_NullInput_ReturnsNull()
    {
        Eval("round(a, 2)", new() { ["a"] = null }).Should().BeNull();
    }

    // ── Comparison ───────────────────────────────────────────────────────

    [Test]
    public void Gt_WhenGreater_ReturnsTrue()
    {
        Eval("gt(a, b)", new() { ["a"] = 5.0, ["b"] = 3.0 }).Should().Be(true);
    }

    [Test]
    public void Gt_WhenEqual_ReturnsFalse()
    {
        Eval("gt(a, b)", new() { ["a"] = 3.0, ["b"] = 3.0 }).Should().Be(false);
    }

    [Test]
    public void Lt_WhenLess_ReturnsTrue()
    {
        Eval("lt(a, b)", new() { ["a"] = 2.0, ["b"] = 5.0 }).Should().Be(true);
    }

    [Test]
    public void Eq_EqualNumbers_ReturnsTrue()
    {
        Eval("eq(a, b)", new() { ["a"] = 3.0, ["b"] = 3.0 }).Should().Be(true);
    }

    [Test]
    public void Eq_NullAndNull_ReturnsTrue()
    {
        Eval("eq(a, b)", new() { ["a"] = null, ["b"] = null }).Should().Be(true);
    }

    [Test]
    public void Eq_NullAndValue_ReturnsFalse()
    {
        Eval("eq(a, b)", new() { ["a"] = null, ["b"] = 3.0 }).Should().Be(false);
    }

    // ── Conditional ──────────────────────────────────────────────────────

    [Test]
    public void Cond_TrueCondition_ReturnsIfTrue()
    {
        Eval("cond(test, a, b)", new() { ["test"] = true, ["a"] = "yes", ["b"] = "no" })
            .Should().Be("yes");
    }

    [Test]
    public void Cond_FalseCondition_ReturnsIfFalse()
    {
        Eval("cond(test, a, b)", new() { ["test"] = false, ["a"] = "yes", ["b"] = "no" })
            .Should().Be("no");
    }

    [Test]
    public void Cond_NullCondition_ReturnsFalse()
    {
        Eval("cond(test, a, b)", new() { ["test"] = null, ["a"] = "yes", ["b"] = "no" })
            .Should().Be("no");
    }

    // ── Coalesce ─────────────────────────────────────────────────────────

    [Test]
    public void Coalesce_FirstNonNull()
    {
        Eval("coalesce(a, b, c)", new() { ["a"] = null, ["b"] = 5.0, ["c"] = 10.0 })
            .Should().Be(5.0);
    }

    [Test]
    public void Coalesce_AllNull_ReturnsNull()
    {
        Eval("coalesce(a, b)", new() { ["a"] = null, ["b"] = null }).Should().BeNull();
    }

    // ── Nested calls ─────────────────────────────────────────────────────

    [Test]
    public void Nested_RoundMultiply()
    {
        // round(multiply(V, I, 0.001), 1)
        Eval("round(multiply(V, I, 0.001), 1)", new() { ["V"] = 1500.0, ["I"] = 100.0 })
            .Should().Be(150.0);
    }

    [Test]
    public void Nested_CondWithComparison()
    {
        // cond(lt(current, 0), 'discharging', cond(gt(current, 0), 'charging', 'idle'))
        var expr = "cond(lt(current, 0), 'discharging', cond(gt(current, 0), 'charging', 'idle'))";

        Eval(expr, new() { ["current"] = -50.0 }).Should().Be("discharging");
        Eval(expr, new() { ["current"] = 50.0 }).Should().Be("charging");
        Eval(expr, new() { ["current"] = 0.0 }).Should().Be("idle");
    }

    [Test]
    public void Nested_RoundAvg()
    {
        Eval("round(avg(a, b), 1)", new() { ["a"] = 10.0, ["b"] = 23.0 })
            .Should().Be(16.5);
    }

    // ── Literals ─────────────────────────────────────────────────────────

    [Test]
    public void StringLiteral_ReturnedDirectly()
    {
        Eval("'hello'").Should().Be("hello");
    }

    [Test]
    public void NumericLiteral_ReturnedDirectly()
    {
        Eval("42").Should().Be(42.0);
    }

    [Test]
    public void NegativeNumericLiteral()
    {
        Eval("-3.14").Should().Be(-3.14);
    }

    [Test]
    public void NullLiteral_ReturnsNull()
    {
        Eval("null").Should().BeNull();
    }

    [Test]
    public void NumericLiteralInFunction()
    {
        Eval("multiply(a, 0.001)", new() { ["a"] = 1500.0 }).Should().Be(1.5);
    }

    // ── Edge cases ───────────────────────────────────────────────────────

    [Test]
    public void EmptyExpression_ReturnsNull()
    {
        Eval("").Should().BeNull();
    }

    [Test]
    public void UnknownFunction_ReturnsNull()
    {
        Eval("unknownFn(a)", new() { ["a"] = 5.0 }).Should().BeNull();
    }

    [Test]
    public void MissingInput_ReturnsNull()
    {
        Eval("add(a, b)", new() { ["a"] = 5.0 }).Should().BeNull();
    }

    [Test]
    public void DeepNesting_RespectedUpToMaxDepth()
    {
        // 5 levels of nesting — should work fine
        Eval("add(add(add(add(add(a, 1), 1), 1), 1), 1)", new() { ["a"] = 0.0 })
            .Should().Be(5.0);
    }

    // ── Real-world formulas from the plan ────────────────────────────────

    [Test]
    public void PowerFormula_VoltageTimesCurrent()
    {
        // round(multiply(voltage, current, 0.001), 1)
        var result = Eval("round(multiply(voltage, current, 0.001), 1)",
            new() { ["voltage"] = 1500.0, ["current"] = -100.0 });
        result.Should().Be(-150.0);
    }

    [Test]
    public void CycleLifePercentage()
    {
        // round(multiply(divide(cycles, designed), 100), 1)
        var result = Eval("round(multiply(divide(cycles, designed), 100), 1)",
            new() { ["cycles"] = 1200.0, ["designed"] = 6000.0 });
        result.Should().Be(20.0);
    }

    [Test]
    public void VoltageSpread()
    {
        // subtract(cellVoltageMax, cellVoltageMin)
        var result = Eval("subtract(cellVoltageMax, cellVoltageMin)",
            new() { ["cellVoltageMax"] = 3450.0, ["cellVoltageMin"] = 3380.0 });
        result.Should().Be(70.0);
    }
}
