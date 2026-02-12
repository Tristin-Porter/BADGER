using System;

namespace Badger.Testing;

/// <summary>
/// Tests for x86_64 assembler and instruction encoding.
/// Verifies correct assembly parsing and machine code generation for x86_64 architecture.
/// </summary>
public static class X86_64Tests
{
    public static void RunTests()
    {
        TestX86_64Assembler();
        TestX86_64Instructions();
    }
    
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
            TestRunner.Assert(code != null && code.Length > 0, "Basic assembly produces output");
        }
        catch (Exception ex)
        {
            TestRunner.Assert(false, "Basic assembly produces output", ex.Message);
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
            TestRunner.Assert(code != null && code.Length > 0, "Assembly with labels produces output");
        }
        catch (Exception ex)
        {
            TestRunner.Assert(false, "Assembly with labels produces output", ex.Message);
        }
    }
    
    private static void TestX86_64Instructions()
    {
        Console.WriteLine("\n--- x86_64 Instruction Encoding Tests ---");
        
        // Test RET instruction
        var retCode = Badger.Architectures.x86_64.Assembler.Assemble("ret");
        TestRunner.AssertArrayEqual(new byte[] { 0xC3 }, retCode, "RET instruction encoding");
        
        // Test NOP instruction
        var nopCode = Badger.Architectures.x86_64.Assembler.Assemble("nop");
        TestRunner.AssertArrayEqual(new byte[] { 0x90 }, nopCode, "NOP instruction encoding");
        
        // Test PUSH rbp
        var pushCode = Badger.Architectures.x86_64.Assembler.Assemble("push rbp");
        TestRunner.AssertArrayEqual(new byte[] { 0x55 }, pushCode, "PUSH rbp encoding");
        
        // Test POP rbp
        var popCode = Badger.Architectures.x86_64.Assembler.Assemble("pop rbp");
        TestRunner.AssertArrayEqual(new byte[] { 0x5D }, popCode, "POP rbp encoding");
        
        // Test PUSH rax
        var pushRaxCode = Badger.Architectures.x86_64.Assembler.Assemble("push rax");
        TestRunner.AssertArrayEqual(new byte[] { 0x50 }, pushRaxCode, "PUSH rax encoding");
        
        // Test POP rax
        var popRaxCode = Badger.Architectures.x86_64.Assembler.Assemble("pop rax");
        TestRunner.AssertArrayEqual(new byte[] { 0x58 }, popRaxCode, "POP rax encoding");
        
        // Test CQO (sign extend rax to rdx:rax)
        var cqoCode = Badger.Architectures.x86_64.Assembler.Assemble("cqo");
        TestRunner.AssertArrayEqual(new byte[] { 0x48, 0x99 }, cqoCode, "CQO encoding");
        
        // Test MOV register to register
        var movCode = Badger.Architectures.x86_64.Assembler.Assemble("mov rbp, rsp");
        TestRunner.Assert(movCode.Length == 3 && movCode[0] == 0x48 && movCode[1] == 0x89, 
               "MOV rbp, rsp encoding (REX.W + MOV)");
        
        // Test ADD register to register
        var addCode = Badger.Architectures.x86_64.Assembler.Assemble("add rax, rbx");
        TestRunner.Assert(addCode.Length == 3 && addCode[0] == 0x48 && addCode[1] == 0x01,
               "ADD rax, rbx encoding");
        
        // Test SUB register to register
        var subCode = Badger.Architectures.x86_64.Assembler.Assemble("sub rax, rbx");
        TestRunner.Assert(subCode.Length == 3 && subCode[0] == 0x48 && subCode[1] == 0x29,
               "SUB rax, rbx encoding");
        
        // Test XOR register to register
        var xorCode = Badger.Architectures.x86_64.Assembler.Assemble("xor rdx, rdx");
        TestRunner.Assert(xorCode.Length == 3 && xorCode[0] == 0x48 && xorCode[1] == 0x31,
               "XOR rdx, rdx encoding");
        
        // Test AND register to register
        var andCode = Badger.Architectures.x86_64.Assembler.Assemble("and rax, rbx");
        TestRunner.Assert(andCode.Length == 3 && andCode[0] == 0x48 && andCode[1] == 0x21,
               "AND rax, rbx encoding");
        
        // Test OR register to register
        var orCode = Badger.Architectures.x86_64.Assembler.Assemble("or rax, rbx");
        TestRunner.Assert(orCode.Length == 3 && orCode[0] == 0x48 && orCode[1] == 0x09,
               "OR rax, rbx encoding");
        
        // Test CMP register to register
        var cmpCode = Badger.Architectures.x86_64.Assembler.Assemble("cmp rax, rbx");
        TestRunner.Assert(cmpCode.Length == 3 && cmpCode[0] == 0x48 && cmpCode[1] == 0x39,
               "CMP rax, rbx encoding");
        
        // Test TEST register to register
        var testCode = Badger.Architectures.x86_64.Assembler.Assemble("test rax, rax");
        TestRunner.Assert(testCode.Length == 3 && testCode[0] == 0x48 && testCode[1] == 0x85,
               "TEST rax, rax encoding");
        
        // Test IMUL register to register
        var imulCode = Badger.Architectures.x86_64.Assembler.Assemble("imul rax, rbx");
        TestRunner.Assert(imulCode.Length == 4 && imulCode[0] == 0x48 && imulCode[1] == 0x0F && imulCode[2] == 0xAF,
               "IMUL rax, rbx encoding");
        
        // Test IDIV
        var idivCode = Badger.Architectures.x86_64.Assembler.Assemble("idiv rbx");
        TestRunner.Assert(idivCode.Length == 3 && idivCode[0] == 0x48 && idivCode[1] == 0xF7,
               "IDIV rbx encoding");
        
        // Test DIV
        var divCode = Badger.Architectures.x86_64.Assembler.Assemble("div rbx");
        TestRunner.Assert(divCode.Length == 3 && divCode[0] == 0x48 && divCode[1] == 0xF7,
               "DIV rbx encoding");
    }
}
