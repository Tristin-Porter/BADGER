using System;

namespace Badger.Testing;

/// <summary>
/// Tests for ARM32 assembler and instruction encoding.
/// Verifies correct assembly parsing and machine code generation for ARM32 architecture.
/// </summary>
public static class ARM32Tests
{
    public static void RunTests()
    {
        TestARM32Assembler();
        TestARM32Instructions();
    }
    
    private static void TestARM32Assembler()
    {
        Console.WriteLine("\n--- ARM32 Assembler Tests ---");
        
        try
        {
            // Test basic instruction
            var asm = "bx lr";
            var code = Badger.Architectures.ARM32.Assembler.Assemble(asm);
            TestRunner.Assert(code.Length == 4, "ARM32: BX LR size", $"Expected 4 bytes, got {code.Length}");
            TestRunner.AssertArrayEqual(code, new byte[] { 0x1E, 0xFF, 0x2F, 0xE1 }, "ARM32: BX LR encoding");
            
            // Test NOP
            asm = "nop";
            code = Badger.Architectures.ARM32.Assembler.Assemble(asm);
            TestRunner.AssertArrayEqual(code, new byte[] { 0x00, 0x00, 0xA0, 0xE1 }, "ARM32: NOP encoding");
            
            // Test MOV register
            asm = "mov r1, r2";
            code = Badger.Architectures.ARM32.Assembler.Assemble(asm);
            TestRunner.AssertArrayEqual(code, new byte[] { 0x02, 0x10, 0xA0, 0xE1 }, "ARM32: MOV register");
            
            // Test MOV immediate
            asm = "mov r0, #42";
            code = Badger.Architectures.ARM32.Assembler.Assemble(asm);
            TestRunner.AssertArrayEqual(code, new byte[] { 0x2A, 0x00, 0xA0, 0xE3 }, "ARM32: MOV immediate");
            
            Console.WriteLine("  ✓ Basic ARM32 instructions");
        }
        catch (Exception ex)
        {
            TestRunner.Assert(false, "ARM32: Basic assembler", ex.Message);
        }
    }
    
    private static void TestARM32Instructions()
    {
        Console.WriteLine("\n--- ARM32 Instruction Encoding Tests ---");
        
        // Arithmetic instructions
        try
        {
            var asm = "add r0, r1, r2";
            var code = Badger.Architectures.ARM32.Assembler.Assemble(asm);
            TestRunner.AssertArrayEqual(code, new byte[] { 0x02, 0x00, 0x81, 0xE0 }, "ARM32: ADD register");
            
            asm = "add r0, r1, #8";
            code = Badger.Architectures.ARM32.Assembler.Assemble(asm);
            TestRunner.AssertArrayEqual(code, new byte[] { 0x08, 0x00, 0x81, 0xE2 }, "ARM32: ADD immediate");
            
            asm = "sub r0, r1, r2";
            code = Badger.Architectures.ARM32.Assembler.Assemble(asm);
            TestRunner.AssertArrayEqual(code, new byte[] { 0x02, 0x00, 0x41, 0xE0 }, "ARM32: SUB register");
            
            asm = "sub r0, r1, #4";
            code = Badger.Architectures.ARM32.Assembler.Assemble(asm);
            TestRunner.AssertArrayEqual(code, new byte[] { 0x04, 0x00, 0x41, 0xE2 }, "ARM32: SUB immediate");
            
            asm = "mul r0, r1, r2";
            code = Badger.Architectures.ARM32.Assembler.Assemble(asm);
            TestRunner.AssertArrayEqual(code, new byte[] { 0x91, 0x02, 0x00, 0xE0 }, "ARM32: MUL");
            
            Console.WriteLine("  ✓ ARM32 arithmetic instructions");
        }
        catch (Exception ex)
        {
            TestRunner.Assert(false, "ARM32: Arithmetic", ex.Message);
        }
        
        // Logical instructions
        try
        {
            var asm = "and r0, r1, r2";
            var code = Badger.Architectures.ARM32.Assembler.Assemble(asm);
            TestRunner.AssertArrayEqual(code, new byte[] { 0x02, 0x00, 0x01, 0xE0 }, "ARM32: AND");
            
            asm = "orr r0, r1, r2";
            code = Badger.Architectures.ARM32.Assembler.Assemble(asm);
            TestRunner.AssertArrayEqual(code, new byte[] { 0x02, 0x00, 0x81, 0xE1 }, "ARM32: ORR");
            
            asm = "eor r0, r1, r2";
            code = Badger.Architectures.ARM32.Assembler.Assemble(asm);
            TestRunner.AssertArrayEqual(code, new byte[] { 0x02, 0x00, 0x21, 0xE0 }, "ARM32: EOR");
            
            Console.WriteLine("  ✓ ARM32 logical instructions");
        }
        catch (Exception ex)
        {
            TestRunner.Assert(false, "ARM32: Logical", ex.Message);
        }
        
        // Compare instruction
        try
        {
            var asm = "cmp r0, #5";
            var code = Badger.Architectures.ARM32.Assembler.Assemble(asm);
            TestRunner.AssertArrayEqual(code, new byte[] { 0x05, 0x00, 0x50, 0xE3 }, "ARM32: CMP immediate");
            
            asm = "cmp r0, r1";
            code = Badger.Architectures.ARM32.Assembler.Assemble(asm);
            TestRunner.AssertArrayEqual(code, new byte[] { 0x01, 0x00, 0x50, 0xE1 }, "ARM32: CMP register");
            
            Console.WriteLine("  ✓ ARM32 compare instruction");
        }
        catch (Exception ex)
        {
            TestRunner.Assert(false, "ARM32: Compare", ex.Message);
        }
        
        // Branch instructions
        try
        {
            var asm = @"
                b skip
                nop
            skip:
                nop
            ";
            var code = Badger.Architectures.ARM32.Assembler.Assemble(asm);
            // First instruction should be B with offset 0 (forward by 1 instruction)
            TestRunner.AssertArrayEqual(new byte[] { code[0], code[1], code[2], code[3] }, 
                       new byte[] { 0x00, 0x00, 0x00, 0xEA }, "ARM32: B unconditional");
            
            asm = @"
                beq target
                nop
            target:
                nop
            ";
            code = Badger.Architectures.ARM32.Assembler.Assemble(asm);
            TestRunner.AssertArrayEqual(new byte[] { code[0], code[1], code[2], code[3] }, 
                       new byte[] { 0x00, 0x00, 0x00, 0x0A }, "ARM32: BEQ");
            
            asm = @"
                bne target
                nop
            target:
                nop
            ";
            code = Badger.Architectures.ARM32.Assembler.Assemble(asm);
            TestRunner.AssertArrayEqual(new byte[] { code[0], code[1], code[2], code[3] }, 
                       new byte[] { 0x00, 0x00, 0x00, 0x1A }, "ARM32: BNE");
            
            asm = @"
                bl func
                nop
            func:
                nop
            ";
            code = Badger.Architectures.ARM32.Assembler.Assemble(asm);
            TestRunner.AssertArrayEqual(new byte[] { code[0], code[1], code[2], code[3] }, 
                       new byte[] { 0x00, 0x00, 0x00, 0xEB }, "ARM32: BL");
            
            Console.WriteLine("  ✓ ARM32 branch instructions");
        }
        catch (Exception ex)
        {
            TestRunner.Assert(false, "ARM32: Branches", ex.Message);
        }
        
        // Stack operations
        try
        {
            var asm = "push {r0, r1}";
            var code = Badger.Architectures.ARM32.Assembler.Assemble(asm);
            TestRunner.AssertArrayEqual(code, new byte[] { 0x03, 0x00, 0x2D, 0xE9 }, "ARM32: PUSH {r0, r1}");
            
            asm = "pop {r0, r1}";
            code = Badger.Architectures.ARM32.Assembler.Assemble(asm);
            TestRunner.AssertArrayEqual(code, new byte[] { 0x03, 0x00, 0xBD, 0xE8 }, "ARM32: POP {r0, r1}");
            
            asm = "push {r11, lr}";
            code = Badger.Architectures.ARM32.Assembler.Assemble(asm);
            TestRunner.AssertArrayEqual(code, new byte[] { 0x00, 0x48, 0x2D, 0xE9 }, "ARM32: PUSH {r11, lr}");
            
            asm = "pop {r11, pc}";
            code = Badger.Architectures.ARM32.Assembler.Assemble(asm);
            TestRunner.AssertArrayEqual(code, new byte[] { 0x00, 0x88, 0xBD, 0xE8 }, "ARM32: POP {r11, pc}");
            
            Console.WriteLine("  ✓ ARM32 stack operations");
        }
        catch (Exception ex)
        {
            TestRunner.Assert(false, "ARM32: Stack ops", ex.Message);
        }
        
        // Load/Store instructions
        try
        {
            var asm = "ldr r0, [r1, #4]";
            var code = Badger.Architectures.ARM32.Assembler.Assemble(asm);
            TestRunner.AssertArrayEqual(code, new byte[] { 0x04, 0x00, 0x91, 0xE5 }, "ARM32: LDR positive offset");
            
            asm = "ldr r0, [r1, #-4]";
            code = Badger.Architectures.ARM32.Assembler.Assemble(asm);
            TestRunner.AssertArrayEqual(code, new byte[] { 0x04, 0x00, 0x11, 0xE5 }, "ARM32: LDR negative offset");
            
            asm = "str r0, [r1, #8]";
            code = Badger.Architectures.ARM32.Assembler.Assemble(asm);
            TestRunner.AssertArrayEqual(code, new byte[] { 0x08, 0x00, 0x81, 0xE5 }, "ARM32: STR");
            
            Console.WriteLine("  ✓ ARM32 load/store instructions");
        }
        catch (Exception ex)
        {
            TestRunner.Assert(false, "ARM32: Load/Store", ex.Message);
        }
        
        // Complete function test
        try
        {
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
            var code = Badger.Architectures.ARM32.Assembler.Assemble(asm);
            TestRunner.Assert(code.Length == 28, "ARM32: Function length", $"Expected 28 bytes, got {code.Length}");
            
            // Verify prologue: PUSH {r11, lr}
            TestRunner.AssertArrayEqual(new byte[] { code[0], code[1], code[2], code[3] }, 
                       new byte[] { 0x00, 0x48, 0x2D, 0xE9 }, "ARM32: Function prologue");
            
            // Verify epilogue: POP {r11, pc}
            TestRunner.AssertArrayEqual(new byte[] { code[24], code[25], code[26], code[27] }, 
                       new byte[] { 0x00, 0x88, 0xBD, 0xE8 }, "ARM32: Function epilogue");
            
            Console.WriteLine("  ✓ ARM32 complete function");
        }
        catch (Exception ex)
        {
            TestRunner.Assert(false, "ARM32: Complete function", ex.Message);
        }
    }
}
