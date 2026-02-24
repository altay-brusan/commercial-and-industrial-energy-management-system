using Backend.Application.Common.Services.ParameterMapper;
using FluentAssertions;
using Microsoft.Extensions.Caching.Memory;
using NUnit.Framework;

namespace Backend.Application.UnitTests.Common.Services.ParameterMapper;

public class ParameterMapParserTests
{
    [Test]
    public void Parse_NullSpecifications_ReturnsEmpty()
    {
        ParameterMapParser.Parse(null).Should().BeEmpty();
    }

    [Test]
    public void Parse_EmptyString_ReturnsEmpty()
    {
        ParameterMapParser.Parse("").Should().BeEmpty();
    }

    [Test]
    public void Parse_NoParameterMapKey_ReturnsEmpty()
    {
        var json = """{"supportedTabs": []}""";
        ParameterMapParser.Parse(json).Should().BeEmpty();
    }

    [Test]
    public void Parse_MalformedJson_ReturnsEmpty()
    {
        ParameterMapParser.Parse("{bad json").Should().BeEmpty();
    }

    [Test]
    public void Parse_EmptyParameters_ReturnsEmpty()
    {
        var json = """{"parameterMap": {"parameters": {}}}""";
        ParameterMapParser.Parse(json).Should().BeEmpty();
    }

    [Test]
    public void Parse_SimpleSource_ParsesCorrectly()
    {
        var json = """
        {
            "parameterMap": {
                "version": 1,
                "parameters": {
                    "soc": {
                        "type": "number",
                        "unit": "%",
                        "source": {
                            "kind": "ContainerBatteryState",
                            "paths": ["socPercent", "stateOfCharge", "soc"]
                        }
                    }
                }
            }
        }
        """;

        var result = ParameterMapParser.Parse(json);

        result.Should().ContainKey("soc");
        var soc = result["soc"];
        soc.Type.Should().Be("number");
        soc.Unit.Should().Be("%");
        soc.Source.Should().NotBeNull();
        soc.Source!.Kind.Should().Be("ContainerBatteryState");
        soc.Source.Paths.Should().BeEquivalentTo(new[] { "socPercent", "stateOfCharge", "soc" });
    }

    [Test]
    public void Parse_FormulaWithRef_ParsesCorrectly()
    {
        var json = """
        {
            "parameterMap": {
                "parameters": {
                    "power": {
                        "type": "number",
                        "unit": "kW",
                        "formula": {
                            "expression": "round(multiply(voltage, current, 0.001), 1)",
                            "inputs": {
                                "voltage": {"$ref": "voltage"},
                                "current": {"$ref": "current"}
                            }
                        },
                        "fallback": {
                            "kind": "ContainerBatteryState",
                            "paths": ["dcPower.power", "power"]
                        }
                    }
                }
            }
        }
        """;

        var result = ParameterMapParser.Parse(json);

        result.Should().ContainKey("power");
        var power = result["power"];
        power.Formula.Should().NotBeNull();
        power.Formula!.Expression.Should().Be("round(multiply(voltage, current, 0.001), 1)");
        power.Formula.Inputs.Should().ContainKey("voltage");
        power.Formula.Inputs["voltage"].Ref.Should().Be("voltage");
        power.Formula.Inputs["current"].Ref.Should().Be("current");
        power.Fallback.Should().NotBeNull();
        power.Fallback!.Kind.Should().Be("ContainerBatteryState");
    }

    [Test]
    public void Parse_FormulaWithInlineSource_ParsesCorrectly()
    {
        var json = """
        {
            "parameterMap": {
                "parameters": {
                    "cycleLifePct": {
                        "type": "number",
                        "formula": {
                            "expression": "round(multiply(divide(cycles, designed), 100), 1)",
                            "inputs": {
                                "cycles": {"$ref": "cycleCount"},
                                "designed": {
                                    "source": {
                                        "origin": "metadata",
                                        "paths": ["designedCycleLife"],
                                        "default": 6000
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }
        """;

        var result = ParameterMapParser.Parse(json);

        result.Should().ContainKey("cycleLifePct");
        var def = result["cycleLifePct"];
        def.Formula.Should().NotBeNull();
        var designed = def.Formula!.Inputs["designed"];
        designed.Source.Should().NotBeNull();
        designed.Source!.Origin.Should().Be("metadata");
        designed.Source.Paths.Should().Contain("designedCycleLife");
        designed.Source.Default.Should().Be(6000.0);
    }

    [Test]
    public void Parse_BooleanType_ParsesCorrectly()
    {
        var json = """
        {
            "parameterMap": {
                "parameters": {
                    "compressorRunning": {
                        "type": "boolean",
                        "source": {
                            "kind": "ChillerState",
                            "paths": ["compressorStatus"]
                        }
                    }
                }
            }
        }
        """;

        var result = ParameterMapParser.Parse(json);

        result.Should().ContainKey("compressorRunning");
        result["compressorRunning"].Type.Should().Be("boolean");
    }

    [Test]
    public void Parse_MultipleParameters_AllParsed()
    {
        var json = """
        {
            "parameterMap": {
                "version": 1,
                "parameters": {
                    "soc": { "type": "number", "source": { "kind": "B", "paths": ["soc"] } },
                    "voltage": { "type": "number", "source": { "kind": "B", "paths": ["v"] } },
                    "current": { "type": "number", "source": { "kind": "B", "paths": ["i"] } }
                }
            }
        }
        """;

        var result = ParameterMapParser.Parse(json);
        result.Should().HaveCount(3);
        result.Keys.Should().Contain(new[] { "soc", "voltage", "current" });
    }

    [Test]
    public void Parse_SourceWithDefaultValue_ParsesDefault()
    {
        var json = """
        {
            "parameterMap": {
                "parameters": {
                    "test": {
                        "type": "number",
                        "source": { "paths": ["missing"], "default": 42 }
                    }
                }
            }
        }
        """;

        var result = ParameterMapParser.Parse(json);
        result["test"].Source!.Default.Should().Be(42.0);
    }

    [Test]
    public void Parse_StringDefault_ParsesCorrectly()
    {
        var json = """
        {
            "parameterMap": {
                "parameters": {
                    "test": {
                        "type": "string",
                        "source": { "paths": ["missing"], "default": "unknown" }
                    }
                }
            }
        }
        """;

        var result = ParameterMapParser.Parse(json);
        result["test"].Source!.Default.Should().Be("unknown");
    }

    // ── Caching ──────────────────────────────────────────────────────────

    [Test]
    public void ParseCached_ReturnsSameInstance()
    {
        var cache = new MemoryCache(new MemoryCacheOptions());
        var json = """
        {
            "parameterMap": {
                "parameters": {
                    "soc": { "type": "number", "source": { "kind": "B", "paths": ["soc"] } }
                }
            }
        }
        """;

        var first = ParameterMapParser.ParseCached(json, cache);
        var second = ParameterMapParser.ParseCached(json, cache);

        first.Should().BeSameAs(second);
    }

    [Test]
    public void ParseCached_NullSpecs_ReturnsEmpty()
    {
        var cache = new MemoryCache(new MemoryCacheOptions());
        ParameterMapParser.ParseCached(null, cache).Should().BeEmpty();
    }

    [Test]
    public void Parse_LabelField_Parsed()
    {
        var json = """
        {
            "parameterMap": {
                "parameters": {
                    "soc": {
                        "type": "number",
                        "label": "State of Charge",
                        "source": { "kind": "B", "paths": ["soc"] }
                    }
                }
            }
        }
        """;

        var result = ParameterMapParser.Parse(json);
        result["soc"].Label.Should().Be("State of Charge");
    }
}
