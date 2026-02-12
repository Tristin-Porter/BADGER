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
        TestX86_32Assembler();
        TestX86_32Instructions();
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
            failures.Add(string.IsNullOrEmpty(message) ? testName : $"{testName}: {message}");
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
    // x86_32 Assembler Tests
    // ============================================================
    
    private static void TestX86_32Assembler()
    {
        Console.WriteLine("\n--- x86_32 Assembler Tests ---");
        
        // Test basic assembly
        var simpleAsm = "ret";
        var result = Badger.Architectures.x86_32.Assembler.Assemble(simpleAsm);
        Assert(result.Length > 0, "Basic assembly produces output");
        
        // Test assembly with labels
        var labelAsm = @"
            main:
                ret
        ";
        var resultWithLabel = Badger.Architectures.x86_32.Assembler.Assemble(labelAsm);
        Assert(resultWithLabel.Length > 0, "Assembly with labels produces output");
    }
    
    // ============================================================
    // x86_32 Instruction Encoding Tests
    // ============================================================
    
    private static void TestX86_32Instructions()
    {
        Console.WriteLine("\n--- x86_32 Instruction Encoding Tests ---");
        
        // Test RET instruction
        var retCode = Badger.Architectures.x86_32.Assembler.Assemble("ret");
        AssertArrayEqual(new byte[] { 0xC3 }, retCode, "RET instruction encoding");
        
        // Test NOP instruction
        var nopCode = Badger.Architectures.x86_32.Assembler.Assemble("nop");
        AssertArrayEqual(new byte[] { 0x90 }, nopCode, "NOP instruction encoding");
        
        // Test PUSH ebp
        var pushCode = Badger.Architectures.x86_32.Assembler.Assemble("push ebp");
        AssertArrayEqual(new byte[] { 0x55 }, pushCode, "PUSH ebp encoding");
        
        // Test POP ebp
        var popCode = Badger.Architectures.x86_32.Assembler.Assemble("pop ebp");
        AssertArrayEqual(new byte[] { 0x5D }, popCode, "POP ebp encoding");
        
        // Test PUSH eax
        var pushEaxCode = Badger.Architectures.x86_32.Assembler.Assemble("push eax");
        AssertArrayEqual(new byte[] { 0x50 }, pushEaxCode, "PUSH eax encoding");
        
        // Test POP eax
        var popEaxCode = Badger.Architectures.x86_32.Assembler.Assemble("pop eax");
        AssertArrayEqual(new byte[] { 0x58 }, popEaxCode, "POP eax encoding");
        
        // Test CDQ (sign extend eax to edx:eax)
        var cdqCode = Badger.Architectures.x86_32.Assembler.Assemble("cdq");
        AssertArrayEqual(new byte[] { 0x99 }, cdqCode, "CDQ encoding");
        
        // Test MOV register to register (no REX prefix!)
        var movCode = Badger.Architectures.x86_32.Assembler.Assemble("mov ebp, esp");
        Assert(movCode.Length == 2 && movCode[0] == 0x89 && movCode[1] == 0xE5, 
               "MOV ebp, esp encoding (no REX prefix)");
        
        // Test MOV immediate
        var movImmCode = Badger.Architectures.x86_32.Assembler.Assemble("mov eax, 42");
        Assert(movImmCode.Length == 5 && movImmCode[0] == 0xB8, 
               "MOV eax, imm32 encoding");
        
        // Test ADD register to register (no REX prefix!)
        var addCode = Badger.Architectures.x86_32.Assembler.Assemble("add eax, ebx");
        Assert(addCode.Length == 2 && addCode[0] == 0x01,
               "ADD eax, ebx encoding (no REX prefix)");
        
        // Test ADD with immediate (8-bit)
        var addImm8Code = Badger.Architectures.x86_32.Assembler.Assemble("add esp, 4");
        AssertArrayEqual(new byte[] { 0x83, 0xC4, 0x04 }, addImm8Code, 
                        "ADD esp, imm8 encoding");
        
        // Test SUB register to register (no REX prefix!)
        var subCode = Badger.Architectures.x86_32.Assembler.Assemble("sub eax, ebx");
        Assert(subCode.Length == 2 && subCode[0] == 0x29,
               "SUB eax, ebx encoding (no REX prefix)");
        
        // Test SUB with immediate (8-bit)
        var subImm8Code = Badger.Architectures.x86_32.Assembler.Assemble("sub esp, 16");
        AssertArrayEqual(new byte[] { 0x83, 0xEC, 0x10 }, subImm8Code,
                        "SUB esp, imm8 encoding");
        
        // Test XOR register to register (no REX prefix!)
        var xorCode = Badger.Architectures.x86_32.Assembler.Assemble("xor edx, edx");
        Assert(xorCode.Length == 2 && xorCode[0] == 0x31,
               "XOR edx, edx encoding (no REX prefix)");
        
        // Test AND register to register (no REX prefix!)
        var andCode = Badger.Architectures.x86_32.Assembler.Assemble("and eax, ebx");
        Assert(andCode.Length == 2 && andCode[0] == 0x21,
               "AND eax, ebx encoding (no REX prefix)");
        
        // Test OR register to register (no REX prefix!)
        var orCode = Badger.Architectures.x86_32.Assembler.Assemble("or eax, ebx");
        Assert(orCode.Length == 2 && orCode[0] == 0x09,
               "OR eax, ebx encoding (no REX prefix)");
        
        // Test CMP register to register (no REX prefix!)
        var cmpCode = Badger.Architectures.x86_32.Assembler.Assemble("cmp eax, ebx");
        Assert(cmpCode.Length == 2 && cmpCode[0] == 0x39,
               "CMP eax, ebx encoding (no REX prefix)");
        
        // Test TEST register to register (no REX prefix!)
        var testCode = Badger.Architectures.x86_32.Assembler.Assemble("test eax, eax");
        Assert(testCode.Length == 2 && testCode[0] == 0x85,
               "TEST eax, eax encoding (no REX prefix)");
        
        // Test IMUL register to register (no REX prefix!)
        var imulCode = Badger.Architectures.x86_32.Assembler.Assemble("imul eax, ebx");
        Assert(imulCode.Length == 3 && imulCode[0] == 0x0F && imulCode[1] == 0xAF,
               "IMUL eax, ebx encoding (no REX prefix)");
        
        // Test IDIV (no REX prefix!)
        var idivCode = Badger.Architectures.x86_32.Assembler.Assemble("idiv ebx");
        Assert(idivCode.Length == 2 && idivCode[0] == 0xF7,
               "IDIV ebx encoding (no REX prefix)");
        
        // Test DIV (no REX prefix!)
        var divCode = Badger.Architectures.x86_32.Assembler.Assemble("div ebx");
        Assert(divCode.Length == 2 && divCode[0] == 0xF7,
               "DIV ebx encoding (no REX prefix)");
        
        // Test conditional jumps
        var jeCode = Badger.Architectures.x86_32.Assembler.Assemble("je target");
        Assert(jeCode.Length == 6 && jeCode[0] == 0x0F && jeCode[1] == 0x84,
               "JE rel32 encoding");
        
        var jneCode = Badger.Architectures.x86_32.Assembler.Assemble("jne target");
        Assert(jneCode.Length == 6 && jneCode[0] == 0x0F && jneCode[1] == 0x85,
               "JNE rel32 encoding");
        
        var jlCode = Badger.Architectures.x86_32.Assembler.Assemble("jl target");
        Assert(jlCode.Length == 6 && jlCode[0] == 0x0F && jlCode[1] == 0x8C,
               "JL rel32 encoding");
        
        var jgCode = Badger.Architectures.x86_32.Assembler.Assemble("jg target");
        Assert(jgCode.Length == 6 && jgCode[0] == 0x0F && jgCode[1] == 0x8F,
               "JG rel32 encoding");
        
        // Test SETcc instructions
        var seteCode = Badger.Architectures.x86_32.Assembler.Assemble("sete al");
        Assert(seteCode.Length == 3 && seteCode[0] == 0x0F && seteCode[1] == 0x94,
               "SETE al encoding");
        
        var setneCode = Badger.Architectures.x86_32.Assembler.Assemble("setne al");
        Assert(setneCode.Length == 3 && setneCode[0] == 0x0F && setneCode[1] == 0x95,
               "SETNE al encoding");
        
        var setlCode = Badger.Architectures.x86_32.Assembler.Assemble("setl al");
        Assert(setlCode.Length == 3 && setlCode[0] == 0x0F && setlCode[1] == 0x9C,
               "SETL al encoding");
        
        var setgCode = Badger.Architectures.x86_32.Assembler.Assemble("setg al");
        Assert(setgCode.Length == 3 && setgCode[0] == 0x0F && setgCode[1] == 0x9F,
               "SETG al encoding");
        
        // Test MOVZX
        var movzxCode = Badger.Architectures.x86_32.Assembler.Assemble("movzx eax, al");
        Assert(movzxCode.Length == 3 && movzxCode[0] == 0x0F && movzxCode[1] == 0xB6,
               "MOVZX eax, al encoding");
        
        // Verify NO REX prefix (0x48) in any 32-bit instruction
        var allInstructions = new[] { 
            movCode, addCode, subCode, xorCode, andCode, orCode, 
            cmpCode, testCode, imulCode, idivCode, divCode 
        };
        foreach (var code in allInstructions)
        {
            Assert(!code.Contains((byte)0x48), 
                   "No REX prefix in x86_32 instructions",
                   "32-bit mode should not use REX prefix (0x48)");
        }
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
function_end:
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
    jmp jump_target
    nop
jump_target:
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
