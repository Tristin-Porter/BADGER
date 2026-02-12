using System;

namespace Badger.Testing;

/// <summary>
/// Tests for container emission (Native and PE formats).
/// Verifies correct binary container generation from machine code.
/// </summary>
public static class ContainerTests
{
    public static void RunTests()
    {
        TestNativeContainer();
        TestPEContainer();
        TestLabelResolution();
    }
    
    private static void TestNativeContainer()
    {
        Console.WriteLine("\n--- Native Container Tests ---");
        
        // Test that Native container is pass-through
        byte[] machineCode = new byte[] { 0xC3 }; // RET
        var binary = Badger.Containers.Native.Emit(machineCode);
        
        TestRunner.AssertArrayEqual(machineCode, binary, "Native container is pass-through");
        
        // Test with larger code
        byte[] largerCode = new byte[] { 0x55, 0x48, 0x89, 0xE5, 0x5D, 0xC3 };
        var largerBinary = Badger.Containers.Native.Emit(largerCode);
        
        TestRunner.AssertArrayEqual(largerCode, largerBinary, "Native container preserves larger code");
    }
    
    private static void TestPEContainer()
    {
        Console.WriteLine("\n--- PE Container Tests ---");
        
        // Test basic PE structure
        byte[] machineCode = new byte[] { 0xC3 }; // RET
        var pe = Badger.Containers.PE.Emit(machineCode);
        
        // Check DOS header magic
        TestRunner.Assert(pe.Length >= 2 && pe[0] == 0x4D && pe[1] == 0x5A,
               "PE has valid DOS magic 'MZ'");
        
        // Check PE signature offset
        TestRunner.Assert(pe.Length >= 64, "PE has complete DOS header");
        
        int peOffset = pe[60] | (pe[61] << 8) | (pe[62] << 16) | (pe[63] << 24);
        TestRunner.Assert(peOffset < pe.Length, "PE signature offset is valid");
        
        // Check PE signature
        if (peOffset + 4 <= pe.Length)
        {
            TestRunner.Assert(pe[peOffset] == 0x50 && pe[peOffset + 1] == 0x45 &&
                   pe[peOffset + 2] == 0x00 && pe[peOffset + 3] == 0x00,
                   "PE has valid signature 'PE\\0\\0'");
        }
        else
        {
            TestRunner.Assert(false, "PE signature accessible");
        }
        
        // Check COFF header machine type (x86-64)
        if (peOffset + 6 <= pe.Length)
        {
            int machine = pe[peOffset + 4] | (pe[peOffset + 5] << 8);
            TestRunner.Assert(machine == 0x8664, "PE has correct machine type (x86-64)",
                   $"Expected 0x8664, got 0x{machine:X4}");
        }
        else
        {
            TestRunner.Assert(false, "PE COFF header accessible");
        }
        
        // Check that PE is larger than just machine code (has headers)
        TestRunner.Assert(pe.Length > machineCode.Length,
               "PE binary includes headers and is larger than raw code");
        
        // Check alignment
        TestRunner.Assert(pe.Length % 512 == 0, "PE binary is aligned to file alignment (512 bytes)");
    }
    
    private static void TestLabelResolution()
    {
        Console.WriteLine("\n--- Label Resolution Tests ---");
        
        // Test that labels are collected in first pass
        string asmWithLabels = @"
start:
    nop
    nop
middle:
    nop
function_end:
    ret
";
        
        try
        {
            var code = Badger.Architectures.x86_64.Assembler.Assemble(asmWithLabels);
            TestRunner.Assert(code.Length == 4, "Label resolution produces correct code size", 
                   $"Expected 4 bytes (3 NOPs + 1 RET), got {code.Length}");
        }
        catch (Exception ex)
        {
            TestRunner.Assert(false, "Label resolution works", ex.Message);
        }
        
        // Test forward and backward label references
        string jumpAsm = @"
main:
    jmp jump_target
    nop
jump_target:
    ret
";
        
        try
        {
            var code = Badger.Architectures.x86_64.Assembler.Assemble(jumpAsm);
            TestRunner.Assert(code.Length > 0, "Forward jump compiles");
        }
        catch (Exception ex)
        {
            TestRunner.Assert(false, "Forward jump compiles", ex.Message);
        }
        
        // Test x86_32 label resolution with forward jump
        string x86_32JumpAsm = @"
start:
    je target
    nop
target:
    ret
";
        
        try
        {
            var code32 = Badger.Architectures.x86_32.Assembler.Assemble(x86_32JumpAsm);
            TestRunner.Assert(code32.Length > 0, "x86_32: Forward conditional jump compiles");
            
            // Verify the jump offset is correct
            // JE = 0x0F 0x84 + 4-byte offset
            // Offset should be: target address - (current address + 6)
            // target is at address 7 (6 bytes for JE + 1 for NOP)
            // current is at 0, so offset = 7 - 6 = 1
            if (code32.Length >= 6 && code32[0] == 0x0F && code32[1] == 0x84)
            {
                int offset = code32[2] | (code32[3] << 8) | (code32[4] << 16) | (code32[5] << 24);
                TestRunner.Assert(offset == 1, "x86_32: Jump offset is correctly calculated",
                       $"Expected offset 1, got {offset}");
            }
        }
        catch (Exception ex)
        {
            TestRunner.Assert(false, "x86_32: Forward jump compiles", ex.Message);
        }
        
        // Test x86_32 backward jump
        string x86_32BackJumpAsm = @"
loop_start:
    nop
    jne loop_start
";
        
        try
        {
            var code32 = Badger.Architectures.x86_32.Assembler.Assemble(x86_32BackJumpAsm);
            TestRunner.Assert(code32.Length > 0, "x86_32: Backward conditional jump compiles");
            
            // Verify backward jump offset
            // loop_start is at 0, NOP is 1 byte, JNE starts at 1
            // JNE = 0x0F 0x85 + 4-byte offset (6 bytes total)
            // Offset = 0 - (1 + 6) = -7
            if (code32.Length >= 7 && code32[1] == 0x0F && code32[2] == 0x85)
            {
                int offset = code32[3] | (code32[4] << 8) | (code32[5] << 16) | (code32[6] << 24);
                TestRunner.Assert(offset == -7, "x86_32: Backward jump offset is correctly calculated",
                       $"Expected offset -7, got {offset}");
            }
        }
        catch (Exception ex)
        {
            TestRunner.Assert(false, "x86_32: Backward jump compiles", ex.Message);
        }
    }
}
