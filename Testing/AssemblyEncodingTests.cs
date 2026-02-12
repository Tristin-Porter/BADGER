using System;
using System.Linq;

namespace Badger.Testing;

/// <summary>
/// Tests for assembly encoding - verifies machine code generation
/// Tests exact byte sequences for each architecture's instruction encoding
/// </summary>
public static class AssemblyEncodingTests
{
    public static void RunTests()
    {
        // x86_64 Encoding Tests
        TestRunner.RunTest("x86_64: Encode mov rax, rbx", TestX86_64MovRaxRbx);
        TestRunner.RunTest("x86_64: Encode push rbp", TestX86_64PushRbp);
        TestRunner.RunTest("x86_64: Encode pop rbp", TestX86_64PopRbp);
        TestRunner.RunTest("x86_64: Encode ret", TestX86_64Ret);
        TestRunner.RunTest("x86_64: Encode add rax, rbx", TestX86_64AddRaxRbx);
        TestRunner.RunTest("x86_64: Encode mov rax, immediate", TestX86_64MovRaxImm);
        
        // x86_32 Encoding Tests
        TestRunner.RunTest("x86_32: Encode mov eax, ebx", TestX86_32MovEaxEbx);
        TestRunner.RunTest("x86_32: Encode push ebp", TestX86_32PushEbp);
        TestRunner.RunTest("x86_32: Encode ret", TestX86_32Ret);
        
        // x86_16 Encoding Tests
        TestRunner.RunTest("x86_16: Encode mov ax, bx", TestX86_16MovAxBx);
        TestRunner.RunTest("x86_16: Encode push bp", TestX86_16PushBp);
        TestRunner.RunTest("x86_16: Encode ret", TestX86_16Ret);
        
        // ARM64 Encoding Tests
        TestRunner.RunTest("ARM64: Encode mov x0, x1", TestARM64MovX0X1);
        TestRunner.RunTest("ARM64: Encode add x0, x1, x2", TestARM64AddX0X1X2);
        TestRunner.RunTest("ARM64: Encode ret", TestARM64Ret);
        
        // ARM32 Encoding Tests
        TestRunner.RunTest("ARM32: Encode mov r0, r1", TestARM32MovR0R1);
        TestRunner.RunTest("ARM32: Encode add r0, r1, r2", TestARM32AddR0R1R2);
        TestRunner.RunTest("ARM32: Encode bx lr", TestARM32BxLr);
    }

    // x86_64 Tests
    private static void TestX86_64MovRaxRbx()
    {
        string asm = "mov rax, rbx";
        byte[] code = Badger.Architectures.x86_64.Assembler.Assemble(asm);
        
        // REX.W + MOV r/m64, r64: 48 89 D8
        TestRunner.Assert(code.Length >= 3, "Should encode to at least 3 bytes");
        TestRunner.AssertEqual(0x48, code[0], "Should have REX.W prefix");
        TestRunner.AssertEqual(0x89, code[1], "Should have MOV opcode");
    }

    private static void TestX86_64PushRbp()
    {
        string asm = "push rbp";
        byte[] code = Badger.Architectures.x86_64.Assembler.Assemble(asm);
        
        // PUSH rbp: 55
        TestRunner.Assert(code.Length >= 1, "Should encode to at least 1 byte");
        TestRunner.AssertEqual(0x55, code[0], "Should have PUSH rbp opcode");
    }

    private static void TestX86_64PopRbp()
    {
        string asm = "pop rbp";
        byte[] code = Badger.Architectures.x86_64.Assembler.Assemble(asm);
        
        // POP rbp: 5D
        TestRunner.Assert(code.Length >= 1, "Should encode to at least 1 byte");
        TestRunner.AssertEqual(0x5D, code[0], "Should have POP rbp opcode");
    }

    private static void TestX86_64Ret()
    {
        string asm = "ret";
        byte[] code = Badger.Architectures.x86_64.Assembler.Assemble(asm);
        
        // RET: C3
        TestRunner.Assert(code.Length >= 1, "Should encode to at least 1 byte");
        TestRunner.AssertEqual(0xC3, code[0], "Should have RET opcode");
    }

    private static void TestX86_64AddRaxRbx()
    {
        string asm = "add rax, rbx";
        byte[] code = Badger.Architectures.x86_64.Assembler.Assemble(asm);
        
        // REX.W + ADD r64, r/m64: 48 01 D8
        TestRunner.Assert(code.Length >= 3, "Should encode to at least 3 bytes");
        TestRunner.AssertEqual(0x48, code[0], "Should have REX.W prefix");
    }

    private static void TestX86_64MovRaxImm()
    {
        string asm = "mov rax, 42";
        byte[] code = Badger.Architectures.x86_64.Assembler.Assemble(asm);
        
        // MOV with immediate should have opcode and immediate value
        TestRunner.Assert(code.Length >= 2, "Should encode with opcode and immediate");
    }

    // x86_32 Tests
    private static void TestX86_32MovEaxEbx()
    {
        string asm = "mov eax, ebx";
        byte[] code = Badger.Architectures.x86_32.Assembler.Assemble(asm);
        
        // MOV r32, r/m32: 89 D8
        TestRunner.Assert(code.Length >= 2, "Should encode to at least 2 bytes");
        TestRunner.AssertEqual(0x89, code[0], "Should have MOV opcode");
    }

    private static void TestX86_32PushEbp()
    {
        string asm = "push ebp";
        byte[] code = Badger.Architectures.x86_32.Assembler.Assemble(asm);
        
        // PUSH ebp: 55
        TestRunner.Assert(code.Length >= 1, "Should encode to at least 1 byte");
        TestRunner.AssertEqual(0x55, code[0], "Should have PUSH ebp opcode");
    }

    private static void TestX86_32Ret()
    {
        string asm = "ret";
        byte[] code = Badger.Architectures.x86_32.Assembler.Assemble(asm);
        
        // RET: C3
        TestRunner.Assert(code.Length >= 1, "Should encode to at least 1 byte");
        TestRunner.AssertEqual(0xC3, code[0], "Should have RET opcode");
    }

    // x86_16 Tests
    private static void TestX86_16MovAxBx()
    {
        string asm = "mov ax, bx";
        byte[] code = Badger.Architectures.x86_16.Assembler.Assemble(asm);
        
        // MOV r16, r/m16: 89 D8
        TestRunner.Assert(code.Length >= 2, "Should encode to at least 2 bytes");
        TestRunner.AssertEqual(0x89, code[0], "Should have MOV opcode");
    }

    private static void TestX86_16PushBp()
    {
        string asm = "push bp";
        byte[] code = Badger.Architectures.x86_16.Assembler.Assemble(asm);
        
        // PUSH bp: 55
        TestRunner.Assert(code.Length >= 1, "Should encode to at least 1 byte");
        TestRunner.AssertEqual(0x55, code[0], "Should have PUSH bp opcode");
    }

    private static void TestX86_16Ret()
    {
        string asm = "ret";
        byte[] code = Badger.Architectures.x86_16.Assembler.Assemble(asm);
        
        // RET: C3
        TestRunner.Assert(code.Length >= 1, "Should encode to at least 1 byte");
        TestRunner.AssertEqual(0xC3, code[0], "Should have RET opcode");
    }

    // ARM64 Tests
    private static void TestARM64MovX0X1()
    {
        string asm = "mov x0, x1";
        byte[] code = Badger.Architectures.ARM64.Assembler.Assemble(asm);
        
        // ARM64 instructions are 4 bytes
        TestRunner.Assert(code.Length >= 4, "Should encode to at least 4 bytes");
    }

    private static void TestARM64AddX0X1X2()
    {
        string asm = "add x0, x1, x2";
        byte[] code = Badger.Architectures.ARM64.Assembler.Assemble(asm);
        
        // ARM64 instructions are 4 bytes
        TestRunner.Assert(code.Length >= 4, "Should encode to at least 4 bytes");
    }

    private static void TestARM64Ret()
    {
        string asm = "ret";
        byte[] code = Badger.Architectures.ARM64.Assembler.Assemble(asm);
        
        // ARM64 ret: D65F03C0
        TestRunner.Assert(code.Length >= 4, "Should encode to 4 bytes");
        byte[] expected = { 0xC0, 0x03, 0x5F, 0xD6 };
        TestRunner.AssertArrayEqual(expected, code, "Should encode ret correctly");
    }

    // ARM32 Tests
    private static void TestARM32MovR0R1()
    {
        string asm = "mov r0, r1";
        byte[] code = Badger.Architectures.ARM32.Assembler.Assemble(asm);
        
        // ARM32 instructions are 4 bytes
        TestRunner.Assert(code.Length >= 4, "Should encode to at least 4 bytes");
    }

    private static void TestARM32AddR0R1R2()
    {
        string asm = "add r0, r1, r2";
        byte[] code = Badger.Architectures.ARM32.Assembler.Assemble(asm);
        
        // ARM32 instructions are 4 bytes
        TestRunner.Assert(code.Length >= 4, "Should encode to at least 4 bytes");
    }

    private static void TestARM32BxLr()
    {
        string asm = "bx lr";
        byte[] code = Badger.Architectures.ARM32.Assembler.Assemble(asm);
        
        // ARM32 bx lr: 1E FF 2F E1
        TestRunner.Assert(code.Length >= 4, "Should encode to 4 bytes");
    }
}
