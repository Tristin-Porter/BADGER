using System;
using System.IO;

namespace Badger.Testing;

/// <summary>
/// End-to-end integration tests
/// Tests complete WAT → Assembly → Machine Code → Container pipeline
/// </summary>
public static class IntegrationTests
{
    public static void RunTests()
    {
        TestRunner.RunTest("Simple x86_64 program end-to-end", TestSimpleX86_64Program);
        TestRunner.RunTest("x86_64 Native container end-to-end", TestX86_64NativeContainer);
        TestRunner.RunTest("x86_64 PE container end-to-end", TestX86_64PEContainer);
        TestRunner.RunTest("All architectures can assemble", TestAllArchitecturesAssemble);
    }

    private static void TestSimpleX86_64Program()
    {
        // Simple assembly program
        string assembly = @"
main:
    push rbp
    mov rbp, rsp
    mov rax, 42
    mov rsp, rbp
    pop rbp
    ret
";
        
        // Assemble to machine code
        byte[] machineCode = Badger.Architectures.x86_64.Assembler.Assemble(assembly);
        
        TestRunner.Assert(machineCode != null, "Should produce machine code");
        TestRunner.Assert(machineCode.Length > 0, "Machine code should not be empty");
        
        // Verify some expected opcodes
        TestRunner.Assert(machineCode.Contains(0x55), "Should contain push rbp (0x55)");
        TestRunner.Assert(machineCode.Contains(0xC3), "Should contain ret (0xC3)");
    }

    private static void TestX86_64NativeContainer()
    {
        string assembly = @"
main:
    push rbp
    mov rbp, rsp
    mov rax, 0
    mov rsp, rbp
    pop rbp
    ret
";
        
        byte[] machineCode = Badger.Architectures.x86_64.Assembler.Assemble(assembly);
        byte[] nativeBinary = Badger.Containers.Native.Emit(machineCode);
        
        TestRunner.Assert(nativeBinary != null, "Should produce Native binary");
        TestRunner.AssertArrayEqual(machineCode, nativeBinary, "Native binary should be raw machine code");
    }

    private static void TestX86_64PEContainer()
    {
        string assembly = @"
main:
    push rbp
    mov rbp, rsp
    mov rax, 0
    mov rsp, rbp
    pop rbp
    ret
";
        
        byte[] machineCode = Badger.Architectures.x86_64.Assembler.Assemble(assembly);
        byte[] peBinary = Badger.Containers.PE.Emit(machineCode);
        
        TestRunner.Assert(peBinary != null, "Should produce PE binary");
        TestRunner.Assert(peBinary.Length > machineCode.Length, "PE binary should be larger than raw code");
        
        // Check PE structure
        TestRunner.AssertEqual(0x4D, peBinary[0], "Should have DOS header 'M'");
        TestRunner.AssertEqual(0x5A, peBinary[1], "Should have DOS header 'Z'");
    }

    private static void TestAllArchitecturesAssemble()
    {
        // Test that all architectures can assemble basic code
        
        // x86_64
        string x86_64_asm = "push rbp\nmov rbp, rsp\npop rbp\nret";
        byte[] x86_64_code = Badger.Architectures.x86_64.Assembler.Assemble(x86_64_asm);
        TestRunner.Assert(x86_64_code.Length > 0, "x86_64 should assemble");
        
        // x86_32
        string x86_32_asm = "push ebp\nmov ebp, esp\npop ebp\nret";
        byte[] x86_32_code = Badger.Architectures.x86_32.Assembler.Assemble(x86_32_asm);
        TestRunner.Assert(x86_32_code.Length > 0, "x86_32 should assemble");
        
        // x86_16
        string x86_16_asm = "push bp\nmov bp, sp\npop bp\nret";
        byte[] x86_16_code = Badger.Architectures.x86_16.Assembler.Assemble(x86_16_asm);
        TestRunner.Assert(x86_16_code.Length > 0, "x86_16 should assemble");
        
        // ARM64
        string arm64_asm = "ret";
        byte[] arm64_code = Badger.Architectures.ARM64.Assembler.Assemble(arm64_asm);
        TestRunner.Assert(arm64_code.Length > 0, "ARM64 should assemble");
        
        // ARM32
        string arm32_asm = "bx lr";
        byte[] arm32_code = Badger.Architectures.ARM32.Assembler.Assemble(arm32_asm);
        TestRunner.Assert(arm32_code.Length > 0, "ARM32 should assemble");
    }
}

// Helper extension method
public static class ByteArrayExtensions
{
    public static bool Contains(this byte[] array, byte value)
    {
        foreach (var b in array)
        {
            if (b == value) return true;
        }
        return false;
    }
}
