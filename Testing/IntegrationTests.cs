using System;

namespace Badger.Testing;

/// <summary>
/// End-to-end integration tests for the complete BADGER pipeline.
/// Tests assembly -> machine code -> container emission workflows.
/// </summary>
public static class IntegrationTests
{
    public static void RunTests()
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
            TestRunner.Assert(machineCode.Length > 0, "E2E: Assembly to machine code");
            
            var nativeBinary = Badger.Containers.Native.Emit(machineCode);
            TestRunner.Assert(nativeBinary.Length == machineCode.Length, 
                   "E2E: Machine code to Native binary");
            
            var peBinary = Badger.Containers.PE.Emit(machineCode);
            TestRunner.Assert(peBinary.Length > machineCode.Length,
                   "E2E: Machine code to PE binary");
        }
        catch (Exception ex)
        {
            TestRunner.Assert(false, "E2E: Complete pipeline", ex.Message);
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
            TestRunner.Assert(code.Length > 0, "E2E: Stack arithmetic compiles");
            
            var binary = Badger.Containers.PE.Emit(code);
            TestRunner.Assert(binary[0] == 0x4D && binary[1] == 0x5A,
                   "E2E: Stack arithmetic produces valid PE");
        }
        catch (Exception ex)
        {
            TestRunner.Assert(false, "E2E: Stack arithmetic", ex.Message);
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
            TestRunner.Assert(code.Length > 0, "E2E: Complex function compiles");
            
            // Test both container formats
            var native = Badger.Containers.Native.Emit(code);
            var pe = Badger.Containers.PE.Emit(code);
            
            TestRunner.Assert(native.Length == code.Length && pe.Length > code.Length,
                   "E2E: Complex function in both containers");
        }
        catch (Exception ex)
        {
            TestRunner.Assert(false, "E2E: Complex function", ex.Message);
        }
    }
}
