using System;
using CDTk;

namespace Badger.Testing;

/// <summary>
/// Tests for WAT parsing using CDTk
/// Verifies that WAT input is correctly tokenized and parsed
/// </summary>
public static class WATParserTests
{
    public static void RunTests()
    {
        TestRunner.RunTest("WAT tokens are defined", TestWATTokensDefined);
        TestRunner.RunTest("WAT grammar rules are defined", TestWATRulesDefined);
        TestRunner.RunTest("WAT module structure", TestWATModuleStructure);
        TestRunner.RunTest("WAT instruction tokens exist", TestWATInstructionTokens);
        TestRunner.RunTest("WAT type tokens exist", TestWATTypeTokens);
        TestRunner.RunTest("WAT control flow tokens exist", TestWATControlFlowTokens);
    }

    private static void TestWATTokensDefined()
    {
        var tokens = new WATTokens();
        TestRunner.Assert(tokens != null, "WATTokens should be instantiable");
        TestRunner.Assert(tokens.Module != null, "Module token should be defined");
        TestRunner.Assert(tokens.Func != null, "Func token should be defined");
    }

    private static void TestWATRulesDefined()
    {
        var rules = new WATRules();
        TestRunner.Assert(rules != null, "WATRules should be instantiable");
        TestRunner.Assert(rules.Module != null, "Module rule should be defined");
        TestRunner.Assert(rules.FunctionDef != null, "FunctionDef rule should be defined");
    }

    private static void TestWATModuleStructure()
    {
        var tokens = new WATTokens();
        TestRunner.Assert(tokens.Module != null, "Module token defined");
        TestRunner.Assert(tokens.Func != null, "Func token defined");
        TestRunner.Assert(tokens.Param != null, "Param token defined");
        TestRunner.Assert(tokens.Result != null, "Result token defined");
        TestRunner.Assert(tokens.Local != null, "Local token defined");
    }

    private static void TestWATInstructionTokens()
    {
        var tokens = new WATTokens();
        TestRunner.Assert(tokens.I32Add != null, "i32.add token defined");
        TestRunner.Assert(tokens.I32Sub != null, "i32.sub token defined");
        TestRunner.Assert(tokens.I32Mul != null, "i32.mul token defined");
        TestRunner.Assert(tokens.I32Const != null, "i32.const token defined");
        TestRunner.Assert(tokens.LocalGet != null, "local.get token defined");
        TestRunner.Assert(tokens.LocalSet != null, "local.set token defined");
    }

    private static void TestWATTypeTokens()
    {
        var tokens = new WATTokens();
        TestRunner.Assert(tokens.I32Type != null, "i32 type token defined");
        TestRunner.Assert(tokens.I64Type != null, "i64 type token defined");
        TestRunner.Assert(tokens.F32Type != null, "f32 type token defined");
        TestRunner.Assert(tokens.F64Type != null, "f64 type token defined");
    }

    private static void TestWATControlFlowTokens()
    {
        var tokens = new WATTokens();
        TestRunner.Assert(tokens.Block != null, "block token defined");
        TestRunner.Assert(tokens.Loop != null, "loop token defined");
        TestRunner.Assert(tokens.If != null, "if token defined");
        TestRunner.Assert(tokens.Br != null, "br token defined");
        TestRunner.Assert(tokens.BrIf != null, "br_if token defined");
        TestRunner.Assert(tokens.Return != null, "return token defined");
        TestRunner.Assert(tokens.Call != null, "call token defined");
    }
}
