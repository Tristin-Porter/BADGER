using System;

namespace Badger.Testing;

/// <summary>
/// Tests for WAT token definitions.
/// Verifies that all required WebAssembly text format tokens are properly defined.
/// </summary>
public static class WATTests
{
    public static void RunTests()
    {
        Console.WriteLine("\n--- WAT Token Tests ---");
        
        // Test that token definitions exist and are accessible
        var tokens = new WATTokens();
        
        TestRunner.Assert(tokens.Module != null, "Module token exists");
        TestRunner.Assert(tokens.Func != null, "Func token exists");
        TestRunner.Assert(tokens.I32Add != null, "I32Add token exists");
        TestRunner.Assert(tokens.I32Const != null, "I32Const token exists");
        TestRunner.Assert(tokens.LocalGet != null, "LocalGet token exists");
        TestRunner.Assert(tokens.LocalSet != null, "LocalSet token exists");
        TestRunner.Assert(tokens.Return != null, "Return token exists");
        TestRunner.Assert(tokens.Call != null, "Call token exists");
        TestRunner.Assert(tokens.LeftParen != null, "LeftParen token exists");
        TestRunner.Assert(tokens.RightParen != null, "RightParen token exists");
        TestRunner.Assert(tokens.Identifier != null, "Identifier token exists");
        TestRunner.Assert(tokens.Integer != null, "Integer token exists");
        
        // Test numeric instruction tokens
        TestRunner.Assert(tokens.I32Sub != null, "I32Sub token exists");
        TestRunner.Assert(tokens.I32Mul != null, "I32Mul token exists");
        TestRunner.Assert(tokens.I32DivS != null, "I32DivS token exists");
        TestRunner.Assert(tokens.I32And != null, "I32And token exists");
        TestRunner.Assert(tokens.I32Or != null, "I32Or token exists");
        TestRunner.Assert(tokens.I32Xor != null, "I32Xor token exists");
        
        // Test comparison tokens
        TestRunner.Assert(tokens.I32Eq != null, "I32Eq token exists");
        TestRunner.Assert(tokens.I32Ne != null, "I32Ne token exists");
        TestRunner.Assert(tokens.I32LtS != null, "I32LtS token exists");
        TestRunner.Assert(tokens.I32GtS != null, "I32GtS token exists");
        
        // Test control flow tokens
        TestRunner.Assert(tokens.Block != null, "Block token exists");
        TestRunner.Assert(tokens.Loop != null, "Loop token exists");
        TestRunner.Assert(tokens.If != null, "If token exists");
        TestRunner.Assert(tokens.Br != null, "Br token exists");
        TestRunner.Assert(tokens.BrIf != null, "BrIf token exists");
    }
}
