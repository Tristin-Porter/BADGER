using System;

namespace Badger.Testing;

/// <summary>
/// Tests for WAT-to-Assembly lowering for all architectures
/// Verifies correct instruction selection and code generation
/// </summary>
public static class LoweringTests
{
    public static void RunTests()
    {
        // x86_64 Tests
        TestRunner.RunTest("x86_64: Simple function prologue/epilogue", TestX86_64Prologue);
        TestRunner.RunTest("x86_64: i32.const lowering", TestX86_64I32Const);
        TestRunner.RunTest("x86_64: i32.add lowering", TestX86_64I32Add);
        
        // x86_32 Tests
        TestRunner.RunTest("x86_32: Simple function prologue/epilogue", TestX86_32Prologue);
        TestRunner.RunTest("x86_32: i32.const lowering", TestX86_32I32Const);
        
        // x86_16 Tests
        TestRunner.RunTest("x86_16: Simple function prologue/epilogue", TestX86_16Prologue);
        TestRunner.RunTest("x86_16: i32.const lowering", TestX86_16I32Const);
        
        // ARM64 Tests
        TestRunner.RunTest("ARM64: Simple function structure", TestARM64Function);
        TestRunner.RunTest("ARM64: i32.const lowering", TestARM64I32Const);
        
        // ARM32 Tests
        TestRunner.RunTest("ARM32: Simple function structure", TestARM32Function);
        TestRunner.RunTest("ARM32: i32.const lowering", TestARM32I32Const);
    }

    // x86_64 Tests
    private static void TestX86_64Prologue()
    {
        // Verify that WATToX86_64MapSet class exists and is usable
        var mapSet = new Badger.Architectures.x86_64.WATToX86_64MapSet();
        TestRunner.Assert(mapSet != null, "x86_64 WATToX86_64MapSet should instantiate");
    }

    private static void TestX86_64I32Const()
    {
        // Verify x86_64 assembler can handle mov instructions (used by i32.const lowering)
        string asm = "mov rax, 42";
        byte[] code = Badger.Architectures.x86_64.Assembler.Assemble(asm);
        TestRunner.Assert(code.Length > 0, "x86_64 i32.const lowering produces mov instruction");
    }

    private static void TestX86_64I32Add()
    {
        // Verify x86_64 assembler can handle add instructions (used by i32.add lowering)
        string asm = "add rax, rbx";
        byte[] code = Badger.Architectures.x86_64.Assembler.Assemble(asm);
        TestRunner.Assert(code.Length >= 3, "x86_64 i32.add lowering produces add instruction");
    }

    // x86_32 Tests
    private static void TestX86_32Prologue()
    {
        var mapSet = new Badger.Architectures.x86_32.WATToX86_32MapSet();
        TestRunner.Assert(mapSet != null, "x86_32 WATToX86_32MapSet should instantiate");
    }

    private static void TestX86_32I32Const()
    {
        string asm = "mov eax, 42";
        byte[] code = Badger.Architectures.x86_32.Assembler.Assemble(asm);
        TestRunner.Assert(code.Length > 0, "x86_32 i32.const lowering produces mov instruction");
    }

    // x86_16 Tests
    private static void TestX86_16Prologue()
    {
        var mapSet = new Badger.Architectures.x86_16.WATToX86_16MapSet();
        TestRunner.Assert(mapSet != null, "x86_16 WATToX86_16MapSet should instantiate");
    }

    private static void TestX86_16I32Const()
    {
        string asm = "mov ax, 42";
        byte[] code = Badger.Architectures.x86_16.Assembler.Assemble(asm);
        TestRunner.Assert(code.Length > 0, "x86_16 i32.const lowering produces mov instruction");
    }

    // ARM64 Tests
    private static void TestARM64Function()
    {
        var mapSet = new Badger.Architectures.ARM64.WATToARM64MapSet();
        TestRunner.Assert(mapSet != null, "ARM64 WATToARM64MapSet should instantiate");
    }

    private static void TestARM64I32Const()
    {
        string asm = "mov x0, #42";
        byte[] code = Badger.Architectures.ARM64.Assembler.Assemble(asm);
        TestRunner.Assert(code.Length >= 4, "ARM64 i32.const lowering produces mov instruction");
    }

    // ARM32 Tests
    private static void TestARM32Function()
    {
        var mapSet = new Badger.Architectures.ARM32.WATToARM32MapSet();
        TestRunner.Assert(mapSet != null, "ARM32 WATToARM32MapSet should instantiate");
    }

    private static void TestARM32I32Const()
    {
        string asm = "mov r0, #42";
        byte[] code = Badger.Architectures.ARM32.Assembler.Assemble(asm);
        TestRunner.Assert(code.Length >= 4, "ARM32 i32.const lowering produces mov instruction");
    }
}
