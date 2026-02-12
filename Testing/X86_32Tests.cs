using System;

namespace Badger.Testing;

/// <summary>
/// Tests for x86_32 assembler and instruction encoding.
/// Verifies correct assembly parsing and machine code generation for x86_32 architecture.
/// </summary>
public static class X86_32Tests
{
    public static void RunTests()
    {
        TestX86_32Assembler();
        TestX86_32Instructions();
    }
    
    private static void TestX86_32Assembler()
    {
        Console.WriteLine("\n--- x86_32 Assembler Tests ---");
        
        // Test basic assembly
        var simpleAsm = "ret";
        var result = Badger.Architectures.x86_32.Assembler.Assemble(simpleAsm);
        TestRunner.Assert(result.Length > 0, "Basic assembly produces output");
        
        // Test assembly with labels
        var labelAsm = @"
            main:
                ret
        ";
        var resultWithLabel = Badger.Architectures.x86_32.Assembler.Assemble(labelAsm);
        TestRunner.Assert(resultWithLabel.Length > 0, "Assembly with labels produces output");
    }
    
    private static void TestX86_32Instructions()
    {
        Console.WriteLine("\n--- x86_32 Instruction Encoding Tests ---");
        
        // Test RET instruction
        var retCode = Badger.Architectures.x86_32.Assembler.Assemble("ret");
        TestRunner.AssertArrayEqual(new byte[] { 0xC3 }, retCode, "RET instruction encoding");
        
        // Test NOP instruction
        var nopCode = Badger.Architectures.x86_32.Assembler.Assemble("nop");
        TestRunner.AssertArrayEqual(new byte[] { 0x90 }, nopCode, "NOP instruction encoding");
        
        // Test PUSH ebp
        var pushCode = Badger.Architectures.x86_32.Assembler.Assemble("push ebp");
        TestRunner.AssertArrayEqual(new byte[] { 0x55 }, pushCode, "PUSH ebp encoding");
        
        // Test POP ebp
        var popCode = Badger.Architectures.x86_32.Assembler.Assemble("pop ebp");
        TestRunner.AssertArrayEqual(new byte[] { 0x5D }, popCode, "POP ebp encoding");
        
        // Test PUSH eax
        var pushEaxCode = Badger.Architectures.x86_32.Assembler.Assemble("push eax");
        TestRunner.AssertArrayEqual(new byte[] { 0x50 }, pushEaxCode, "PUSH eax encoding");
        
        // Test POP eax
        var popEaxCode = Badger.Architectures.x86_32.Assembler.Assemble("pop eax");
        TestRunner.AssertArrayEqual(new byte[] { 0x58 }, popEaxCode, "POP eax encoding");
        
        // Test CDQ (sign extend eax to edx:eax)
        var cdqCode = Badger.Architectures.x86_32.Assembler.Assemble("cdq");
        TestRunner.AssertArrayEqual(new byte[] { 0x99 }, cdqCode, "CDQ encoding");
        
        // Test MOV register to register (no REX prefix!)
        var movCode = Badger.Architectures.x86_32.Assembler.Assemble("mov ebp, esp");
        TestRunner.Assert(movCode.Length == 2 && movCode[0] == 0x89 && movCode[1] == 0xE5, 
               "MOV ebp, esp encoding (no REX prefix)");
        
        // Test MOV immediate
        var movImmCode = Badger.Architectures.x86_32.Assembler.Assemble("mov eax, 42");
        TestRunner.Assert(movImmCode.Length == 5 && movImmCode[0] == 0xB8, 
               "MOV eax, imm32 encoding");
        
        // Test ADD register to register (no REX prefix!)
        var addCode = Badger.Architectures.x86_32.Assembler.Assemble("add eax, ebx");
        TestRunner.Assert(addCode.Length == 2 && addCode[0] == 0x01,
               "ADD eax, ebx encoding (no REX prefix)");
        
        // Test ADD with immediate (8-bit)
        var addImm8Code = Badger.Architectures.x86_32.Assembler.Assemble("add esp, 4");
        TestRunner.AssertArrayEqual(new byte[] { 0x83, 0xC4, 0x04 }, addImm8Code, 
                        "ADD esp, imm8 encoding");
        
        // Test SUB register to register (no REX prefix!)
        var subCode = Badger.Architectures.x86_32.Assembler.Assemble("sub eax, ebx");
        TestRunner.Assert(subCode.Length == 2 && subCode[0] == 0x29,
               "SUB eax, ebx encoding (no REX prefix)");
        
        // Test SUB with immediate (8-bit)
        var subImm8Code = Badger.Architectures.x86_32.Assembler.Assemble("sub esp, 16");
        TestRunner.AssertArrayEqual(new byte[] { 0x83, 0xEC, 0x10 }, subImm8Code,
                        "SUB esp, imm8 encoding");
        
        // Test XOR register to register (no REX prefix!)
        var xorCode = Badger.Architectures.x86_32.Assembler.Assemble("xor edx, edx");
        TestRunner.Assert(xorCode.Length == 2 && xorCode[0] == 0x31,
               "XOR edx, edx encoding (no REX prefix)");
        
        // Test AND register to register (no REX prefix!)
        var andCode = Badger.Architectures.x86_32.Assembler.Assemble("and eax, ebx");
        TestRunner.Assert(andCode.Length == 2 && andCode[0] == 0x21,
               "AND eax, ebx encoding (no REX prefix)");
        
        // Test OR register to register (no REX prefix!)
        var orCode = Badger.Architectures.x86_32.Assembler.Assemble("or eax, ebx");
        TestRunner.Assert(orCode.Length == 2 && orCode[0] == 0x09,
               "OR eax, ebx encoding (no REX prefix)");
        
        // Test CMP register to register (no REX prefix!)
        var cmpCode = Badger.Architectures.x86_32.Assembler.Assemble("cmp eax, ebx");
        TestRunner.Assert(cmpCode.Length == 2 && cmpCode[0] == 0x39,
               "CMP eax, ebx encoding (no REX prefix)");
        
        // Test TEST register to register (no REX prefix!)
        var testCode = Badger.Architectures.x86_32.Assembler.Assemble("test eax, eax");
        TestRunner.Assert(testCode.Length == 2 && testCode[0] == 0x85,
               "TEST eax, eax encoding (no REX prefix)");
        
        // Test IMUL register to register (no REX prefix!)
        var imulCode = Badger.Architectures.x86_32.Assembler.Assemble("imul eax, ebx");
        TestRunner.Assert(imulCode.Length == 3 && imulCode[0] == 0x0F && imulCode[1] == 0xAF,
               "IMUL eax, ebx encoding (no REX prefix)");
        
        // Test IDIV (no REX prefix!)
        var idivCode = Badger.Architectures.x86_32.Assembler.Assemble("idiv ebx");
        TestRunner.Assert(idivCode.Length == 2 && idivCode[0] == 0xF7,
               "IDIV ebx encoding (no REX prefix)");
        
        // Test DIV (no REX prefix!)
        var divCode = Badger.Architectures.x86_32.Assembler.Assemble("div ebx");
        TestRunner.Assert(divCode.Length == 2 && divCode[0] == 0xF7,
               "DIV ebx encoding (no REX prefix)");
        
        // Test conditional jumps
        var jeCode = Badger.Architectures.x86_32.Assembler.Assemble("je target");
        TestRunner.Assert(jeCode.Length == 6 && jeCode[0] == 0x0F && jeCode[1] == 0x84,
               "JE rel32 encoding");
        
        var jneCode = Badger.Architectures.x86_32.Assembler.Assemble("jne target");
        TestRunner.Assert(jneCode.Length == 6 && jneCode[0] == 0x0F && jneCode[1] == 0x85,
               "JNE rel32 encoding");
        
        var jlCode = Badger.Architectures.x86_32.Assembler.Assemble("jl target");
        TestRunner.Assert(jlCode.Length == 6 && jlCode[0] == 0x0F && jlCode[1] == 0x8C,
               "JL rel32 encoding");
        
        var jgCode = Badger.Architectures.x86_32.Assembler.Assemble("jg target");
        TestRunner.Assert(jgCode.Length == 6 && jgCode[0] == 0x0F && jgCode[1] == 0x8F,
               "JG rel32 encoding");
        
        // Test SETcc instructions
        var seteCode = Badger.Architectures.x86_32.Assembler.Assemble("sete al");
        TestRunner.Assert(seteCode.Length == 3 && seteCode[0] == 0x0F && seteCode[1] == 0x94,
               "SETE al encoding");
        
        var setneCode = Badger.Architectures.x86_32.Assembler.Assemble("setne al");
        TestRunner.Assert(setneCode.Length == 3 && setneCode[0] == 0x0F && setneCode[1] == 0x95,
               "SETNE al encoding");
        
        var setlCode = Badger.Architectures.x86_32.Assembler.Assemble("setl al");
        TestRunner.Assert(setlCode.Length == 3 && setlCode[0] == 0x0F && setlCode[1] == 0x9C,
               "SETL al encoding");
        
        var setgCode = Badger.Architectures.x86_32.Assembler.Assemble("setg al");
        TestRunner.Assert(setgCode.Length == 3 && setgCode[0] == 0x0F && setgCode[1] == 0x9F,
               "SETG al encoding");
        
        // Test MOVZX
        var movzxCode = Badger.Architectures.x86_32.Assembler.Assemble("movzx eax, al");
        TestRunner.Assert(movzxCode.Length == 3 && movzxCode[0] == 0x0F && movzxCode[1] == 0xB6,
               "MOVZX eax, al encoding");
        
        // Verify NO REX prefix (0x48) in any 32-bit instruction
        var allInstructions = new[] { 
            movCode, addCode, subCode, xorCode, andCode, orCode, 
            cmpCode, testCode, imulCode, idivCode, divCode 
        };
        foreach (var code in allInstructions)
        {
            TestRunner.Assert(!code.Contains((byte)0x48), 
                   "No REX prefix in x86_32 instructions",
                   "32-bit mode should not use REX prefix (0x48)");
        }
    }
}
