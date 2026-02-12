using CDTk;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

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

// Part 2: ARM64 Assembler - converts assembly text to machine code
public static class Assembler
{
    public static byte[] Assemble(string assemblyText)
    {
        var labels = new Dictionary<string, int>();
        var code = new List<byte>();
        
        var lines = assemblyText.Split('\n', StringSplitOptions.RemoveEmptyEntries);
        
        // First pass: collect labels
        // All ARM64 instructions are 4 bytes (fixed width)
        int address = 0;
        foreach (var line in lines)
        {
            var trimmed = line.Trim();
            if (string.IsNullOrEmpty(trimmed) || trimmed.StartsWith("//") || trimmed.StartsWith(";"))
                continue;
                
            if (trimmed.EndsWith(":"))
            {
                var label = trimmed.TrimEnd(':');
                labels[label] = address;
            }
            else
            {
                // All ARM64 instructions are 4 bytes
                address += 4;
            }
        }
        
        // Second pass: encode instructions
        int currentAddress = 0;
        foreach (var line in lines)
        {
            var trimmed = line.Trim();
            if (string.IsNullOrEmpty(trimmed) || trimmed.StartsWith("//") || trimmed.StartsWith(";") || trimmed.EndsWith(":"))
                continue;
                
            EncodeInstruction(trimmed, currentAddress, code, labels);
            currentAddress += 4;
        }
        
        return code.ToArray();
    }
    
    private static void EncodeInstruction(string instruction, int currentAddress, List<byte> code, Dictionary<string, int> labels)
    {
        // Parse instruction
        var match = Regex.Match(instruction, @"^(\w+(?:\.\w+)?)\s*(.*)$");
        if (!match.Success)
            throw new ArgumentException($"Invalid instruction format: {instruction}");
        
        var opcode = match.Groups[1].Value.ToLower();
        var operands = match.Groups[2].Value;
        
        switch (opcode)
        {
            case "ret":
                EncodeRet(code);
                break;
            case "nop":
                EncodeNop(code);
                break;
            case "mov":
                EncodeMov(operands, code);
                break;
            case "add":
                EncodeAdd(operands, code);
                break;
            case "sub":
                EncodeSub(operands, code);
                break;
            case "mul":
                EncodeMul(operands, code);
                break;
            case "and":
                EncodeAnd(operands, code);
                break;
            case "orr":
                EncodeOrr(operands, code);
                break;
            case "eor":
                EncodeEor(operands, code);
                break;
            case "cmp":
                EncodeCmp(operands, code);
                break;
            case "b":
                EncodeB(operands, currentAddress, code, labels);
                break;
            case "b.eq":
                EncodeBCond(operands, 0b0000, currentAddress, code, labels); // EQ
                break;
            case "b.ne":
                EncodeBCond(operands, 0b0001, currentAddress, code, labels); // NE
                break;
            case "b.lt":
                EncodeBCond(operands, 0b1011, currentAddress, code, labels); // LT
                break;
            case "b.gt":
                EncodeBCond(operands, 0b1100, currentAddress, code, labels); // GT
                break;
            case "bl":
                EncodeBL(operands, currentAddress, code, labels);
                break;
            case "stp":
                EncodeStp(operands, code);
                break;
            case "ldp":
                EncodeLdp(operands, code);
                break;
            case "ldr":
                EncodeLdr(operands, code);
                break;
            case "str":
                EncodeStr(operands, code);
                break;
            default:
                throw new NotImplementedException($"Instruction not implemented: {opcode}");
        }
    }
    
    // Helper: Parse operands
    private static string[] ParseOperands(string operands)
    {
        var result = new List<string>();
        var parts = operands.Split(',');
        
        foreach (var part in parts)
        {
            result.Add(part.Trim());
        }
        
        return result.ToArray();
    }
    
    // Helper: Get register number
    private static int GetRegisterNumber(string reg)
    {
        reg = reg.Trim().ToLower();
        
        // Handle special registers
        if (reg == "sp") return 31;
        if (reg == "xzr" || reg == "wzr") return 31;
        
        // Handle x0-x30 and w0-w30
        if (reg.StartsWith("x") || reg.StartsWith("w"))
        {
            if (int.TryParse(reg.Substring(1), out int num))
            {
                if (num >= 0 && num <= 30)
                    return num;
            }
        }
        
        throw new ArgumentException($"Invalid register: {reg}");
    }
    
    // Helper: Check if register is 64-bit
    private static bool Is64BitRegister(string reg)
    {
        reg = reg.Trim().ToLower();
        return reg.StartsWith("x") || reg == "sp" || reg == "xzr";
    }
    
    // Helper: Add 32-bit instruction (little-endian)
    private static void AddInstruction(uint instruction, List<byte> code)
    {
        code.Add((byte)(instruction & 0xFF));
        code.Add((byte)((instruction >> 8) & 0xFF));
        code.Add((byte)((instruction >> 16) & 0xFF));
        code.Add((byte)((instruction >> 24) & 0xFF));
    }
    
    // RET: Return from subroutine
    // Encoding: 1101 0110 0101 1111 0000 0011 1100 0000
    // RET uses X30 (link register)
    private static void EncodeRet(List<byte> code)
    {
        AddInstruction(0xD65F03C0, code);
    }
    
    // NOP: No operation
    // Encoding: 1101 0101 0000 0011 0010 0000 0001 1111
    private static void EncodeNop(List<byte> code)
    {
        AddInstruction(0xD503201F, code);
    }
    
    // MOV: Move (register or immediate)
    private static void EncodeMov(string operands, List<byte> code)
    {
        var parts = ParseOperands(operands);
        if (parts.Length != 2)
            throw new ArgumentException($"MOV requires 2 operands: {operands}");
        
        var dst = parts[0];
        var src = parts[1];
        
        // Check if source is immediate
        if (src.StartsWith("#"))
        {
            // MOV with immediate (using MOVZ - Move wide with zero)
            var immStr = src.Substring(1);
            if (!int.TryParse(immStr, out int imm))
                throw new ArgumentException($"Invalid immediate: {src}");
            
            bool is64 = Is64BitRegister(dst);
            int rd = GetRegisterNumber(dst);
            
            // MOVZ encoding: sf=1 opc=10 10010 1 hw=00 imm16 Rd
            uint instruction = 0;
            instruction |= (uint)(is64 ? 1 : 0) << 31; // sf
            instruction |= 0b10u << 29; // opc = 10
            instruction |= 0b100101u << 23; // fixed bits
            instruction |= 0u << 21; // hw = 00 (shift 0)
            instruction |= ((uint)imm & 0xFFFF) << 5; // imm16
            instruction |= (uint)rd; // Rd
            
            AddInstruction(instruction, code);
        }
        else
        {
            // MOV register to register (using ORR with zero register)
            // ORR Rd, XZR, Rm (which is equivalent to MOV Rd, Rm)
            bool is64 = Is64BitRegister(dst);
            int rd = GetRegisterNumber(dst);
            int rm = GetRegisterNumber(src);
            
            // ORR (shifted register): sf=1 01 01010 shift=00 0 Rm 000000 Rn=31 Rd
            uint instruction = 0;
            instruction |= (uint)(is64 ? 1 : 0) << 31; // sf
            instruction |= 0b01u << 29; // opc
            instruction |= 0b01010u << 24; // fixed
            instruction |= 0b00u << 22; // shift = 00 (LSL)
            instruction |= 0u << 21; // N = 0
            instruction |= (uint)rm << 16; // Rm
            instruction |= 0u << 10; // imm6 = 000000
            instruction |= 31u << 5; // Rn = XZR
            instruction |= (uint)rd; // Rd
            
            AddInstruction(instruction, code);
        }
    }
    
    // ADD: Add (register or immediate)
    private static void EncodeAdd(string operands, List<byte> code)
    {
        var parts = ParseOperands(operands);
        if (parts.Length != 3)
            throw new ArgumentException($"ADD requires 3 operands: {operands}");
        
        var rd = parts[0];
        var rn = parts[1];
        var op2 = parts[2];
        
        bool is64 = Is64BitRegister(rd);
        int rdNum = GetRegisterNumber(rd);
        int rnNum = GetRegisterNumber(rn);
        
        if (op2.StartsWith("#"))
        {
            // ADD immediate
            var immStr = op2.Substring(1);
            if (!int.TryParse(immStr, out int imm))
                throw new ArgumentException($"Invalid immediate: {op2}");
            
            // ADD (immediate): sf 0 0 100010 shift imm12 Rn Rd
            uint instruction = 0;
            instruction |= (uint)(is64 ? 1 : 0) << 31; // sf
            instruction |= 0b00u << 29; // op
            instruction |= 0b100010u << 23; // fixed
            instruction |= 0u << 22; // shift = 0
            instruction |= ((uint)imm & 0xFFF) << 10; // imm12
            instruction |= (uint)rnNum << 5; // Rn
            instruction |= (uint)rdNum; // Rd
            
            AddInstruction(instruction, code);
        }
        else
        {
            // ADD register
            int rmNum = GetRegisterNumber(op2);
            
            // ADD (shifted register): sf 0 0 01011 shift 0 Rm imm6 Rn Rd
            uint instruction = 0;
            instruction |= (uint)(is64 ? 1 : 0) << 31; // sf
            instruction |= 0b00u << 29; // op
            instruction |= 0b01011u << 24; // fixed
            instruction |= 0b00u << 22; // shift = 00 (LSL)
            instruction |= 0u << 21; // N = 0
            instruction |= (uint)rmNum << 16; // Rm
            instruction |= 0u << 10; // imm6 = 0
            instruction |= (uint)rnNum << 5; // Rn
            instruction |= (uint)rdNum; // Rd
            
            AddInstruction(instruction, code);
        }
    }
    
    // SUB: Subtract (register or immediate)
    private static void EncodeSub(string operands, List<byte> code)
    {
        var parts = ParseOperands(operands);
        if (parts.Length != 3)
            throw new ArgumentException($"SUB requires 3 operands: {operands}");
        
        var rd = parts[0];
        var rn = parts[1];
        var op2 = parts[2];
        
        bool is64 = Is64BitRegister(rd);
        int rdNum = GetRegisterNumber(rd);
        int rnNum = GetRegisterNumber(rn);
        
        if (op2.StartsWith("#"))
        {
            // SUB immediate
            var immStr = op2.Substring(1);
            if (!int.TryParse(immStr, out int imm))
                throw new ArgumentException($"Invalid immediate: {op2}");
            
            // SUB (immediate): sf 1 0 100010 shift imm12 Rn Rd
            uint instruction = 0;
            instruction |= (uint)(is64 ? 1 : 0) << 31; // sf
            instruction |= 0b10u << 29; // op
            instruction |= 0b100010u << 23; // fixed
            instruction |= 0u << 22; // shift = 0
            instruction |= ((uint)imm & 0xFFF) << 10; // imm12
            instruction |= (uint)rnNum << 5; // Rn
            instruction |= (uint)rdNum; // Rd
            
            AddInstruction(instruction, code);
        }
        else
        {
            // SUB register
            int rmNum = GetRegisterNumber(op2);
            
            // SUB (shifted register): sf 1 0 01011 shift 0 Rm imm6 Rn Rd
            uint instruction = 0;
            instruction |= (uint)(is64 ? 1 : 0) << 31; // sf
            instruction |= 0b10u << 29; // op
            instruction |= 0b01011u << 24; // fixed
            instruction |= 0b00u << 22; // shift = 00 (LSL)
            instruction |= 0u << 21; // N = 0
            instruction |= (uint)rmNum << 16; // Rm
            instruction |= 0u << 10; // imm6 = 0
            instruction |= (uint)rnNum << 5; // Rn
            instruction |= (uint)rdNum; // Rd
            
            AddInstruction(instruction, code);
        }
    }
    
    // MUL: Multiply
    private static void EncodeMul(string operands, List<byte> code)
    {
        var parts = ParseOperands(operands);
        if (parts.Length != 3)
            throw new ArgumentException($"MUL requires 3 operands: {operands}");
        
        bool is64 = Is64BitRegister(parts[0]);
        int rd = GetRegisterNumber(parts[0]);
        int rn = GetRegisterNumber(parts[1]);
        int rm = GetRegisterNumber(parts[2]);
        
        // MADD with Ra = XZR: sf 0 0 11011 000 Rm 0 Ra=31 Rn Rd
        uint instruction = 0;
        instruction |= (uint)(is64 ? 1 : 0) << 31; // sf
        instruction |= 0b00u << 29; // op54
        instruction |= 0b11011u << 24; // fixed
        instruction |= 0b000u << 21; // op31
        instruction |= (uint)rm << 16; // Rm
        instruction |= 0u << 15; // o0
        instruction |= 31u << 10; // Ra = 31 (XZR)
        instruction |= (uint)rn << 5; // Rn
        instruction |= (uint)rd; // Rd
        
        AddInstruction(instruction, code);
    }
    
    // AND: Bitwise AND
    private static void EncodeAnd(string operands, List<byte> code)
    {
        var parts = ParseOperands(operands);
        if (parts.Length != 3)
            throw new ArgumentException($"AND requires 3 operands: {operands}");
        
        bool is64 = Is64BitRegister(parts[0]);
        int rd = GetRegisterNumber(parts[0]);
        int rn = GetRegisterNumber(parts[1]);
        int rm = GetRegisterNumber(parts[2]);
        
        // AND (shifted register): sf 00 01010 shift 0 Rm imm6 Rn Rd
        uint instruction = 0;
        instruction |= (uint)(is64 ? 1 : 0) << 31; // sf
        instruction |= 0b00u << 29; // opc
        instruction |= 0b01010u << 24; // fixed
        instruction |= 0b00u << 22; // shift = 00
        instruction |= 0u << 21; // N = 0
        instruction |= (uint)rm << 16; // Rm
        instruction |= 0u << 10; // imm6 = 0
        instruction |= (uint)rn << 5; // Rn
        instruction |= (uint)rd; // Rd
        
        AddInstruction(instruction, code);
    }
    
    // ORR: Bitwise OR
    private static void EncodeOrr(string operands, List<byte> code)
    {
        var parts = ParseOperands(operands);
        if (parts.Length != 3)
            throw new ArgumentException($"ORR requires 3 operands: {operands}");
        
        bool is64 = Is64BitRegister(parts[0]);
        int rd = GetRegisterNumber(parts[0]);
        int rn = GetRegisterNumber(parts[1]);
        int rm = GetRegisterNumber(parts[2]);
        
        // ORR (shifted register): sf 01 01010 shift 0 Rm imm6 Rn Rd
        uint instruction = 0;
        instruction |= (uint)(is64 ? 1 : 0) << 31; // sf
        instruction |= 0b01u << 29; // opc
        instruction |= 0b01010u << 24; // fixed
        instruction |= 0b00u << 22; // shift = 00
        instruction |= 0u << 21; // N = 0
        instruction |= (uint)rm << 16; // Rm
        instruction |= 0u << 10; // imm6 = 0
        instruction |= (uint)rn << 5; // Rn
        instruction |= (uint)rd; // Rd
        
        AddInstruction(instruction, code);
    }
    
    // EOR: Bitwise Exclusive OR
    private static void EncodeEor(string operands, List<byte> code)
    {
        var parts = ParseOperands(operands);
        if (parts.Length != 3)
            throw new ArgumentException($"EOR requires 3 operands: {operands}");
        
        bool is64 = Is64BitRegister(parts[0]);
        int rd = GetRegisterNumber(parts[0]);
        int rn = GetRegisterNumber(parts[1]);
        int rm = GetRegisterNumber(parts[2]);
        
        // EOR (shifted register): sf 10 01010 shift 0 Rm imm6 Rn Rd
        uint instruction = 0;
        instruction |= (uint)(is64 ? 1 : 0) << 31; // sf
        instruction |= 0b10u << 29; // opc
        instruction |= 0b01010u << 24; // fixed
        instruction |= 0b00u << 22; // shift = 00
        instruction |= 0u << 21; // N = 0
        instruction |= (uint)rm << 16; // Rm
        instruction |= 0u << 10; // imm6 = 0
        instruction |= (uint)rn << 5; // Rn
        instruction |= (uint)rd; // Rd
        
        AddInstruction(instruction, code);
    }
    
    // CMP: Compare (implemented as SUBS with Rd = XZR)
    private static void EncodeCmp(string operands, List<byte> code)
    {
        var parts = ParseOperands(operands);
        if (parts.Length != 2)
            throw new ArgumentException($"CMP requires 2 operands: {operands}");
        
        var rn = parts[0];
        var op2 = parts[1];
        
        bool is64 = Is64BitRegister(rn);
        int rnNum = GetRegisterNumber(rn);
        
        if (op2.StartsWith("#"))
        {
            // CMP immediate (SUBS with Rd = XZR)
            var immStr = op2.Substring(1);
            if (!int.TryParse(immStr, out int imm))
                throw new ArgumentException($"Invalid immediate: {op2}");
            
            // SUBS (immediate): sf 1 1 100010 shift imm12 Rn Rd=31
            uint instruction = 0;
            instruction |= (uint)(is64 ? 1 : 0) << 31; // sf
            instruction |= 0b11u << 29; // op (SUBS)
            instruction |= 0b100010u << 23; // fixed
            instruction |= 0u << 22; // shift = 0
            instruction |= ((uint)imm & 0xFFF) << 10; // imm12
            instruction |= (uint)rnNum << 5; // Rn
            instruction |= 31u; // Rd = XZR
            
            AddInstruction(instruction, code);
        }
        else
        {
            // CMP register (SUBS with Rd = XZR)
            int rmNum = GetRegisterNumber(op2);
            
            // SUBS (shifted register): sf 1 1 01011 shift 0 Rm imm6 Rn Rd=31
            uint instruction = 0;
            instruction |= (uint)(is64 ? 1 : 0) << 31; // sf
            instruction |= 0b11u << 29; // op (SUBS)
            instruction |= 0b01011u << 24; // fixed
            instruction |= 0b00u << 22; // shift = 00
            instruction |= 0u << 21; // N = 0
            instruction |= (uint)rmNum << 16; // Rm
            instruction |= 0u << 10; // imm6 = 0
            instruction |= (uint)rnNum << 5; // Rn
            instruction |= 31u; // Rd = XZR
            
            AddInstruction(instruction, code);
        }
    }
    
    // B: Unconditional branch
    private static void EncodeB(string operands, int currentAddress, List<byte> code, Dictionary<string, int> labels)
    {
        var label = operands.Trim();
        
        if (!labels.TryGetValue(label, out int targetAddress))
            throw new ArgumentException($"Undefined label: {label}");
        
        // Calculate PC-relative offset (in instructions, i.e., offset / 4)
        int offset = (targetAddress - currentAddress) / 4;
        
        // Validate 26-bit signed range: -2^25 to 2^25-1 (±128 MB / 4 = ±33,554,432 instructions)
        if (offset < -33554432 || offset > 33554431)
            throw new ArgumentException($"Branch offset {offset} instructions ({offset * 4} bytes) out of range for B instruction (±128 MB)");
        
        // B encoding: 0 00101 imm26
        uint instruction = 0;
        instruction |= 0b000101u << 26; // fixed
        instruction |= ((uint)offset & 0x3FFFFFF); // imm26
        
        AddInstruction(instruction, code);
    }
    
    // B.cond: Conditional branch
    private static void EncodeBCond(string operands, int cond, int currentAddress, List<byte> code, Dictionary<string, int> labels)
    {
        var label = operands.Trim();
        
        if (!labels.TryGetValue(label, out int targetAddress))
            throw new ArgumentException($"Undefined label: {label}");
        
        // Calculate PC-relative offset (in instructions)
        int offset = (targetAddress - currentAddress) / 4;
        
        // Validate 19-bit signed range: -2^18 to 2^18-1 (±1 MB / 4 = ±262,144 instructions)
        if (offset < -262144 || offset > 262143)
            throw new ArgumentException($"Conditional branch offset {offset} instructions ({offset * 4} bytes) out of range for B.cond instruction (±1 MB)");
        
        // B.cond encoding: 0101010 0 imm19 0 cond
        uint instruction = 0;
        instruction |= 0b0101010u << 25; // fixed
        instruction |= 0u << 24; // o1
        instruction |= ((uint)offset & 0x7FFFF) << 5; // imm19
        instruction |= 0u << 4; // o0
        instruction |= (uint)cond; // cond
        
        AddInstruction(instruction, code);
    }
    
    // BL: Branch with link
    private static void EncodeBL(string operands, int currentAddress, List<byte> code, Dictionary<string, int> labels)
    {
        var label = operands.Trim();
        
        if (!labels.TryGetValue(label, out int targetAddress))
            throw new ArgumentException($"Undefined label: {label}");
        
        // Calculate PC-relative offset (in instructions)
        int offset = (targetAddress - currentAddress) / 4;
        
        // Validate 26-bit signed range: -2^25 to 2^25-1 (±128 MB / 4 = ±33,554,432 instructions)
        if (offset < -33554432 || offset > 33554431)
            throw new ArgumentException($"BL offset {offset} instructions ({offset * 4} bytes) out of range (±128 MB)");
        
        // BL encoding: 1 00101 imm26
        uint instruction = 0;
        instruction |= 0b100101u << 26; // fixed (bit 31 = 1 for BL)
        instruction |= ((uint)offset & 0x3FFFFFF); // imm26
        
        AddInstruction(instruction, code);
    }
    
    // STP: Store pair of registers
    private static void EncodeStp(string operands, List<byte> code)
    {
        // Parse: stp Rt1, Rt2, [Rn, #imm]!  or  stp Rt1, Rt2, [Rn], #imm
        var match = Regex.Match(operands, @"([xw]\d+|sp|xzr|wzr)\s*,\s*([xw]\d+|sp|xzr|wzr)\s*,\s*\[([xw]\d+|sp)\s*,?\s*#?(-?\d+)\](!?)");
        if (!match.Success)
            throw new ArgumentException($"Invalid STP operands: {operands}");
        
        bool is64 = Is64BitRegister(match.Groups[1].Value);
        int rt1 = GetRegisterNumber(match.Groups[1].Value);
        int rt2 = GetRegisterNumber(match.Groups[2].Value);
        int rn = GetRegisterNumber(match.Groups[3].Value);
        int imm = int.Parse(match.Groups[4].Value);
        bool preIndex = match.Groups[5].Value == "!";
        
        // Offset must be scaled by size (8 for 64-bit, 4 for 32-bit)
        int scale = is64 ? 8 : 4;
        int imm7 = imm / scale;
        
        // Validate 7-bit signed range: -64 to +63
        if (imm7 < -64 || imm7 > 63)
            throw new ArgumentException($"STP offset {imm} bytes (scaled: {imm7}) out of 7-bit signed range (must be -64 to +63 after scaling by {scale})");
        
        // STP encoding: sf 0 101 0 010 imm7 Rt2 Rn Rt1
        // Addressing mode: 11 = pre-index, 10 = signed offset, 01 = post-index
        uint addrMode = preIndex ? 0b11u : 0b10u;
        
        uint instruction = 0;
        instruction |= (uint)(is64 ? 1 : 0) << 31; // sf
        instruction |= 0b0u << 30; // opc[1]
        instruction |= 0b101u << 27; // fixed
        instruction |= 0b0u << 26; // V
        instruction |= addrMode << 23; // addressing mode
        instruction |= 0b1u << 22; // L = 0 for store
        instruction |= ((uint)imm7 & 0x7F) << 15; // imm7
        instruction |= (uint)rt2 << 10; // Rt2
        instruction |= (uint)rn << 5; // Rn
        instruction |= (uint)rt1; // Rt1
        
        AddInstruction(instruction, code);
    }
    
    // LDP: Load pair of registers
    private static void EncodeLdp(string operands, List<byte> code)
    {
        // Parse: ldp Rt1, Rt2, [Rn, #imm]!  or  ldp Rt1, Rt2, [Rn], #imm
        var match = Regex.Match(operands, @"([xw]\d+|sp|xzr|wzr)\s*,\s*([xw]\d+|sp|xzr|wzr)\s*,\s*\[([xw]\d+|sp)\]\s*,?\s*#?(-?\d+)");
        if (!match.Success)
        {
            // Try pre-index form
            match = Regex.Match(operands, @"([xw]\d+|sp|xzr|wzr)\s*,\s*([xw]\d+|sp|xzr|wzr)\s*,\s*\[([xw]\d+|sp)\s*,?\s*#?(-?\d+)\]");
        }
        
        if (!match.Success)
            throw new ArgumentException($"Invalid LDP operands: {operands}");
        
        bool is64 = Is64BitRegister(match.Groups[1].Value);
        int rt1 = GetRegisterNumber(match.Groups[1].Value);
        int rt2 = GetRegisterNumber(match.Groups[2].Value);
        int rn = GetRegisterNumber(match.Groups[3].Value);
        int imm = int.Parse(match.Groups[4].Value);
        
        // Check if post-index (], #imm)
        bool postIndex = operands.Contains("],");
        
        // Offset must be scaled by size (8 for 64-bit, 4 for 32-bit)
        int scale = is64 ? 8 : 4;
        int imm7 = imm / scale;
        
        // Validate 7-bit signed range: -64 to +63
        if (imm7 < -64 || imm7 > 63)
            throw new ArgumentException($"LDP offset {imm} bytes (scaled: {imm7}) out of 7-bit signed range (must be -64 to +63 after scaling by {scale})");
        
        // LDP encoding: sf 0 101 0 011 imm7 Rt2 Rn Rt1
        // Addressing mode: 11 = pre-index, 10 = signed offset, 01 = post-index
        uint addrMode = postIndex ? 0b01u : 0b10u;
        
        uint instruction = 0;
        instruction |= (uint)(is64 ? 1 : 0) << 31; // sf
        instruction |= 0b0u << 30; // opc[1]
        instruction |= 0b101u << 27; // fixed
        instruction |= 0b0u << 26; // V
        instruction |= addrMode << 23; // addressing mode
        instruction |= 0b1u << 22; // L = 1 for load
        instruction |= ((uint)imm7 & 0x7F) << 15; // imm7
        instruction |= (uint)rt2 << 10; // Rt2
        instruction |= (uint)rn << 5; // Rn
        instruction |= (uint)rt1; // Rt1
        
        AddInstruction(instruction, code);
    }
    
    // LDR: Load register
    private static void EncodeLdr(string operands, List<byte> code)
    {
        // Parse: ldr Rt, [Rn, #imm]!  or  ldr Rt, [Rn], #imm
        var match = Regex.Match(operands, @"([xw]\d+|sp|xzr|wzr)\s*,\s*\[([xw]\d+|sp)\]\s*,?\s*#?(-?\d+)");
        if (!match.Success)
        {
            // Try pre-index or offset form
            match = Regex.Match(operands, @"([xw]\d+|sp|xzr|wzr)\s*,\s*\[([xw]\d+|sp)\s*,?\s*#?(-?\d+)\](!?)");
        }
        
        if (!match.Success)
            throw new ArgumentException($"Invalid LDR operands: {operands}");
        
        bool is64 = Is64BitRegister(match.Groups[1].Value);
        int rt = GetRegisterNumber(match.Groups[1].Value);
        int rn = GetRegisterNumber(match.Groups[2].Value);
        int imm = int.Parse(match.Groups[3].Value);
        bool preIndex = match.Groups.Count > 4 && match.Groups[4].Value == "!";
        bool postIndex = operands.Contains("],");
        
        // LDR (immediate): size 11 1 00 0 01 imm9 mode Rn Rt
        // mode: 00 = unscaled, 01 = post-index, 11 = pre-index, 10 = unsigned offset
        uint mode;
        if (postIndex)
            mode = 0b01u;
        else if (preIndex)
            mode = 0b11u;
        else
            mode = 0b01u; // Use post-index as default
        
        uint size = is64 ? 0b11u : 0b10u;
        
        uint instruction = 0;
        instruction |= size << 30; // size
        instruction |= 0b111u << 27; // fixed
        instruction |= 0b0u << 26; // V
        instruction |= 0b00u << 24; // opc
        instruction |= 0b0u << 22; // fixed
        instruction |= ((uint)imm & 0x1FF) << 12; // imm9
        instruction |= mode << 10; // mode
        instruction |= (uint)rn << 5; // Rn
        instruction |= (uint)rt; // Rt
        
        AddInstruction(instruction, code);
    }
    
    // STR: Store register
    private static void EncodeStr(string operands, List<byte> code)
    {
        // Parse: str Rt, [Rn, #imm]!  or  str Rt, [Rn], #imm
        var match = Regex.Match(operands, @"([xw]\d+|sp|xzr|wzr)\s*,\s*\[([xw]\d+|sp)\]\s*,?\s*#?(-?\d+)");
        if (!match.Success)
        {
            // Try pre-index or offset form
            match = Regex.Match(operands, @"([xw]\d+|sp|xzr|wzr)\s*,\s*\[([xw]\d+|sp)\s*,?\s*#?(-?\d+)\](!?)");
        }
        
        if (!match.Success)
            throw new ArgumentException($"Invalid STR operands: {operands}");
        
        bool is64 = Is64BitRegister(match.Groups[1].Value);
        int rt = GetRegisterNumber(match.Groups[1].Value);
        int rn = GetRegisterNumber(match.Groups[2].Value);
        int imm = int.Parse(match.Groups[3].Value);
        bool preIndex = match.Groups.Count > 4 && match.Groups[4].Value == "!";
        bool postIndex = operands.Contains("],");
        
        // STR (immediate): size 11 1 00 0 00 imm9 mode Rn Rt
        // mode: 01 = post-index, 11 = pre-index
        uint mode;
        if (postIndex)
            mode = 0b01u;
        else if (preIndex)
            mode = 0b11u;
        else
            mode = 0b01u; // Use post-index as default
        
        uint size = is64 ? 0b11u : 0b10u;
        
        uint instruction = 0;
        instruction |= size << 30; // size
        instruction |= 0b111u << 27; // fixed
        instruction |= 0b0u << 26; // V
        instruction |= 0b00u << 24; // opc
        instruction |= 0b0u << 22; // fixed
        instruction |= ((uint)imm & 0x1FF) << 12; // imm9
        instruction |= mode << 10; // mode
        instruction |= (uint)rn << 5; // Rn
        instruction |= (uint)rt; // Rt
        
        AddInstruction(instruction, code);
    }
}
