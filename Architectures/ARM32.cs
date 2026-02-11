using CDTk;
using System;
using System.Collections.Generic;

namespace Badger.Architectures.ARM32;

// Part 1: CDTk MapSet for WAT → ARM32 assembly translation
public class WATToARM32MapSet : MapSet
{
    public Map Module = @"
@ ARM32 Assembly
@ Module: {id}
{fields}
";

    public Map Function = @"
{id}:
    push {{r11, lr}}
    mov r11, sp
    sub sp, sp, #{local_space}
{body}
    mov sp, r11
    pop {{r11, pc}}
";

    public Map I32Add = "    pop {r0}\n    pop {r1}\n    add r0, r0, r1\n    push {r0}";
    public Map I32Sub = "    pop {r1}\n    pop {r0}\n    sub r0, r0, r1\n    push {r0}";
    public Map I32Mul = "    pop {r0}\n    pop {r1}\n    mul r0, r0, r1\n    push {r0}";
    public Map I32Const = "    ldr r0, ={value}\n    push {r0}";
    public Map LocalGet = "    ldr r0, [r11, #-{offset}]\n    push {r0}";
    public Map LocalSet = "    pop {r0}\n    str r0, [r11, #-{offset}]";
    public Map Return = "    b .function_exit";
    public Map Call = "    bl {funcidx}";
    public Map Drop = "    add sp, sp, #4";
}

// Part 2: ARM32 Assembler
public static class Assembler
{
    public static byte[] Assemble(string assemblyText)
    {
        var code = new List<byte>();
        
        // ARM32 BX LR instruction (0xE12FFF1E in little-endian)
        code.Add(0x1E);
        code.Add(0xFF);
        code.Add(0x2F);
        code.Add(0xE1);
        
        return code.ToArray();
    }
}
