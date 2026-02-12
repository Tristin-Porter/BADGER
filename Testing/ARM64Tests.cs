using System;

namespace Badger.Testing;

/// <summary>
/// Tests for ARM64 assembler and instruction encoding.
/// Verifies correct assembly parsing and machine code generation for ARM64 (AArch64) architecture.
/// </summary>
public static class ARM64Tests
{
    public static void RunTests()
    {
        TestARM64Assembler();
        TestARM64Instructions();
    }
    
    private static void TestARM64Assembler()
    {
        Console.WriteLine("\n--- ARM64 Assembler Tests ---");
        
        // Test basic assembly
        var simpleAsm = "ret";
        var result = Badger.Architectures.ARM64.Assembler.Assemble(simpleAsm);
        TestRunner.Assert(result.Length == 4, "Basic ARM64 assembly produces 4 bytes");
        
        // Test assembly with labels
        var labelAsm = @"
            main:
                nop
                ret
        ";
        var resultWithLabel = Badger.Architectures.ARM64.Assembler.Assemble(labelAsm);
        TestRunner.Assert(resultWithLabel.Length == 8, "Assembly with labels produces correct size (2 instructions = 8 bytes)");
        
        // Test multi-instruction assembly
        var multiAsm = @"
            mov x0, #42
            mov x1, #100
            add x2, x0, x1
            ret
        ";
        var multiResult = Badger.Architectures.ARM64.Assembler.Assemble(multiAsm);
        TestRunner.Assert(multiResult.Length == 16, "Multi-instruction assembly (4 instructions = 16 bytes)");
    }
    
    private static void TestARM64Instructions()
    {
        Console.WriteLine("\n--- ARM64 Instruction Encoding Tests ---");
        
        // Test RET instruction
        var retCode = Badger.Architectures.ARM64.Assembler.Assemble("ret");
        TestRunner.AssertArrayEqual(new byte[] { 0xC0, 0x03, 0x5F, 0xD6 }, retCode, "RET instruction encoding");
        
        // Test NOP instruction
        var nopCode = Badger.Architectures.ARM64.Assembler.Assemble("nop");
        TestRunner.AssertArrayEqual(new byte[] { 0x1F, 0x20, 0x03, 0xD5 }, nopCode, "NOP instruction encoding");
        
        // Test MOV immediate (MOVZ)
        var movImm = Badger.Architectures.ARM64.Assembler.Assemble("mov x0, #42");
        TestRunner.Assert(movImm.Length == 4, "MOV x0, #42 is 4 bytes");
        TestRunner.Assert(movImm[3] == 0xD2, "MOV x0, #42 uses MOVZ encoding (top byte 0xD2)");
        
        // Test MOV register to register (ORR with XZR)
        var movReg = Badger.Architectures.ARM64.Assembler.Assemble("mov x1, x2");
        TestRunner.Assert(movReg.Length == 4, "MOV x1, x2 is 4 bytes");
        TestRunner.Assert(movReg[3] == 0xAA, "MOV x1, x2 uses ORR encoding (top byte 0xAA)");
        
        // Test ADD with immediate
        var addImm = Badger.Architectures.ARM64.Assembler.Assemble("add x0, x1, #10");
        TestRunner.Assert(addImm.Length == 4, "ADD x0, x1, #10 is 4 bytes");
        TestRunner.Assert((addImm[3] & 0xFF) == 0x91, "ADD immediate uses correct opcode");
        
        // Test ADD with register
        var addReg = Badger.Architectures.ARM64.Assembler.Assemble("add x0, x1, x2");
        TestRunner.Assert(addReg.Length == 4, "ADD x0, x1, x2 is 4 bytes");
        TestRunner.Assert((addReg[3] & 0xFF) == 0x8B, "ADD register uses correct opcode");
        
        // Test SUB with immediate
        var subImm = Badger.Architectures.ARM64.Assembler.Assemble("sub sp, sp, #16");
        TestRunner.Assert(subImm.Length == 4, "SUB sp, sp, #16 is 4 bytes");
        TestRunner.Assert((subImm[3] & 0xFF) == 0xD1, "SUB immediate uses correct opcode");
        
        // Test SUB with register
        var subReg = Badger.Architectures.ARM64.Assembler.Assemble("sub x0, x1, x2");
        TestRunner.Assert(subReg.Length == 4, "SUB x0, x1, x2 is 4 bytes");
        TestRunner.Assert((subReg[3] & 0xFF) == 0xCB, "SUB register uses correct opcode");
        
        // Test MUL instruction
        var mul = Badger.Architectures.ARM64.Assembler.Assemble("mul x0, x1, x2");
        TestRunner.Assert(mul.Length == 4, "MUL x0, x1, x2 is 4 bytes");
        TestRunner.Assert((mul[3] & 0xFF) == 0x9B, "MUL uses correct opcode");
        
        // Test AND instruction
        var and = Badger.Architectures.ARM64.Assembler.Assemble("and x0, x1, x2");
        TestRunner.Assert(and.Length == 4, "AND x0, x1, x2 is 4 bytes");
        TestRunner.Assert((and[3] & 0xFF) == 0x8A, "AND uses correct opcode");
        
        // Test ORR instruction
        var orr = Badger.Architectures.ARM64.Assembler.Assemble("orr x0, x1, x2");
        TestRunner.Assert(orr.Length == 4, "ORR x0, x1, x2 is 4 bytes");
        TestRunner.Assert((orr[3] & 0xFF) == 0xAA, "ORR uses correct opcode");
        
        // Test EOR instruction
        var eor = Badger.Architectures.ARM64.Assembler.Assemble("eor x0, x1, x2");
        TestRunner.Assert(eor.Length == 4, "EOR x0, x1, x2 is 4 bytes");
        TestRunner.Assert((eor[3] & 0xFF) == 0xCA, "EOR uses correct opcode");
        
        // Test CMP with immediate
        var cmpImm = Badger.Architectures.ARM64.Assembler.Assemble("cmp x0, #10");
        TestRunner.Assert(cmpImm.Length == 4, "CMP x0, #10 is 4 bytes");
        TestRunner.Assert((cmpImm[3] & 0xFF) == 0xF1, "CMP immediate uses SUBS opcode");
        
        // Test CMP with register
        var cmpReg = Badger.Architectures.ARM64.Assembler.Assemble("cmp x0, x1");
        TestRunner.Assert(cmpReg.Length == 4, "CMP x0, x1 is 4 bytes");
        TestRunner.Assert((cmpReg[3] & 0xFF) == 0xEB, "CMP register uses SUBS opcode");
        
        // Test STP (store pair)
        var stp = Badger.Architectures.ARM64.Assembler.Assemble("stp x29, x30, [sp, #-16]!");
        TestRunner.Assert(stp.Length == 4, "STP x29, x30, [sp, #-16]! is 4 bytes");
        TestRunner.Assert((stp[3] & 0xFE) == 0xA8, "STP uses correct opcode");
        
        // Test LDP (load pair)
        var ldp = Badger.Architectures.ARM64.Assembler.Assemble("ldp x29, x30, [sp], #16");
        TestRunner.Assert(ldp.Length == 4, "LDP x29, x30, [sp], #16 is 4 bytes");
        TestRunner.Assert((ldp[3] & 0xFE) == 0xA8, "LDP uses correct opcode");
        
        // Test LDR (load register)
        var ldr = Badger.Architectures.ARM64.Assembler.Assemble("ldr w0, [sp], #4");
        TestRunner.Assert(ldr.Length == 4, "LDR w0, [sp], #4 is 4 bytes");
        TestRunner.Assert((ldr[3] & 0xFF) == 0xB8, "LDR uses correct opcode");
        
        // Test STR (store register)
        var str = Badger.Architectures.ARM64.Assembler.Assemble("str w0, [sp, #-4]!");
        TestRunner.Assert(str.Length == 4, "STR w0, [sp, #-4]! is 4 bytes");
        TestRunner.Assert((str[3] & 0xFF) == 0xB8, "STR uses correct opcode");
        
        // Test 32-bit operations (w registers)
        var add32 = Badger.Architectures.ARM64.Assembler.Assemble("add w0, w1, w2");
        TestRunner.Assert(add32.Length == 4, "ADD w0, w1, w2 is 4 bytes");
        TestRunner.Assert((add32[3] & 0x7F) == 0x0B, "32-bit ADD has sf=0");
        
        var mov32 = Badger.Architectures.ARM64.Assembler.Assemble("mov w0, #42");
        TestRunner.Assert(mov32.Length == 4, "MOV w0, #42 is 4 bytes");
        TestRunner.Assert((mov32[3] & 0x7F) == 0x52, "32-bit MOV has sf=0");
        
        // Test branch with label
        var branchAsm = @"
            start:
                nop
                b start
        ";
        var branchCode = Badger.Architectures.ARM64.Assembler.Assemble(branchAsm);
        TestRunner.Assert(branchCode.Length == 8, "Branch with label is 2 instructions = 8 bytes");
        TestRunner.Assert((branchCode[7] & 0xFC) == 0x14, "B uses correct opcode (0b000101)");
        
        // Test conditional branches
        var beqAsm = @"
            loop:
                nop
                b.eq loop
        ";
        var beqCode = Badger.Architectures.ARM64.Assembler.Assemble(beqAsm);
        TestRunner.Assert(beqCode.Length == 8, "B.EQ with label is 2 instructions = 8 bytes");
        TestRunner.Assert((beqCode[7] & 0xFE) == 0x54, "B.EQ uses correct opcode");
        TestRunner.Assert((beqCode[4] & 0x0F) == 0x00, "B.EQ uses condition code 0000 (EQ)");
        
        var bneAsm = @"
            loop:
                nop
                b.ne loop
        ";
        var bneCode = Badger.Architectures.ARM64.Assembler.Assemble(bneAsm);
        TestRunner.Assert(bneCode.Length == 8, "B.NE with label is 2 instructions = 8 bytes");
        TestRunner.Assert((bneCode[4] & 0x0F) == 0x01, "B.NE uses condition code 0001 (NE)");
        
        var bltAsm = @"
            loop:
                nop
                b.lt loop
        ";
        var bltCode = Badger.Architectures.ARM64.Assembler.Assemble(bltAsm);
        TestRunner.Assert(bltCode.Length == 8, "B.LT with label is 2 instructions = 8 bytes");
        TestRunner.Assert((bltCode[4] & 0x0F) == 0x0B, "B.LT uses condition code 1011 (LT)");
        
        var bgtAsm = @"
            loop:
                nop
                b.gt loop
        ";
        var bgtCode = Badger.Architectures.ARM64.Assembler.Assemble(bgtAsm);
        TestRunner.Assert(bgtCode.Length == 8, "B.GT with label is 2 instructions = 8 bytes");
        TestRunner.Assert((bgtCode[4] & 0x0F) == 0x0C, "B.GT uses condition code 1100 (GT)");
        
        // Test BL (branch with link)
        var blAsm = @"
            main:
                nop
                bl main
        ";
        var blCode = Badger.Architectures.ARM64.Assembler.Assemble(blAsm);
        TestRunner.Assert(blCode.Length == 8, "BL with label is 2 instructions = 8 bytes");
        TestRunner.Assert((blCode[7] & 0xFC) == 0x94, "BL uses correct opcode (0b100101)");
        
        // Test complete function prologue/epilogue (ARM64)
        var prologueCode = Badger.Architectures.ARM64.Assembler.Assemble(@"
            stp x29, x30, [sp, #-16]!
            mov x29, sp
            sub sp, sp, #32
        ");
        TestRunner.Assert(prologueCode.Length == 12, "ARM64 function prologue (3 instructions = 12 bytes)");
        
        var epilogueCode = Badger.Architectures.ARM64.Assembler.Assemble(@"
            mov sp, x29
            ldp x29, x30, [sp], #16
            ret
        ");
        TestRunner.Assert(epilogueCode.Length == 12, "ARM64 function epilogue (3 instructions = 12 bytes)");
        
        // Verify all ARM64 instructions are exactly 4 bytes (fixed width)
        var allInstructions = new[] { 
            retCode, nopCode, movImm, movReg, addImm, addReg, 
            subImm, subReg, mul, and, orr, eor, cmpImm, cmpReg,
            stp, ldp, ldr, str
        };
        foreach (var code in allInstructions)
        {
            TestRunner.Assert(code.Length == 4, "All ARM64 instructions are exactly 4 bytes");
        }
        
        // Verify little-endian encoding
        // RET = 0xD65F03C0, so in little-endian: C0 03 5F D6
        TestRunner.Assert(retCode[0] == 0xC0 && retCode[1] == 0x03 && 
               retCode[2] == 0x5F && retCode[3] == 0xD6,
               "ARM64 instructions use little-endian byte order");
    }
}
