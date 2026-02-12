using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Badger;

/// <summary>
/// Comprehensive test suite for BADGER assembler.
/// Tests WAT parsing, architecture lowering, instruction encoding,
/// label resolution, and container emission.
/// </summary>
public static class Testing
{
    private static int passedTests = 0;
    private static int failedTests = 0;
    private static List<string> failures = new List<string>();
    
    public static void RunAllTests()
    {
        Console.WriteLine("=".PadRight(70, '='));
        Console.WriteLine("BADGER Test Suite");
        Console.WriteLine("=".PadRight(70, '='));
        Console.WriteLine();
        
        passedTests = 0;
        failedTests = 0;
        failures.Clear();
        
        // Run test suites
        TestWATTokens();
        TestX86_64Assembler();
        TestX86_64Instructions();
        TestLabelResolution();
        TestNativeContainer();
        TestPEContainer();
        TestEndToEnd();
        
        // Print summary
        Console.WriteLine();
        Console.WriteLine("=".PadRight(70, '='));
        Console.WriteLine($"Test Results: {passedTests} passed, {failedTests} failed");
        Console.WriteLine("=".PadRight(70, '='));
        
        if (failedTests > 0)
        {
            Console.WriteLine();
            Console.WriteLine("Failed tests:");
            foreach (var failure in failures)
            {
                Console.WriteLine($"  - {failure}");
            }
        }
    }
    
    private static void Assert(bool condition, string testName, string message = "")
    {
        if (condition)
        {
            passedTests++;
            Console.WriteLine($"✓ {testName}");
        }
        else
        {
            failedTests++;
            string fullMessage = string.IsNullOrEmpty(message) ? testName : $"{testName}: {message}";
            failures.Add(fullMessage);
            Console.WriteLine($"✗ {testName}");
            if (!string.IsNullOrEmpty(message))
            {
                Console.WriteLine($"  {message}");
            }
        }
    }
    
    private static void AssertArrayEqual(byte[] expected, byte[] actual, string testName)
    {
        if (expected.Length != actual.Length)
        {
            Assert(false, testName, $"Length mismatch: expected {expected.Length}, got {actual.Length}");
            return;
        }
        
        for (int i = 0; i < expected.Length; i++)
        {
            if (expected[i] != actual[i])
            {
                Assert(false, testName, $"Byte mismatch at index {i}: expected 0x{expected[i]:X2}, got 0x{actual[i]:X2}");
                return;
            }
        }
        
        Assert(true, testName);
    }
    
    // ============================================================
    // WAT Token Tests
    // ============================================================
    
    private static void TestWATTokens()
    {
        Console.WriteLine("\n--- WAT Token Tests ---");
        
        // Test that token definitions exist and are accessible
        var tokens = new WATTokens();
        
        Assert(tokens.Module != null, "Module token exists");
        Assert(tokens.Func != null, "Func token exists");
        Assert(tokens.I32Add != null, "I32Add token exists");
        Assert(tokens.I32Const != null, "I32Const token exists");
        Assert(tokens.LocalGet != null, "LocalGet token exists");
        Assert(tokens.LocalSet != null, "LocalSet token exists");
        Assert(tokens.Return != null, "Return token exists");
        Assert(tokens.Call != null, "Call token exists");
        Assert(tokens.LeftParen != null, "LeftParen token exists");
        Assert(tokens.RightParen != null, "RightParen token exists");
        Assert(tokens.Identifier != null, "Identifier token exists");
        Assert(tokens.Integer != null, "Integer token exists");
        
        // Test numeric instruction tokens
        Assert(tokens.I32Sub != null, "I32Sub token exists");
        Assert(tokens.I32Mul != null, "I32Mul token exists");
        Assert(tokens.I32DivS != null, "I32DivS token exists");
        Assert(tokens.I32And != null, "I32And token exists");
        Assert(tokens.I32Or != null, "I32Or token exists");
        Assert(tokens.I32Xor != null, "I32Xor token exists");
        
        // Test comparison tokens
        Assert(tokens.I32Eq != null, "I32Eq token exists");
        Assert(tokens.I32Ne != null, "I32Ne token exists");
        Assert(tokens.I32LtS != null, "I32LtS token exists");
        Assert(tokens.I32GtS != null, "I32GtS token exists");
        
        // Test control flow tokens
        Assert(tokens.Block != null, "Block token exists");
        Assert(tokens.Loop != null, "Loop token exists");
        Assert(tokens.If != null, "If token exists");
        Assert(tokens.Br != null, "Br token exists");
        Assert(tokens.BrIf != null, "BrIf token exists");
    }
    
    // ============================================================
    // x86_64 Assembler Tests
    // ============================================================
    
    private static void TestX86_64Assembler()
    {
        Console.WriteLine("\n--- x86_64 Assembler Tests ---");
        
        // Test basic assembly parsing
        string simpleAsm = @"
main:
    push rbp
    pop rbp
    ret
";
        
        try
        {
            var code = Badger.Architectures.x86_64.Assembler.Assemble(simpleAsm);
            Assert(code != null && code.Length > 0, "Basic assembly produces output");
        }
        catch (Exception ex)
        {
            Assert(false, "Basic assembly produces output", ex.Message);
        }
        
        // Test that assembler handles labels
        string labelAsm = @"
start:
    nop
loop:
    nop
    ret
";
        
        try
        {
            var code = Badger.Architectures.x86_64.Assembler.Assemble(labelAsm);
            Assert(code != null && code.Length > 0, "Assembly with labels produces output");
        }
        catch (Exception ex)
        {
            Assert(false, "Assembly with labels produces output", ex.Message);
        }
    }
    
    // ============================================================
    // x86_64 Instruction Encoding Tests
    // ============================================================
    
    private static void TestX86_64Instructions()
    {
        Console.WriteLine("\n--- x86_64 Instruction Encoding Tests ---");
        
        // Test RET instruction
        var retCode = Badger.Architectures.x86_64.Assembler.Assemble("ret");
        AssertArrayEqual(new byte[] { 0xC3 }, retCode, "RET instruction encoding");
        
        // Test NOP instruction
        var nopCode = Badger.Architectures.x86_64.Assembler.Assemble("nop");
        AssertArrayEqual(new byte[] { 0x90 }, nopCode, "NOP instruction encoding");
        
        // Test PUSH rbp
        var pushCode = Badger.Architectures.x86_64.Assembler.Assemble("push rbp");
        AssertArrayEqual(new byte[] { 0x55 }, pushCode, "PUSH rbp encoding");
        
        // Test POP rbp
        var popCode = Badger.Architectures.x86_64.Assembler.Assemble("pop rbp");
        AssertArrayEqual(new byte[] { 0x5D }, popCode, "POP rbp encoding");
        
        // Test PUSH rax
        var pushRaxCode = Badger.Architectures.x86_64.Assembler.Assemble("push rax");
        AssertArrayEqual(new byte[] { 0x50 }, pushRaxCode, "PUSH rax encoding");
        
        // Test POP rax
        var popRaxCode = Badger.Architectures.x86_64.Assembler.Assemble("pop rax");
        AssertArrayEqual(new byte[] { 0x58 }, popRaxCode, "POP rax encoding");
        
        // Test CQO (sign extend rax to rdx:rax)
        var cqoCode = Badger.Architectures.x86_64.Assembler.Assemble("cqo");
        AssertArrayEqual(new byte[] { 0x48, 0x99 }, cqoCode, "CQO encoding");
        
        // Test MOV register to register
        var movCode = Badger.Architectures.x86_64.Assembler.Assemble("mov rbp, rsp");
        Assert(movCode.Length == 3 && movCode[0] == 0x48 && movCode[1] == 0x89, 
               "MOV rbp, rsp encoding (REX.W + MOV)");
        
        // Test ADD register to register
        var addCode = Badger.Architectures.x86_64.Assembler.Assemble("add rax, rbx");
        Assert(addCode.Length == 3 && addCode[0] == 0x48 && addCode[1] == 0x01,
               "ADD rax, rbx encoding");
        
        // Test SUB register to register
        var subCode = Badger.Architectures.x86_64.Assembler.Assemble("sub rax, rbx");
        Assert(subCode.Length == 3 && subCode[0] == 0x48 && subCode[1] == 0x29,
               "SUB rax, rbx encoding");
        
        // Test XOR register to register
        var xorCode = Badger.Architectures.x86_64.Assembler.Assemble("xor rdx, rdx");
        Assert(xorCode.Length == 3 && xorCode[0] == 0x48 && xorCode[1] == 0x31,
               "XOR rdx, rdx encoding");
        
        // Test AND register to register
        var andCode = Badger.Architectures.x86_64.Assembler.Assemble("and rax, rbx");
        Assert(andCode.Length == 3 && andCode[0] == 0x48 && andCode[1] == 0x21,
               "AND rax, rbx encoding");
        
        // Test OR register to register
        var orCode = Badger.Architectures.x86_64.Assembler.Assemble("or rax, rbx");
        Assert(orCode.Length == 3 && orCode[0] == 0x48 && orCode[1] == 0x09,
               "OR rax, rbx encoding");
        
        // Test CMP register to register
        var cmpCode = Badger.Architectures.x86_64.Assembler.Assemble("cmp rax, rbx");
        Assert(cmpCode.Length == 3 && cmpCode[0] == 0x48 && cmpCode[1] == 0x39,
               "CMP rax, rbx encoding");
        
        // Test TEST register to register
        var testCode = Badger.Architectures.x86_64.Assembler.Assemble("test rax, rax");
        Assert(testCode.Length == 3 && testCode[0] == 0x48 && testCode[1] == 0x85,
               "TEST rax, rax encoding");
        
        // Test IMUL register to register
        var imulCode = Badger.Architectures.x86_64.Assembler.Assemble("imul rax, rbx");
        Assert(imulCode.Length == 4 && imulCode[0] == 0x48 && imulCode[1] == 0x0F && imulCode[2] == 0xAF,
               "IMUL rax, rbx encoding");
        
        // Test IDIV
        var idivCode = Badger.Architectures.x86_64.Assembler.Assemble("idiv rbx");
        Assert(idivCode.Length == 3 && idivCode[0] == 0x48 && idivCode[1] == 0xF7,
               "IDIV rbx encoding");
        
        // Test DIV
        var divCode = Badger.Architectures.x86_64.Assembler.Assemble("div rbx");
        Assert(divCode.Length == 3 && divCode[0] == 0x48 && divCode[1] == 0xF7,
               "DIV rbx encoding");
    }
    
    // ============================================================
    // Label Resolution Tests
    // ============================================================
    
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
end:
    ret
";
        
        try
        {
            var code = Badger.Architectures.x86_64.Assembler.Assemble(asmWithLabels);
            Assert(code.Length == 4, "Label resolution produces correct code size", 
                   $"Expected 4 bytes (3 NOPs + 1 RET), got {code.Length}");
        }
        catch (Exception ex)
        {
            Assert(false, "Label resolution works", ex.Message);
        }
        
        // Test forward and backward label references
        string jumpAsm = @"
main:
    jmp end
    nop
end:
    ret
";
        
        try
        {
            var code = Badger.Architectures.x86_64.Assembler.Assemble(jumpAsm);
            Assert(code.Length > 0, "Forward jump compiles");
        }
        catch (Exception ex)
        {
            Assert(false, "Forward jump compiles", ex.Message);
        }
    }
    
    // ============================================================
    // Container Tests
    // ============================================================
    
    private static void TestNativeContainer()
    {
        Console.WriteLine("\n--- Native Container Tests ---");
        
        // Test that Native container is pass-through
        byte[] machineCode = new byte[] { 0xC3 }; // RET
        var binary = Badger.Containers.Native.Emit(machineCode);
        
        AssertArrayEqual(machineCode, binary, "Native container is pass-through");
        
        // Test with larger code
        byte[] largerCode = new byte[] { 0x55, 0x48, 0x89, 0xE5, 0x5D, 0xC3 };
        var largerBinary = Badger.Containers.Native.Emit(largerCode);
        
        AssertArrayEqual(largerCode, largerBinary, "Native container preserves larger code");
    }
    
    private static void TestPEContainer()
    {
        Console.WriteLine("\n--- PE Container Tests ---");
        
        // Test basic PE structure
        byte[] machineCode = new byte[] { 0xC3 }; // RET
        var pe = Badger.Containers.PE.Emit(machineCode);
        
        // Check DOS header magic
        Assert(pe.Length >= 2 && pe[0] == 0x4D && pe[1] == 0x5A,
               "PE has valid DOS magic 'MZ'");
        
        // Check PE signature offset
        Assert(pe.Length >= 64, "PE has complete DOS header");
        
        int peOffset = pe[60] | (pe[61] << 8) | (pe[62] << 16) | (pe[63] << 24);
        Assert(peOffset < pe.Length, "PE signature offset is valid");
        
        // Check PE signature
        if (peOffset + 4 <= pe.Length)
        {
            Assert(pe[peOffset] == 0x50 && pe[peOffset + 1] == 0x45 &&
                   pe[peOffset + 2] == 0x00 && pe[peOffset + 3] == 0x00,
                   "PE has valid signature 'PE\\0\\0'");
        }
        else
        {
            Assert(false, "PE signature accessible");
        }
        
        // Check COFF header machine type (x86-64)
        if (peOffset + 6 <= pe.Length)
        {
            int machine = pe[peOffset + 4] | (pe[peOffset + 5] << 8);
            Assert(machine == 0x8664, "PE has correct machine type (x86-64)",
                   $"Expected 0x8664, got 0x{machine:X4}");
        }
        else
        {
            Assert(false, "PE COFF header accessible");
        }
        
        // Check that PE is larger than just machine code (has headers)
        Assert(pe.Length > machineCode.Length,
               "PE binary includes headers and is larger than raw code");
        
        // Check alignment
        Assert(pe.Length % 512 == 0, "PE binary is aligned to file alignment (512 bytes)");
    }
    
    // ============================================================
    // End-to-End Integration Tests
    // ============================================================
    
    private static void TestEndToEnd()
    {
        Console.WriteLine("\n--- End-to-End Integration Tests ---");
        
        // Test complete pipeline: Assembly → Machine Code → Native Binary
        string simpleFunction = @"
main:
    push rbp
    mov rbp, rsp
    mov rsp, rbp
    pop rbp
    ret
";
        
        try
        {
            var machineCode = Badger.Architectures.x86_64.Assembler.Assemble(simpleFunction);
            Assert(machineCode.Length > 0, "E2E: Assembly to machine code");
            
            var nativeBinary = Badger.Containers.Native.Emit(machineCode);
            Assert(nativeBinary.Length == machineCode.Length, 
                   "E2E: Machine code to Native binary");
            
            var peBinary = Badger.Containers.PE.Emit(machineCode);
            Assert(peBinary.Length > machineCode.Length,
                   "E2E: Machine code to PE binary");
        }
        catch (Exception ex)
        {
            Assert(false, "E2E: Complete pipeline", ex.Message);
        }
        
        // Test stack-based arithmetic simulation
        string arithmetic = @"
add_function:
    push rbp
    mov rbp, rsp
    ; Simulate: push 5, push 3, add
    pop rax
    pop rbx
    add rax, rbx
    push rax
    mov rsp, rbp
    pop rbp
    ret
";
        
        try
        {
            var code = Badger.Architectures.x86_64.Assembler.Assemble(arithmetic);
            Assert(code.Length > 0, "E2E: Stack arithmetic compiles");
            
            var binary = Badger.Containers.PE.Emit(code);
            Assert(binary[0] == 0x4D && binary[1] == 0x5A,
                   "E2E: Stack arithmetic produces valid PE");
        }
        catch (Exception ex)
        {
            Assert(false, "E2E: Stack arithmetic", ex.Message);
        }
        
        // Test multiple instructions
        string complex = @"
test:
    push rbp
    mov rbp, rsp
    sub rsp, 16
    ; Arithmetic
    pop rax
    pop rbx
    add rax, rbx
    push rax
    ; Comparison
    pop rbx
    pop rax
    cmp rax, rbx
    sete al
    movzx rax, al
    push rax
    ; Cleanup
    mov rsp, rbp
    pop rbp
    ret
";
        
        try
        {
            var code = Badger.Architectures.x86_64.Assembler.Assemble(complex);
            Assert(code.Length > 0, "E2E: Complex function compiles");
            
            // Test both container formats
            var native = Badger.Containers.Native.Emit(code);
            var pe = Badger.Containers.PE.Emit(code);
            
            Assert(native.Length == code.Length && pe.Length > code.Length,
                   "E2E: Complex function in both containers");
        }
        catch (Exception ex)
        {
            Assert(false, "E2E: Complex function", ex.Message);
        }
    }
}
