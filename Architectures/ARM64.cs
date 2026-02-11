using CDTk;
using System;
using System.Collections.Generic;

namespace Badger.Architectures.ARM64;

// Part 1: CDTk MapSet for WAT → ARM64 assembly translation
public class WATToARM64MapSet : MapSet
{
    public Map Module = @"
// ARM64 Assembly (AArch64)
// Module: {id}
{fields}
";

    public Map Function = @"
{id}:
    stp x29, x30, [sp, #-16]!
    mov x29, sp
    sub sp, sp, #{local_space}
{body}
    mov sp, x29
    ldp x29, x30, [sp], #16
    ret
";

    public Map I32Add = "    ldr w0, [sp], #4\n    ldr w1, [sp], #4\n    add w0, w0, w1\n    str w0, [sp, #-4]!";
    public Map I32Sub = "    ldr w1, [sp], #4\n    ldr w0, [sp], #4\n    sub w0, w0, w1\n    str w0, [sp, #-4]!";
    public Map I32Mul = "    ldr w0, [sp], #4\n    ldr w1, [sp], #4\n    mul w0, w0, w1\n    str w0, [sp, #-4]!";
    public Map I32Const = "    mov w0, #{value}\n    str w0, [sp, #-4]!";
    public Map LocalGet = "    ldr w0, [x29, #-{offset}]\n    str w0, [sp, #-4]!";
    public Map LocalSet = "    ldr w0, [sp], #4\n    str w0, [x29, #-{offset}]";
    public Map Return = "    b .function_exit";
    public Map Call = "    bl {funcidx}";
    public Map Drop = "    add sp, sp, #4";
}

// Part 2: ARM64 Assembler
public static class Assembler
{
    public static byte[] Assemble(string assemblyText)
    {
        var code = new List<byte>();
        
        // ARM64 RET instruction (0xD65F03C0)
        code.Add(0xC0);
        code.Add(0x03);
        code.Add(0x5F);
        code.Add(0xD6);
        
        return code.ToArray();
    }
}
