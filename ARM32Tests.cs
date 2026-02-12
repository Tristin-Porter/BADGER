using System;
using Badger.Architectures.ARM32;

namespace Badger.Tests;

public static class ARM32AssemblerTests
{
    public static void RunTests()
    {
        Console.WriteLine("=== ARM32 Assembler Tests ===\n");
        
        TestBxLr();
        TestNop();
        TestMovRegister();
        TestMovImmediate();
        TestAddRegister();
        TestAddImmediate();
        TestSubRegister();
        TestSubImmediate();
        TestMul();
        TestLogicalOps();
        TestCmp();
        TestBranches();
        TestPushPop();
        TestLoadStore();
        TestCompleteFunction();
        
        Console.WriteLine("\n=== All ARM32 Tests Passed! ===");
    }
    
    private static void TestBxLr()
    {
        Console.WriteLine("Testing BX LR...");
        var asm = "bx lr";
        var code = Assembler.Assemble(asm);
        
        // BX LR should be 0xE12FFF1E
        AssertEqual(code, new byte[] { 0x1E, 0xFF, 0x2F, 0xE1 });
        Console.WriteLine("  ✓ BX LR encoded correctly");
    }
    
    private static void TestNop()
    {
        Console.WriteLine("Testing NOP...");
        var asm = "nop";
        var code = Assembler.Assemble(asm);
        
        // NOP (MOV r0, r0) should be 0xE1A00000
        AssertEqual(code, new byte[] { 0x00, 0x00, 0xA0, 0xE1 });
        Console.WriteLine("  ✓ NOP encoded correctly");
    }
    
    private static void TestMovRegister()
    {
        Console.WriteLine("Testing MOV (register)...");
        var asm = "mov r1, r2";
        var code = Assembler.Assemble(asm);
        
        // MOV r1, r2 should be 0xE1A01002
        AssertEqual(code, new byte[] { 0x02, 0x10, 0xA0, 0xE1 });
        Console.WriteLine("  ✓ MOV register encoded correctly");
    }
    
    private static void TestMovImmediate()
    {
        Console.WriteLine("Testing MOV (immediate)...");
        var asm = "mov r0, #42";
        var code = Assembler.Assemble(asm);
        
        // MOV r0, #42 should be 0xE3A0002A
        AssertEqual(code, new byte[] { 0x2A, 0x00, 0xA0, 0xE3 });
        Console.WriteLine("  ✓ MOV immediate encoded correctly");
    }
    
    private static void TestAddRegister()
    {
        Console.WriteLine("Testing ADD (register)...");
        var asm = "add r0, r1, r2";
        var code = Assembler.Assemble(asm);
        
        // ADD r0, r1, r2 should be 0xE0810002
        AssertEqual(code, new byte[] { 0x02, 0x00, 0x81, 0xE0 });
        Console.WriteLine("  ✓ ADD register encoded correctly");
    }
    
    private static void TestAddImmediate()
    {
        Console.WriteLine("Testing ADD (immediate)...");
        var asm = "add r0, r1, #8";
        var code = Assembler.Assemble(asm);
        
        // ADD r0, r1, #8 should be 0xE2810008
        AssertEqual(code, new byte[] { 0x08, 0x00, 0x81, 0xE2 });
        Console.WriteLine("  ✓ ADD immediate encoded correctly");
    }
    
    private static void TestSubRegister()
    {
        Console.WriteLine("Testing SUB (register)...");
        var asm = "sub r0, r1, r2";
        var code = Assembler.Assemble(asm);
        
        // SUB r0, r1, r2 should be 0xE0410002
        AssertEqual(code, new byte[] { 0x02, 0x00, 0x41, 0xE0 });
        Console.WriteLine("  ✓ SUB register encoded correctly");
    }
    
    private static void TestSubImmediate()
    {
        Console.WriteLine("Testing SUB (immediate)...");
        var asm = "sub r0, r1, #4";
        var code = Assembler.Assemble(asm);
        
        // SUB r0, r1, #4 should be 0xE2410004
        AssertEqual(code, new byte[] { 0x04, 0x00, 0x41, 0xE2 });
        Console.WriteLine("  ✓ SUB immediate encoded correctly");
    }
    
    private static void TestMul()
    {
        Console.WriteLine("Testing MUL...");
        var asm = "mul r0, r1, r2";
        var code = Assembler.Assemble(asm);
        
        // MUL r0, r1, r2 should be 0xE0000291
        AssertEqual(code, new byte[] { 0x91, 0x02, 0x00, 0xE0 });
        Console.WriteLine("  ✓ MUL encoded correctly");
    }
    
    private static void TestLogicalOps()
    {
        Console.WriteLine("Testing logical operations...");
        
        // AND r0, r1, r2
        var asm = "and r0, r1, r2";
        var code = Assembler.Assemble(asm);
        AssertEqual(code, new byte[] { 0x02, 0x00, 0x01, 0xE0 });
        Console.WriteLine("  ✓ AND encoded correctly");
        
        // ORR r0, r1, r2
        asm = "orr r0, r1, r2";
        code = Assembler.Assemble(asm);
        AssertEqual(code, new byte[] { 0x02, 0x00, 0x81, 0xE1 });
        Console.WriteLine("  ✓ ORR encoded correctly");
        
        // EOR r0, r1, r2
        asm = "eor r0, r1, r2";
        code = Assembler.Assemble(asm);
        AssertEqual(code, new byte[] { 0x02, 0x00, 0x21, 0xE0 });
        Console.WriteLine("  ✓ EOR encoded correctly");
    }
    
    private static void TestCmp()
    {
        Console.WriteLine("Testing CMP...");
        
        // CMP r0, #5
        var asm = "cmp r0, #5";
        var code = Assembler.Assemble(asm);
        AssertEqual(code, new byte[] { 0x05, 0x00, 0x50, 0xE3 });
        Console.WriteLine("  ✓ CMP immediate encoded correctly");
        
        // CMP r0, r1
        asm = "cmp r0, r1";
        code = Assembler.Assemble(asm);
        AssertEqual(code, new byte[] { 0x01, 0x00, 0x50, 0xE1 });
        Console.WriteLine("  ✓ CMP register encoded correctly");
    }
    
    private static void TestBranches()
    {
        Console.WriteLine("Testing branch instructions...");
        
        // Test unconditional branch
        var asm = @"
            b skip
            nop
        skip:
            nop
        ";
        var code = Assembler.Assemble(asm);
        // B skip (forward by 1 instruction) should have offset = 0
        // Offset calculation: target (8) - (current (0) + 8) = 0
        AssertEqual(new byte[] { code[0], code[1], code[2], code[3] }, 
                    new byte[] { 0x00, 0x00, 0x00, 0xEA });
        Console.WriteLine("  ✓ B (unconditional) encoded correctly");
        
        // Test conditional branches
        asm = @"
            beq target
            nop
        target:
            nop
        ";
        code = Assembler.Assemble(asm);
        // BEQ with cond=0
        AssertEqual(new byte[] { code[0], code[1], code[2], code[3] }, 
                    new byte[] { 0x00, 0x00, 0x00, 0x0A });
        Console.WriteLine("  ✓ BEQ encoded correctly");
        
        // Test BL
        asm = @"
            bl func
            nop
        func:
            nop
        ";
        code = Assembler.Assemble(asm);
        // BL should have L=1
        AssertEqual(new byte[] { code[0], code[1], code[2], code[3] }, 
                    new byte[] { 0x00, 0x00, 0x00, 0xEB });
        Console.WriteLine("  ✓ BL encoded correctly");
    }
    
    private static void TestPushPop()
    {
        Console.WriteLine("Testing PUSH/POP...");
        
        // PUSH {r0, r1}
        var asm = "push {r0, r1}";
        var code = Assembler.Assemble(asm);
        // STMDB sp!, {r0, r1} - reglist = 0x0003
        AssertEqual(code, new byte[] { 0x03, 0x00, 0x2D, 0xE9 });
        Console.WriteLine("  ✓ PUSH encoded correctly");
        
        // POP {r0, r1}
        asm = "pop {r0, r1}";
        code = Assembler.Assemble(asm);
        // LDMIA sp!, {r0, r1}
        AssertEqual(code, new byte[] { 0x03, 0x00, 0xBD, 0xE8 });
        Console.WriteLine("  ✓ POP encoded correctly");
        
        // PUSH {r11, lr}
        asm = "push {r11, lr}";
        code = Assembler.Assemble(asm);
        // reglist = 0x4800 (bits 11 and 14)
        AssertEqual(code, new byte[] { 0x00, 0x48, 0x2D, 0xE9 });
        Console.WriteLine("  ✓ PUSH {r11, lr} encoded correctly");
    }
    
    private static void TestLoadStore()
    {
        Console.WriteLine("Testing LDR/STR...");
        
        // LDR r0, [r1, #4]
        var asm = "ldr r0, [r1, #4]";
        var code = Assembler.Assemble(asm);
        AssertEqual(code, new byte[] { 0x04, 0x00, 0x91, 0xE5 });
        Console.WriteLine("  ✓ LDR with positive offset encoded correctly");
        
        // LDR r0, [r1, #-4]
        asm = "ldr r0, [r1, #-4]";
        code = Assembler.Assemble(asm);
        AssertEqual(code, new byte[] { 0x04, 0x00, 0x11, 0xE5 });
        Console.WriteLine("  ✓ LDR with negative offset encoded correctly");
        
        // STR r0, [r1, #8]
        asm = "str r0, [r1, #8]";
        code = Assembler.Assemble(asm);
        AssertEqual(code, new byte[] { 0x08, 0x00, 0x81, 0xE5 });
        Console.WriteLine("  ✓ STR encoded correctly");
    }
    
    private static void TestCompleteFunction()
    {
        Console.WriteLine("Testing complete function...");
        
        var asm = @"
        main:
            push {r11, lr}
            mov r11, sp
            sub sp, sp, #16
            mov r0, #5
            add r0, r0, #3
            mov sp, r11
            pop {r11, pc}
        ";
        
        var code = Assembler.Assemble(asm);
        
        // Verify we got 7 instructions (7 * 4 = 28 bytes)
        if (code.Length != 28)
            throw new Exception($"Expected 28 bytes, got {code.Length}");
        
        Console.WriteLine($"  ✓ Complete function assembled ({code.Length} bytes)");
        
        // Verify some key instructions
        // First instruction: PUSH {r11, lr}
        AssertEqual(new byte[] { code[0], code[1], code[2], code[3] }, 
                    new byte[] { 0x00, 0x48, 0x2D, 0xE9 });
        
        // Last instruction: POP {r11, pc}
        AssertEqual(new byte[] { code[24], code[25], code[26], code[27] }, 
                    new byte[] { 0x00, 0x88, 0xBD, 0xE8 });
        
        Console.WriteLine("  ✓ Function prologue/epilogue verified");
    }
    
    private static void AssertEqual(byte[] actual, byte[] expected)
    {
        if (actual.Length != expected.Length)
        {
            throw new Exception($"Length mismatch: expected {expected.Length}, got {actual.Length}");
        }
        
        for (int i = 0; i < expected.Length; i++)
        {
            if (actual[i] != expected[i])
            {
                Console.WriteLine($"Byte mismatch at position {i}:");
                Console.WriteLine($"  Expected: {BitConverter.ToString(expected)}");
                Console.WriteLine($"  Actual:   {BitConverter.ToString(actual)}");
                throw new Exception($"Byte mismatch at position {i}: expected 0x{expected[i]:X2}, got 0x{actual[i]:X2}");
            }
        }
    }
}
