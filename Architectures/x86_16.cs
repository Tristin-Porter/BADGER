using CDTk;
using System;
using System.Collections.Generic;

namespace Badger.Architectures.x86_16;

// Part 1: CDTk MapSet for WAT → x86_16 assembly translation
public class WATToX86_16MapSet : MapSet
{
    public Map Module = @"
; x86_16 Assembly (Real Mode)
; Module: {id}
{fields}
";

    public Map Function = @"
{id}:
    push bp
    mov bp, sp
    sub sp, {local_space}
{body}
    mov sp, bp
    pop bp
    ret
";

    public Map I32Add = "    pop ax\n    pop bx\n    add ax, bx\n    push ax";
    public Map I32Sub = "    pop bx\n    pop ax\n    sub ax, bx\n    push ax";
    public Map I32Mul = "    pop ax\n    pop bx\n    imul ax, bx\n    push ax";
    public Map I32Const = "    mov ax, {value}\n    push ax";
    public Map LocalGet = "    mov ax, [bp - {offset}]\n    push ax";
    public Map LocalSet = "    pop ax\n    mov [bp - {offset}], ax";
    public Map Return = "    jmp .function_exit";
    public Map Call = "    call {funcidx}";
    public Map Drop = "    add sp, 2";
}

// Part 2: x86_16 Assembler - converts assembly text to machine code
public static class Assembler
{
    public static byte[] Assemble(string assemblyText)
    {
        var labels = new Dictionary<string, int>();
        var code = new List<byte>();
        
        var lines = assemblyText.Split('\n', StringSplitOptions.RemoveEmptyEntries);
        
        // First pass: collect labels
        int address = 0;
        foreach (var line in lines)
        {
            var trimmed = line.Trim();
            if (string.IsNullOrEmpty(trimmed) || trimmed.StartsWith(";"))
                continue;
                
            if (trimmed.EndsWith(":"))
            {
                var label = trimmed.TrimEnd(':');
                labels[label] = address;
            }
            else
            {
                address += EstimateInstructionSize(trimmed);
            }
        }
        
        // Second pass: encode instructions
        foreach (var line in lines)
        {
            var trimmed = line.Trim();
            if (string.IsNullOrEmpty(trimmed) || trimmed.StartsWith(";") || trimmed.EndsWith(":"))
                continue;
                
            EncodeInstruction(trimmed, code, labels);
        }
        
        return code.ToArray();
    }
    
    private static int GetCurrentAddress(List<byte> code)
    {
        return code.Count;
    }
    
    private static int CalculateRelativeOffsetFrom(string label, int fromAddress, int instructionSize, Dictionary<string, int> labels)
    {
        if (!labels.ContainsKey(label))
            return 0; // Label not found, use placeholder
            
        int targetAddress = labels[label];
        // Offset is relative to the end of the instruction
        return targetAddress - (fromAddress + instructionSize);
    }
    
    private static int EstimateInstructionSize(string instruction)
    {
        // Size estimation for 16-bit instructions
        var parts = instruction.Split(new[] { ' ', ',' }, StringSplitOptions.RemoveEmptyEntries);
        var opcode = parts[0].ToLower();
        
        if (opcode == "mov")
        {
            // MOV r16, imm16 = 3 bytes (opcode + 2-byte immediate)
            // MOV r16, r16 = 2 bytes (opcode + ModR/M)
            if (parts.Length > 2 && IsImmediate(parts[2]))
                return 3;
            return 2;
        }
        if (opcode == "push" || opcode == "pop" || opcode == "ret" || opcode == "retf" || opcode == "nop") return 1;
        if (opcode == "add" || opcode == "sub")
        {
            // ADD/SUB with imm8 = 3 bytes, with imm16 = 4 bytes
            if (parts.Length > 2 && IsImmediate(parts[2]))
            {
                int imm = int.Parse(parts[2]);
                return (imm >= -128 && imm <= 127) ? 3 : 4;
            }
            return 2;
        }
        if (opcode == "call" || opcode == "jmp") return 3; // 16-bit near call/jmp
        if (opcode.StartsWith("j")) return 4; // Conditional jumps (0F + opcode + 2-byte offset)
        if (opcode == "imul") return 3;
        if (opcode == "sete" || opcode == "setne" || opcode == "setl" || opcode == "setg") return 3;
        if (opcode == "movzx") return 3;
        return 2;
    }
    
    private static void EncodeInstruction(string instruction, List<byte> code, Dictionary<string, int> labels)
    {
        var parts = instruction.Split(new[] { ' ', ',' }, StringSplitOptions.RemoveEmptyEntries);
        var opcode = parts[0].ToLower();
        
        switch (opcode)
        {
            case "push":
                EncodePush(parts[1], code);
                break;
            case "pop":
                EncodePop(parts[1], code);
                break;
            case "mov":
                EncodeMov(parts[1], parts[2], code);
                break;
            case "add":
                EncodeAdd(parts[1], parts[2], code);
                break;
            case "sub":
                EncodeSub(parts[1], parts[2], code);
                break;
            case "imul":
                EncodeIMul(parts[1], parts[2], code);
                break;
            case "and":
                EncodeAnd(parts[1], parts[2], code);
                break;
            case "or":
                EncodeOr(parts[1], parts[2], code);
                break;
            case "xor":
                EncodeXor(parts[1], parts[2], code);
                break;
            case "cmp":
                EncodeCmp(parts[1], parts[2], code);
                break;
            case "test":
                EncodeTest(parts[1], parts[2], code);
                break;
            case "jmp":
                EncodeJmp(parts[1], code, labels);
                break;
            case "jnz":
                EncodeJnz(parts[1], code, labels);
                break;
            case "je":
                EncodeJe(parts[1], code, labels);
                break;
            case "jne":
                EncodeJne(parts[1], code, labels);
                break;
            case "jl":
                EncodeJl(parts[1], code, labels);
                break;
            case "jg":
                EncodeJg(parts[1], code, labels);
                break;
            case "call":
                EncodeCall(parts[1], code, labels);
                break;
            case "ret":
                EncodeRet(code);
                break;
            case "retf":
                EncodeRetf(code);
                break;
            case "nop":
                EncodeNop(code);
                break;
            case "sete":
            case "setne":
            case "setl":
            case "setg":
                EncodeSet(opcode, parts[1], code);
                break;
            case "movzx":
                EncodeMovzx(parts[1], parts[2], code);
                break;
            default:
                throw new NotImplementedException($"Instruction not implemented: {opcode}");
        }
    }
    
    // Register encoding helpers for 16-bit registers
    private static byte GetRegisterCode(string reg)
    {
        return reg.ToLower() switch
        {
            "ax" or "al" => 0,
            "cx" or "cl" => 1,
            "dx" or "dl" => 2,
            "bx" or "bl" => 3,
            "sp" or "ah" => 4,
            "bp" or "ch" => 5,
            "si" or "dh" => 6,
            "di" or "bh" => 7,
            _ => throw new ArgumentException($"Unknown register: {reg}")
        };
    }
    
    private static bool IsImmediate(string operand)
    {
        return !string.IsNullOrEmpty(operand) && (char.IsDigit(operand[0]) || operand[0] == '-');
    }
    
    // Instruction encoders for 16-bit
    private static void EncodePush(string operand, List<byte> code)
    {
        // PUSH r16
        code.Add((byte)(0x50 + GetRegisterCode(operand)));
    }
    
    private static void EncodePop(string operand, List<byte> code)
    {
        // POP r16
        code.Add((byte)(0x58 + GetRegisterCode(operand)));
    }
    
    private static void EncodeMov(string dst, string src, List<byte> code)
    {
        // Guard against empty strings
        if (string.IsNullOrEmpty(dst) || string.IsNullOrEmpty(src))
        {
            throw new ArgumentException($"MOV instruction requires non-empty source and destination operands (dst='{dst}', src='{src}')");
        }
        
        if (src.StartsWith("[") && src.EndsWith("]"))
        {
            // MOV r16, [m16]
            code.Add(0x8B); // MOV r16, r/m16
            // ModR/M byte (simplified - would need full parsing for complete implementation)
            code.Add(0x46); // [bp + disp8]
            code.Add(0x00); // displacement
        }
        else if (dst.StartsWith("[") && dst.EndsWith("]"))
        {
            // MOV [m16], r16
            code.Add(0x89); // MOV r/m16, r16
            code.Add(0x46); // [bp + disp8]
            code.Add(0x00); // displacement
        }
        else if (IsImmediate(src))
        {
            // MOV r16, imm16
            code.Add((byte)(0xB8 + GetRegisterCode(dst)));
            AddImmediate16(int.Parse(src), code);
        }
        else
        {
            // MOV r16, r16
            code.Add(0x89); // MOV r/m16, r16
            code.Add((byte)(0xC0 | (GetRegisterCode(src) << 3) | GetRegisterCode(dst)));
        }
    }
    
    private static void EncodeAdd(string dst, string src, List<byte> code)
    {
        // Check if src is immediate
        if (IsImmediate(src))
        {
            int imm = int.Parse(src);
            if (imm >= -128 && imm <= 127)
            {
                // ADD r/m16, imm8
                code.Add(0x83);
                code.Add((byte)(0xC0 + GetRegisterCode(dst))); // ModR/M: mod=11, reg=000 (/0), r/m=dst
                code.Add((byte)imm);
            }
            else
            {
                // ADD r/m16, imm16
                code.Add(0x81);
                code.Add((byte)(0xC0 + GetRegisterCode(dst))); // ModR/M: mod=11, reg=000 (/0), r/m=dst
                AddImmediate16(imm, code);
            }
        }
        else
        {
            // ADD r/m16, r16
            code.Add(0x01);
            code.Add((byte)(0xC0 | (GetRegisterCode(src) << 3) | GetRegisterCode(dst)));
        }
    }
    
    private static void EncodeSub(string dst, string src, List<byte> code)
    {
        // Check if src is immediate
        if (IsImmediate(src))
        {
            int imm = int.Parse(src);
            if (imm >= -128 && imm <= 127)
            {
                // SUB r/m16, imm8
                code.Add(0x83);
                code.Add((byte)(0xE8 + GetRegisterCode(dst))); // ModR/M: mod=11, reg=101 (/5), r/m=dst
                code.Add((byte)imm);
            }
            else
            {
                // SUB r/m16, imm16
                code.Add(0x81);
                code.Add((byte)(0xE8 + GetRegisterCode(dst))); // ModR/M: mod=11, reg=101 (/5), r/m=dst
                AddImmediate16(imm, code);
            }
        }
        else
        {
            // SUB r/m16, r16
            code.Add(0x29);
            code.Add((byte)(0xC0 | (GetRegisterCode(src) << 3) | GetRegisterCode(dst)));
        }
    }
    
    private static void EncodeIMul(string dst, string src, List<byte> code)
    {
        // IMUL r16, r/m16
        code.Add(0x0F);
        code.Add(0xAF);
        code.Add((byte)(0xC0 | (GetRegisterCode(dst) << 3) | GetRegisterCode(src)));
    }
    
    private static void EncodeAnd(string dst, string src, List<byte> code)
    {
        // AND r/m16, r16
        code.Add(0x21);
        code.Add((byte)(0xC0 | (GetRegisterCode(src) << 3) | GetRegisterCode(dst)));
    }
    
    private static void EncodeOr(string dst, string src, List<byte> code)
    {
        // OR r/m16, r16
        code.Add(0x09);
        code.Add((byte)(0xC0 | (GetRegisterCode(src) << 3) | GetRegisterCode(dst)));
    }
    
    private static void EncodeXor(string dst, string src, List<byte> code)
    {
        // XOR r/m16, r16
        code.Add(0x31);
        code.Add((byte)(0xC0 | (GetRegisterCode(src) << 3) | GetRegisterCode(dst)));
    }
    
    private static void EncodeCmp(string dst, string src, List<byte> code)
    {
        // CMP r/m16, r16
        code.Add(0x39);
        code.Add((byte)(0xC0 | (GetRegisterCode(src) << 3) | GetRegisterCode(dst)));
    }
    
    private static void EncodeTest(string dst, string src, List<byte> code)
    {
        // TEST r/m16, r16
        code.Add(0x85);
        code.Add((byte)(0xC0 | (GetRegisterCode(src) << 3) | GetRegisterCode(dst)));
    }
    
    private static void EncodeJmp(string label, List<byte> code, Dictionary<string, int> labels)
    {
        // JMP rel16 (near jump)
        int currentAddress = GetCurrentAddress(code);
        code.Add(0xE9);
        int offset = CalculateRelativeOffsetFrom(label, currentAddress, 3, labels); // JMP instruction is 3 bytes (1 opcode + 2 offset)
        AddImmediate16(offset, code);
    }
    
    private static void EncodeJnz(string label, List<byte> code, Dictionary<string, int> labels)
    {
        // JNZ rel16
        int currentAddress = GetCurrentAddress(code);
        code.Add(0x0F);
        code.Add(0x85);
        int offset = CalculateRelativeOffsetFrom(label, currentAddress, 4, labels); // JNZ instruction is 4 bytes
        AddImmediate16(offset, code);
    }
    
    private static void EncodeJe(string label, List<byte> code, Dictionary<string, int> labels)
    {
        // JE rel16
        int currentAddress = GetCurrentAddress(code);
        code.Add(0x0F);
        code.Add(0x84);
        int offset = CalculateRelativeOffsetFrom(label, currentAddress, 4, labels); // JE instruction is 4 bytes
        AddImmediate16(offset, code);
    }
    
    private static void EncodeJne(string label, List<byte> code, Dictionary<string, int> labels)
    {
        // JNE rel16
        int currentAddress = GetCurrentAddress(code);
        code.Add(0x0F);
        code.Add(0x85);
        int offset = CalculateRelativeOffsetFrom(label, currentAddress, 4, labels); // JNE instruction is 4 bytes
        AddImmediate16(offset, code);
    }
    
    private static void EncodeJl(string label, List<byte> code, Dictionary<string, int> labels)
    {
        // JL rel16
        int currentAddress = GetCurrentAddress(code);
        code.Add(0x0F);
        code.Add(0x8C);
        int offset = CalculateRelativeOffsetFrom(label, currentAddress, 4, labels); // JL instruction is 4 bytes
        AddImmediate16(offset, code);
    }
    
    private static void EncodeJg(string label, List<byte> code, Dictionary<string, int> labels)
    {
        // JG rel16
        int currentAddress = GetCurrentAddress(code);
        code.Add(0x0F);
        code.Add(0x8F);
        int offset = CalculateRelativeOffsetFrom(label, currentAddress, 4, labels); // JG instruction is 4 bytes
        AddImmediate16(offset, code);
    }
    
    private static void EncodeCall(string target, List<byte> code, Dictionary<string, int> labels)
    {
        // CALL rel16 (near call)
        int currentAddress = GetCurrentAddress(code);
        code.Add(0xE8);
        int offset = CalculateRelativeOffsetFrom(target, currentAddress, 3, labels); // CALL instruction is 3 bytes
        AddImmediate16(offset, code);
    }
    
    private static void EncodeRet(List<byte> code)
    {
        // RET (near return)
        code.Add(0xC3);
    }
    
    private static void EncodeRetf(List<byte> code)
    {
        // RETF (far return for real mode)
        code.Add(0xCB);
    }
    
    private static void EncodeNop(List<byte> code)
    {
        // NOP
        code.Add(0x90);
    }
    
    private static void EncodeSet(string op, string reg, List<byte> code)
    {
        // SETcc r/m8
        code.Add(0x0F);
        code.Add(op switch
        {
            "sete" => (byte)0x94,
            "setne" => (byte)0x95,
            "setl" => (byte)0x9C,
            "setg" => (byte)0x9F,
            _ => throw new ArgumentException($"Unknown set instruction: {op}")
        });
        code.Add((byte)(0xC0 | GetRegisterCode(reg)));
    }
    
    private static void EncodeMovzx(string dst, string src, List<byte> code)
    {
        // MOVZX r16, r/m8
        code.Add(0x0F);
        code.Add(0xB6);
        code.Add((byte)(0xC0 | (GetRegisterCode(dst) << 3) | GetRegisterCode(src)));
    }
    
    private static void AddImmediate16(int value, List<byte> code)
    {
        // Add a 16-bit immediate value (little-endian)
        code.Add((byte)(value & 0xFF));
        code.Add((byte)((value >> 8) & 0xFF));
    }
}
