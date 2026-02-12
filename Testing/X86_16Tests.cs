using System;

namespace Badger.Testing;

/// <summary>
/// Tests for x86_16 assembler and instruction encoding.
/// Verifies correct assembly parsing and machine code generation for x86_16 real mode architecture.
/// </summary>
public static class X86_16Tests
{
    public static void RunTests()
    {
        TestX86_16Assembler();
        TestX86_16Instructions();
    }
    
    private static void TestX86_16Assembler()
    {
        Console.WriteLine("\n--- x86_16 Assembler Tests ---");
        
        // Test basic assembly
        var simpleAsm = "ret";
        var result = Badger.Architectures.x86_16.Assembler.Assemble(simpleAsm);
        TestRunner.Assert(result.Length > 0, "Basic assembly produces output");
        
        // Test assembly with labels
        var labelAsm = @"
            main:
                ret
        ";
        var resultWithLabel = Badger.Architectures.x86_16.Assembler.Assemble(labelAsm);
        TestRunner.Assert(resultWithLabel.Length > 0, "Assembly with labels produces output");
    }
    
    private static void TestX86_16Instructions()
    {
        Console.WriteLine("\n--- x86_16 Instruction Encoding Tests ---");
        
        // Test RET instruction (near return)
        var retCode = Badger.Architectures.x86_16.Assembler.Assemble("ret");
        TestRunner.AssertArrayEqual(new byte[] { 0xC3 }, retCode, "RET instruction encoding");
        
        // Test RETF instruction (far return for real mode)
        var retfCode = Badger.Architectures.x86_16.Assembler.Assemble("retf");
        TestRunner.AssertArrayEqual(new byte[] { 0xCB }, retfCode, "RETF instruction encoding");
        
        // Test NOP instruction
        var nopCode = Badger.Architectures.x86_16.Assembler.Assemble("nop");
        TestRunner.AssertArrayEqual(new byte[] { 0x90 }, nopCode, "NOP instruction encoding");
        
        // Test PUSH bp (16-bit)
        var pushCode = Badger.Architectures.x86_16.Assembler.Assemble("push bp");
        TestRunner.AssertArrayEqual(new byte[] { 0x55 }, pushCode, "PUSH bp encoding");
        
        // Test POP bp (16-bit)
        var popCode = Badger.Architectures.x86_16.Assembler.Assemble("pop bp");
        TestRunner.AssertArrayEqual(new byte[] { 0x5D }, popCode, "POP bp encoding");
        
        // Test PUSH ax
        var pushAxCode = Badger.Architectures.x86_16.Assembler.Assemble("push ax");
        TestRunner.AssertArrayEqual(new byte[] { 0x50 }, pushAxCode, "PUSH ax encoding");
        
        // Test POP ax
        var popAxCode = Badger.Architectures.x86_16.Assembler.Assemble("pop ax");
        TestRunner.AssertArrayEqual(new byte[] { 0x58 }, popAxCode, "POP ax encoding");
        
        // Test MOV register to register (16-bit, no REX!)
        var movCode = Badger.Architectures.x86_16.Assembler.Assemble("mov bp, sp");
        TestRunner.Assert(movCode.Length == 2 && movCode[0] == 0x89 && movCode[1] == 0xE5, 
               "MOV bp, sp encoding (no REX prefix)");
        
        // Test MOV immediate (16-bit)
        var movImmCode = Badger.Architectures.x86_16.Assembler.Assemble("mov ax, 100");
        TestRunner.Assert(movImmCode.Length == 3 && movImmCode[0] == 0xB8 && 
               movImmCode[1] == 100 && movImmCode[2] == 0, 
               "MOV ax, imm16 encoding (3 bytes total)");
        
        // Test ADD register to register (16-bit, no REX!)
        var addCode = Badger.Architectures.x86_16.Assembler.Assemble("add ax, bx");
        TestRunner.Assert(addCode.Length == 2 && addCode[0] == 0x01,
               "ADD ax, bx encoding (no REX prefix)");
        
        // Test ADD with immediate (8-bit)
        var addImm8Code = Badger.Architectures.x86_16.Assembler.Assemble("add sp, 2");
        TestRunner.AssertArrayEqual(new byte[] { 0x83, 0xC4, 0x02 }, addImm8Code, 
                        "ADD sp, imm8 encoding");
        
        // Test ADD with immediate (16-bit)
        var addImm16Code = Badger.Architectures.x86_16.Assembler.Assemble("add sp, 300");
        TestRunner.Assert(addImm16Code.Length == 4 && addImm16Code[0] == 0x81 && 
               addImm16Code[1] == 0xC4,
               "ADD sp, imm16 encoding (4 bytes total)");
        
        // Test SUB register to register (16-bit, no REX!)
        var subCode = Badger.Architectures.x86_16.Assembler.Assemble("sub ax, bx");
        TestRunner.Assert(subCode.Length == 2 && subCode[0] == 0x29,
               "SUB ax, bx encoding (no REX prefix)");
        
        // Test SUB with immediate (8-bit)
        var subImm8Code = Badger.Architectures.x86_16.Assembler.Assemble("sub sp, 8");
        TestRunner.AssertArrayEqual(new byte[] { 0x83, 0xEC, 0x08 }, subImm8Code,
                        "SUB sp, imm8 encoding");
        
        // Test SUB with immediate (16-bit)
        var subImm16Code = Badger.Architectures.x86_16.Assembler.Assemble("sub sp, 200");
        TestRunner.Assert(subImm16Code.Length == 4 && subImm16Code[0] == 0x81 && 
               subImm16Code[1] == 0xEC,
               "SUB sp, imm16 encoding (4 bytes total)");
        
        // Test XOR register to register (16-bit, no REX!)
        var xorCode = Badger.Architectures.x86_16.Assembler.Assemble("xor dx, dx");
        TestRunner.Assert(xorCode.Length == 2 && xorCode[0] == 0x31,
               "XOR dx, dx encoding (no REX prefix)");
        
        // Test AND register to register (16-bit, no REX!)
        var andCode = Badger.Architectures.x86_16.Assembler.Assemble("and ax, bx");
        TestRunner.Assert(andCode.Length == 2 && andCode[0] == 0x21,
               "AND ax, bx encoding (no REX prefix)");
        
        // Test OR register to register (16-bit, no REX!)
        var orCode = Badger.Architectures.x86_16.Assembler.Assemble("or cx, dx");
        TestRunner.Assert(orCode.Length == 2 && orCode[0] == 0x09,
               "OR cx, dx encoding (no REX prefix)");
        
        // Test CMP register to register (16-bit, no REX!)
        var cmpCode = Badger.Architectures.x86_16.Assembler.Assemble("cmp ax, bx");
        TestRunner.Assert(cmpCode.Length == 2 && cmpCode[0] == 0x39,
               "CMP ax, bx encoding (no REX prefix)");
        
        // Test TEST register to register (16-bit, no REX!)
        var testCode = Badger.Architectures.x86_16.Assembler.Assemble("test ax, ax");
        TestRunner.Assert(testCode.Length == 2 && testCode[0] == 0x85,
               "TEST ax, ax encoding (no REX prefix)");
        
        // Test IMUL register to register (16-bit, no REX!)
        var imulCode = Badger.Architectures.x86_16.Assembler.Assemble("imul ax, bx");
        TestRunner.Assert(imulCode.Length == 3 && imulCode[0] == 0x0F && imulCode[1] == 0xAF,
               "IMUL ax, bx encoding (no REX prefix)");
        
        // Test conditional jumps (16-bit near jumps)
        var jeCode = Badger.Architectures.x86_16.Assembler.Assemble("je target");
        TestRunner.Assert(jeCode.Length == 4 && jeCode[0] == 0x0F && jeCode[1] == 0x84,
               "JE rel16 encoding (4 bytes: 0F 84 + 2-byte offset)");
        
        var jneCode = Badger.Architectures.x86_16.Assembler.Assemble("jne target");
        TestRunner.Assert(jneCode.Length == 4 && jneCode[0] == 0x0F && jneCode[1] == 0x85,
               "JNE rel16 encoding");
        
        var jlCode = Badger.Architectures.x86_16.Assembler.Assemble("jl target");
        TestRunner.Assert(jlCode.Length == 4 && jlCode[0] == 0x0F && jlCode[1] == 0x8C,
               "JL rel16 encoding");
        
        var jgCode = Badger.Architectures.x86_16.Assembler.Assemble("jg target");
        TestRunner.Assert(jgCode.Length == 4 && jgCode[0] == 0x0F && jgCode[1] == 0x8F,
               "JG rel16 encoding");
        
        var jnzCode = Badger.Architectures.x86_16.Assembler.Assemble("jnz target");
        TestRunner.Assert(jnzCode.Length == 4 && jnzCode[0] == 0x0F && jnzCode[1] == 0x85,
               "JNZ rel16 encoding");
        
        // Test JMP (16-bit near jump)
        var jmpCode = Badger.Architectures.x86_16.Assembler.Assemble("jmp target");
        TestRunner.Assert(jmpCode.Length == 3 && jmpCode[0] == 0xE9,
               "JMP rel16 encoding (3 bytes: E9 + 2-byte offset)");
        
        // Test CALL (16-bit near call)
        var callCode = Badger.Architectures.x86_16.Assembler.Assemble("call target");
        TestRunner.Assert(callCode.Length == 3 && callCode[0] == 0xE8,
               "CALL rel16 encoding (3 bytes: E8 + 2-byte offset)");
        
        // Test SETcc instructions
        var seteCode = Badger.Architectures.x86_16.Assembler.Assemble("sete al");
        TestRunner.Assert(seteCode.Length == 3 && seteCode[0] == 0x0F && seteCode[1] == 0x94,
               "SETE al encoding");
        
        var setneCode = Badger.Architectures.x86_16.Assembler.Assemble("setne al");
        TestRunner.Assert(setneCode.Length == 3 && setneCode[0] == 0x0F && setneCode[1] == 0x95,
               "SETNE al encoding");
        
        var setlCode = Badger.Architectures.x86_16.Assembler.Assemble("setl al");
        TestRunner.Assert(setlCode.Length == 3 && setlCode[0] == 0x0F && setlCode[1] == 0x9C,
               "SETL al encoding");
        
        var setgCode = Badger.Architectures.x86_16.Assembler.Assemble("setg al");
        TestRunner.Assert(setgCode.Length == 3 && setgCode[0] == 0x0F && setgCode[1] == 0x9F,
               "SETG al encoding");
        
        // Test MOVZX (16-bit)
        var movzxCode = Badger.Architectures.x86_16.Assembler.Assemble("movzx ax, al");
        TestRunner.Assert(movzxCode.Length == 3 && movzxCode[0] == 0x0F && movzxCode[1] == 0xB6,
               "MOVZX ax, al encoding");
        
        // Test complete function prologue/epilogue (16-bit)
        var prologueCode = Badger.Architectures.x86_16.Assembler.Assemble(@"
            push bp
            mov bp, sp
            sub sp, 10
        ");
        TestRunner.Assert(prologueCode.Length == 6, "16-bit function prologue",
               $"Expected 6 bytes (push bp + mov bp,sp + sub sp,10), got {prologueCode.Length}");
        
        var epilogueCode = Badger.Architectures.x86_16.Assembler.Assemble(@"
            mov sp, bp
            pop bp
            ret
        ");
        TestRunner.Assert(epilogueCode.Length == 4, "16-bit function epilogue",
               $"Expected 4 bytes (mov sp,bp + pop bp + ret), got {epilogueCode.Length}");
        
        // Verify NO REX prefix (0x48) or 32-bit operand size override (0x66) in any 16-bit instruction
        var allInstructions = new[] { 
            movCode, addCode, subCode, xorCode, andCode, orCode, 
            cmpCode, testCode, imulCode, jeCode, jneCode, jlCode, jgCode
        };
        foreach (var code in allInstructions)
        {
            TestRunner.Assert(!code.Contains((byte)0x48), 
                   "No REX prefix in x86_16 instructions",
                   "16-bit real mode should not use REX prefix (0x48)");
        }
    }
}
