using System.Text.Json;
using Backend.Application.Common.Services.ParameterMapper;
using FluentAssertions;
using NUnit.Framework;

namespace Backend.Application.UnitTests.Common.Services.ParameterMapper;

public class ParameterResolverTests
{
    private static JsonElement? MakePayload(object obj)
    {
        var json = JsonSerializer.Serialize(obj);
        using var doc = JsonDocument.Parse(json);
        return doc.RootElement.Clone();
    }

    private static Dictionary<string, JsonElement?> MakePayloads(params (string kind, object data)[] entries)
    {
        var result = new Dictionary<string, JsonElement?>();
        foreach (var (kind, data) in entries)
            result[kind] = MakePayload(data);
        return result;
    }

    // ── Simple source extraction ─────────────────────────────────────────

    [Test]
    public void Resolve_SimpleSource_ExtractsFromPayload()
    {
        var map = new Dictionary<string, ParameterDefinition>
        {
            ["soc"] = new()
            {
                Key = "soc",
                Source = new ParameterSource
                {
                    Kind = "ContainerBatteryState",
                    Paths = new[] { "socPercent" }
                }
            }
        };

        var payloads = MakePayloads(("ContainerBatteryState", new { socPercent = 85.5 }));
        var resolver = new ParameterResolver(map, payloads);

        resolver.ResolveNumber("soc").Should().Be(85.5);
    }

    [Test]
    public void Resolve_MultiPathFallback_UsesFirstMatch()
    {
        var map = new Dictionary<string, ParameterDefinition>
        {
            ["soc"] = new()
            {
                Key = "soc",
                Source = new ParameterSource
                {
                    Kind = "ContainerBatteryState",
                    Paths = new[] { "socPercent", "stateOfCharge", "soc" }
                }
            }
        };

        // socPercent is missing, stateOfCharge is present
        var payloads = MakePayloads(("ContainerBatteryState", new { stateOfCharge = 72.0 }));
        var resolver = new ParameterResolver(map, payloads);

        resolver.ResolveNumber("soc").Should().Be(72.0);
    }

    [Test]
    public void Resolve_MissingKind_ReturnsNull()
    {
        var map = new Dictionary<string, ParameterDefinition>
        {
            ["soc"] = new()
            {
                Key = "soc",
                Source = new ParameterSource
                {
                    Kind = "MissingKind",
                    Paths = new[] { "soc" }
                }
            }
        };

        var payloads = MakePayloads(("ContainerBatteryState", new { soc = 80.0 }));
        var resolver = new ParameterResolver(map, payloads);

        resolver.ResolveNumber("soc").Should().BeNull();
    }

    [Test]
    public void Resolve_SourceDefault_UsedWhenPathMissing()
    {
        var map = new Dictionary<string, ParameterDefinition>
        {
            ["test"] = new()
            {
                Key = "test",
                Source = new ParameterSource
                {
                    Kind = "ContainerBatteryState",
                    Paths = new[] { "nonExistent" },
                    Default = 42.0
                }
            }
        };

        var payloads = MakePayloads(("ContainerBatteryState", new { other = 1 }));
        var resolver = new ParameterResolver(map, payloads);

        resolver.ResolveNumber("test").Should().Be(42.0);
    }

    // ── Formula resolution ───────────────────────────────────────────────

    [Test]
    public void Resolve_Formula_ComputesFromInputRefs()
    {
        var map = new Dictionary<string, ParameterDefinition>
        {
            ["voltage"] = new()
            {
                Key = "voltage",
                Source = new ParameterSource { Kind = "B", Paths = new[] { "totalVoltageV" } }
            },
            ["current"] = new()
            {
                Key = "current",
                Source = new ParameterSource { Kind = "B", Paths = new[] { "instantCurrentA" } }
            },
            ["power"] = new()
            {
                Key = "power",
                Formula = new ParameterFormula
                {
                    Expression = "round(multiply(voltage, current, 0.001), 1)",
                    Inputs = new Dictionary<string, FormulaInput>
                    {
                        ["voltage"] = new() { Ref = "voltage" },
                        ["current"] = new() { Ref = "current" }
                    }
                }
            }
        };

        var payloads = MakePayloads(("B", new { totalVoltageV = 1500.0, instantCurrentA = -100.0 }));
        var resolver = new ParameterResolver(map, payloads);

        resolver.ResolveNumber("power").Should().Be(-150.0);
    }

    [Test]
    public void Resolve_FormulaWithFallback_UsesFallbackWhenFormulaReturnsNull()
    {
        var map = new Dictionary<string, ParameterDefinition>
        {
            ["power"] = new()
            {
                Key = "power",
                Formula = new ParameterFormula
                {
                    Expression = "multiply(voltage, current)",
                    Inputs = new Dictionary<string, FormulaInput>
                    {
                        ["voltage"] = new() { Ref = "voltage" },
                        ["current"] = new() { Ref = "current" }
                    }
                },
                Fallback = new ParameterSource
                {
                    Kind = "B",
                    Paths = new[] { "dcPower" }
                }
            }
        };

        // voltage/current not in map, so formula returns null → falls back
        var payloads = MakePayloads(("B", new { dcPower = 150.0 }));
        var resolver = new ParameterResolver(map, payloads);

        resolver.ResolveNumber("power").Should().Be(150.0);
    }

    // ── Cross-Kind resolution ────────────────────────────────────────────

    [Test]
    public void Resolve_CrossKind_ExtractsFromDifferentPayloads()
    {
        var map = new Dictionary<string, ParameterDefinition>
        {
            ["soc"] = new()
            {
                Key = "soc",
                Source = new ParameterSource { Kind = "ContainerBatteryState", Paths = new[] { "socPercent" } }
            },
            ["pcsActivePower"] = new()
            {
                Key = "pcsActivePower",
                Source = new ParameterSource { Kind = "PcsPowerState", Paths = new[] { "pcs_outActPwrKw" } }
            }
        };

        var payloads = MakePayloads(
            ("ContainerBatteryState", new { socPercent = 80.0 }),
            ("PcsPowerState", new { pcs_outActPwrKw = 250.5 })
        );
        var resolver = new ParameterResolver(map, payloads);

        resolver.ResolveNumber("soc").Should().Be(80.0);
        resolver.ResolveNumber("pcsActivePower").Should().Be(250.5);
    }

    // ── Metadata input ───────────────────────────────────────────────────

    [Test]
    public void Resolve_MetadataInput_ReadsFromMetadata()
    {
        var map = new Dictionary<string, ParameterDefinition>
        {
            ["cycleCount"] = new()
            {
                Key = "cycleCount",
                Source = new ParameterSource { Kind = "B", Paths = new[] { "cycleCount" } }
            },
            ["cycleLifePct"] = new()
            {
                Key = "cycleLifePct",
                Formula = new ParameterFormula
                {
                    Expression = "round(multiply(divide(cycles, designed), 100), 1)",
                    Inputs = new Dictionary<string, FormulaInput>
                    {
                        ["cycles"] = new() { Ref = "cycleCount" },
                        ["designed"] = new()
                        {
                            Source = new ParameterSource
                            {
                                Origin = "metadata",
                                Paths = new[] { "designedCycleLife" },
                                Default = 6000.0
                            }
                        }
                    }
                }
            }
        };

        var payloads = MakePayloads(("B", new { cycleCount = 1200 }));
        var metadata = MakePayload(new { designedCycleLife = 6000 });
        var resolver = new ParameterResolver(map, payloads, metadata);

        resolver.ResolveNumber("cycleLifePct").Should().Be(20.0);
    }

    [Test]
    public void Resolve_MetadataDefault_UsedWhenMetadataMissing()
    {
        var map = new Dictionary<string, ParameterDefinition>
        {
            ["cycleCount"] = new()
            {
                Key = "cycleCount",
                Source = new ParameterSource { Kind = "B", Paths = new[] { "cycleCount" } }
            },
            ["cycleLifePct"] = new()
            {
                Key = "cycleLifePct",
                Formula = new ParameterFormula
                {
                    Expression = "round(multiply(divide(cycles, designed), 100), 1)",
                    Inputs = new Dictionary<string, FormulaInput>
                    {
                        ["cycles"] = new() { Ref = "cycleCount" },
                        ["designed"] = new()
                        {
                            Source = new ParameterSource
                            {
                                Origin = "metadata",
                                Paths = new[] { "designedCycleLife" },
                                Default = 6000.0
                            }
                        }
                    }
                }
            }
        };

        var payloads = MakePayloads(("B", new { cycleCount = 1200 }));
        // No metadata at all
        var resolver = new ParameterResolver(map, payloads);

        resolver.ResolveNumber("cycleLifePct").Should().Be(20.0);
    }

    // ── $ref chains ──────────────────────────────────────────────────────

    [Test]
    public void Resolve_RefChain_ResolvesTransitively()
    {
        var map = new Dictionary<string, ParameterDefinition>
        {
            ["tempMin"] = new()
            {
                Key = "tempMin",
                Source = new ParameterSource { Kind = "B", Paths = new[] { "minCellTempC" } }
            },
            ["tempMax"] = new()
            {
                Key = "tempMax",
                Source = new ParameterSource { Kind = "B", Paths = new[] { "maxCellTempC" } }
            },
            ["tempAvg"] = new()
            {
                Key = "tempAvg",
                Formula = new ParameterFormula
                {
                    Expression = "round(avg(tempMin, tempMax), 1)",
                    Inputs = new Dictionary<string, FormulaInput>
                    {
                        ["tempMin"] = new() { Ref = "tempMin" },
                        ["tempMax"] = new() { Ref = "tempMax" }
                    }
                }
            }
        };

        var payloads = MakePayloads(("B", new { minCellTempC = 22.3, maxCellTempC = 28.7 }));
        var resolver = new ParameterResolver(map, payloads);

        resolver.ResolveNumber("tempAvg").Should().Be(25.5);
    }

    [Test]
    public void Resolve_CyclicRef_ReturnsNull()
    {
        var map = new Dictionary<string, ParameterDefinition>
        {
            ["a"] = new()
            {
                Key = "a",
                Formula = new ParameterFormula
                {
                    Expression = "add(b, 1)",
                    Inputs = new Dictionary<string, FormulaInput> { ["b"] = new() { Ref = "b" } }
                }
            },
            ["b"] = new()
            {
                Key = "b",
                Formula = new ParameterFormula
                {
                    Expression = "add(a, 1)",
                    Inputs = new Dictionary<string, FormulaInput> { ["a"] = new() { Ref = "a" } }
                }
            }
        };

        var payloads = new Dictionary<string, JsonElement?>();
        var resolver = new ParameterResolver(map, payloads);

        resolver.ResolveNumber("a").Should().BeNull();
    }

    // ── ResolveAll ───────────────────────────────────────────────────────

    [Test]
    public void ResolveAll_ResolvesEveryParameter()
    {
        var map = new Dictionary<string, ParameterDefinition>
        {
            ["soc"] = new()
            {
                Key = "soc",
                Source = new ParameterSource { Kind = "B", Paths = new[] { "soc" } }
            },
            ["voltage"] = new()
            {
                Key = "voltage",
                Source = new ParameterSource { Kind = "B", Paths = new[] { "v" } }
            }
        };

        var payloads = MakePayloads(("B", new { soc = 80.0, v = 1500.0 }));
        var resolver = new ParameterResolver(map, payloads);

        var all = resolver.ResolveAll();
        all.Should().ContainKey("soc").WhoseValue.Should().Be(80.0);
        all.Should().ContainKey("voltage").WhoseValue.Should().Be(1500.0);
    }

    // ── ResolveString / ResolveBool ──────────────────────────────────────

    [Test]
    public void ResolveString_ReturnsStringValue()
    {
        var map = new Dictionary<string, ParameterDefinition>
        {
            ["mode"] = new()
            {
                Key = "mode",
                Source = new ParameterSource { Kind = "B", Paths = new[] { "state" } }
            }
        };

        var payloads = MakePayloads(("B", new { state = "discharge" }));
        var resolver = new ParameterResolver(map, payloads);

        resolver.ResolveString("mode").Should().Be("discharge");
    }

    [Test]
    public void ResolveBool_ReturnsBoolValue()
    {
        var map = new Dictionary<string, ParameterDefinition>
        {
            ["running"] = new()
            {
                Key = "running",
                Source = new ParameterSource { Kind = "C", Paths = new[] { "compressorStatus" } }
            }
        };

        var payloads = MakePayloads(("C", new { compressorStatus = true }));
        var resolver = new ParameterResolver(map, payloads);

        resolver.ResolveBool("running").Should().BeTrue();
    }

    // ── Legacy ResolveSourcePath ─────────────────────────────────────────

    [Test]
    public void ResolveSourcePath_WithKind_ExtractsFromSpecificPayload()
    {
        var map = new Dictionary<string, ParameterDefinition>();
        var payloads = MakePayloads(
            ("ContainerBatteryState", new { socPercent = 85.0 }),
            ("PcsPowerState", new { power = 250.0 })
        );
        var resolver = new ParameterResolver(map, payloads);

        resolver.ResolveSourcePath("socPercent", "ContainerBatteryState").Should().Be(85.0);
    }

    [Test]
    public void ResolveSourcePath_WithoutKind_SearchesAllPayloads()
    {
        var map = new Dictionary<string, ParameterDefinition>();
        var payloads = MakePayloads(("B", new { socPercent = 90.0 }));
        var resolver = new ParameterResolver(map, payloads);

        resolver.ResolveSourcePath("socPercent").Should().Be(90.0);
    }

    // ── Unknown parameter ────────────────────────────────────────────────

    [Test]
    public void Resolve_UnknownParameter_ReturnsNull()
    {
        var map = new Dictionary<string, ParameterDefinition>();
        var payloads = new Dictionary<string, JsonElement?>();
        var resolver = new ParameterResolver(map, payloads);

        resolver.ResolveNumber("nonexistent").Should().BeNull();
    }

    // ── Caching within resolver ──────────────────────────────────────────

    [Test]
    public void Resolve_CachesResultPerRequest()
    {
        var map = new Dictionary<string, ParameterDefinition>
        {
            ["voltage"] = new()
            {
                Key = "voltage",
                Source = new ParameterSource { Kind = "B", Paths = new[] { "v" } }
            },
            ["power"] = new()
            {
                Key = "power",
                Formula = new ParameterFormula
                {
                    Expression = "multiply(voltage, 2)",
                    Inputs = new Dictionary<string, FormulaInput>
                    {
                        ["voltage"] = new() { Ref = "voltage" }
                    }
                }
            }
        };

        var payloads = MakePayloads(("B", new { v = 100.0 }));
        var resolver = new ParameterResolver(map, payloads);

        // Resolve voltage twice (once directly, once via power formula)
        var v1 = resolver.ResolveNumber("voltage");
        var power = resolver.ResolveNumber("power");

        v1.Should().Be(100.0);
        power.Should().Be(200.0);
    }

    // ── PowerDirection formula (real-world) ──────────────────────────────

    [Test]
    public void Resolve_PowerDirectionFormula_Discharging()
    {
        var map = ParameterMapParser.Parse("""
        {
            "parameterMap": {
                "parameters": {
                    "current": {
                        "type": "number",
                        "source": { "kind": "B", "paths": ["instantCurrentA"] }
                    },
                    "powerDirection": {
                        "type": "string",
                        "formula": {
                            "expression": "cond(lt(current, 0), 'discharging', cond(gt(current, 0), 'charging', 'idle'))",
                            "inputs": { "current": {"$ref": "current"} }
                        }
                    }
                }
            }
        }
        """);

        var payloads = MakePayloads(("B", new { instantCurrentA = -50.0 }));
        var resolver = new ParameterResolver(map, payloads);

        resolver.ResolveString("powerDirection").Should().Be("discharging");
    }

    [Test]
    public void Resolve_PowerDirectionFormula_Charging()
    {
        var map = ParameterMapParser.Parse("""
        {
            "parameterMap": {
                "parameters": {
                    "current": {
                        "type": "number",
                        "source": { "kind": "B", "paths": ["instantCurrentA"] }
                    },
                    "powerDirection": {
                        "type": "string",
                        "formula": {
                            "expression": "cond(lt(current, 0), 'discharging', cond(gt(current, 0), 'charging', 'idle'))",
                            "inputs": { "current": {"$ref": "current"} }
                        }
                    }
                }
            }
        }
        """);

        var payloads = MakePayloads(("B", new { instantCurrentA = 50.0 }));
        var resolver = new ParameterResolver(map, payloads);

        resolver.ResolveString("powerDirection").Should().Be("charging");
    }

    // ── No Kind specified in source (search all payloads) ────────────────

    [Test]
    public void Resolve_NoKindInSource_SearchesAllPayloads()
    {
        var map = new Dictionary<string, ParameterDefinition>
        {
            ["test"] = new()
            {
                Key = "test",
                Source = new ParameterSource
                {
                    Paths = new[] { "someField" }
                }
            }
        };

        var payloads = MakePayloads(("AnyKind", new { someField = 42.0 }));
        var resolver = new ParameterResolver(map, payloads);

        resolver.ResolveNumber("test").Should().Be(42.0);
    }
}
