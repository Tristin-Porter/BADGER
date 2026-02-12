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

    public Map I32Add = "    pop {{r0}}\n    pop {{r1}}\n    add r0, r0, r1\n    push {{r0}}";
    public Map I32Sub = "    pop {{r1}}\n    pop {{r0}}\n    sub r0, r0, r1\n    push {{r0}}";
    public Map I32Mul = "    pop {{r0}}\n    pop {{r1}}\n    mul r0, r0, r1\n    push {{r0}}";
    public Map I32DivS = "    pop {{r1}}\n    pop {{r0}}\n    sdiv r0, r0, r1\n    push {{r0}}";
    public Map I32DivU = "    pop {{r1}}\n    pop {{r0}}\n    udiv r0, r0, r1\n    push {{r0}}";
    
    public Map I32Const = "    ldr r0, ={value}\n    push {{r0}}";
    
    // Logical operations
    public Map I32And = "    pop {{r0}}\n    pop {{r1}}\n    and r0, r0, r1\n    push {{r0}}";
    public Map I32Or = "    pop {{r0}}\n    pop {{r1}}\n    orr r0, r0, r1\n    push {{r0}}";
    public Map I32Xor = "    pop {{r0}}\n    pop {{r1}}\n    eor r0, r0, r1\n    push {{r0}}";
    
    // Comparison operations
    public Map I32Eq = "    pop {{r1}}\n    pop {{r0}}\n    cmp r0, r1\n    moveq r0, #1\n    movne r0, #0\n    push {{r0}}";
    public Map I32Ne = "    pop {{r1}}\n    pop {{r0}}\n    cmp r0, r1\n    movne r0, #1\n    moveq r0, #0\n    push {{r0}}";
    public Map I32LtS = "    pop {{r1}}\n    pop {{r0}}\n    cmp r0, r1\n    movlt r0, #1\n    movge r0, #0\n    push {{r0}}";
    public Map I32GtS = "    pop {{r1}}\n    pop {{r0}}\n    cmp r0, r1\n    movgt r0, #1\n    movle r0, #0\n    push {{r0}}";
    
    // Local variables
    public Map LocalGet = "    ldr r0, [r11, #-{offset}]\n    push {{r0}}";
    public Map LocalSet = "    pop {{r0}}\n    str r0, [r11, #-{offset}]";
    public Map LocalTee = "    pop {{r0}}\n    str r0, [r11, #-{offset}]\n    push {{r0}}";
    
    // Control flow
    public Map Return = "    b .function_exit";
    public Map Call = "    bl {funcidx}";
    public Map Br = "    b {labelidx}";
    public Map BrIf = "    pop {{r0}}\n    cmp r0, #0\n    bne {labelidx}";
    
    // Memory operations
    public Map I32Load = "    pop {{r0}}\n    ldr r1, [r0, #{offset}]\n    push {{r1}}";
    public Map I32Store = "    pop {{r1}}\n    pop {{r0}}\n    str r1, [r0, #{offset}]";
    
    // Stack operations
    public Map Drop = "    add sp, sp, #4";
    public Map Nop = "    nop";
}

// Part 2: ARM32 Assembler
public static class Assembler
{
    public static byte[] Assemble(string assemblyText)
    {
        var labels = new Dictionary<string, int>();
        var code = new List<byte>();
        
        var lines = assemblyText.Split('\n', StringSplitOptions.RemoveEmptyEntries);
        
        // First pass: collect labels
        // All ARM32 instructions are 4 bytes (fixed width)
        int address = 0;
        foreach (var line in lines)
        {
            var trimmed = line.Trim();
            if (string.IsNullOrEmpty(trimmed) || trimmed.StartsWith("//") || trimmed.StartsWith(";") || trimmed.StartsWith("@"))
                continue;
                
            if (trimmed.EndsWith(":"))
            {
                var label = trimmed.TrimEnd(':');
                labels[label] = address;
            }
            else
            {
                // All ARM32 instructions are 4 bytes
                address += 4;
            }
        }
        
        // Second pass: encode instructions
        int currentAddress = 0;
        foreach (var line in lines)
        {
            var trimmed = line.Trim();
            if (string.IsNullOrEmpty(trimmed) || trimmed.StartsWith("//") || trimmed.StartsWith(";") || trimmed.StartsWith("@") || trimmed.EndsWith(":"))
                continue;
                
            EncodeInstruction(trimmed, currentAddress, code, labels);
            currentAddress += 4;
        }
        
        return code.ToArray();
    }
    
    private static void EncodeInstruction(string instruction, int currentAddress, List<byte> code, Dictionary<string, int> labels)
    {
        // Parse instruction
        var match = System.Text.RegularExpressions.Regex.Match(instruction, @"^(\w+)\s*(.*)$");
        if (!match.Success)
            throw new ArgumentException($"Invalid instruction format: {instruction}");
        
        var opcode = match.Groups[1].Value.ToLower();
        var operands = match.Groups[2].Value;
        
        switch (opcode)
        {
            case "bx":
                EncodeBx(operands, code);
                break;
            case "nop":
                EncodeNop(code);
                break;
            case "mov":
                EncodeMov(operands, code);
                break;
            case "moveq":
                EncodeMovCond(operands, 0x0, code); // EQ condition
                break;
            case "movne":
                EncodeMovCond(operands, 0x1, code); // NE condition
                break;
            case "movlt":
                EncodeMovCond(operands, 0xB, code); // LT condition
                break;
            case "movge":
                EncodeMovCond(operands, 0xA, code); // GE condition
                break;
            case "movgt":
                EncodeMovCond(operands, 0xC, code); // GT condition
                break;
            case "movle":
                EncodeMovCond(operands, 0xD, code); // LE condition
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
            case "sdiv":
                EncodeSdiv(operands, code);
                break;
            case "udiv":
                EncodeUdiv(operands, code);
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
            case "beq":
                EncodeBCond(operands, 0x0, currentAddress, code, labels); // EQ
                break;
            case "bne":
                EncodeBCond(operands, 0x1, currentAddress, code, labels); // NE
                break;
            case "blt":
                EncodeBCond(operands, 0xB, currentAddress, code, labels); // LT
                break;
            case "bgt":
                EncodeBCond(operands, 0xC, currentAddress, code, labels); // GT
                break;
            case "bl":
                EncodeBL(operands, currentAddress, code, labels);
                break;
            case "push":
                EncodePush(operands, code);
                break;
            case "pop":
                EncodePop(operands, code);
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
        // Handle register lists like {r0, r1}
        if (operands.Contains("{"))
        {
            return new[] { operands };
        }
        
        // Handle bracket expressions (don't split inside brackets)
        if (operands.Contains("["))
        {
            var result = new List<string>();
            int bracketDepth = 0;
            int start = 0;
            
            for (int i = 0; i < operands.Length; i++)
            {
                if (operands[i] == '[')
                    bracketDepth++;
                else if (operands[i] == ']')
                    bracketDepth--;
                else if (operands[i] == ',' && bracketDepth == 0)
                {
                    result.Add(operands.Substring(start, i - start).Trim());
                    start = i + 1;
                }
            }
            result.Add(operands.Substring(start).Trim());
            return result.ToArray();
        }
        
        var parts = operands.Split(',');
        var list = new List<string>();
        
        foreach (var part in parts)
        {
            list.Add(part.Trim());
        }
        
        return list.ToArray();
    }
    
    // Helper: Get register number
    private static int GetRegisterNumber(string reg)
    {
        reg = reg.Trim().ToLower();
        
        // Handle special registers
        if (reg == "sp") return 13;
        if (reg == "lr") return 14;
        if (reg == "pc") return 15;
        
        // Handle r0-r15
        if (reg.StartsWith("r"))
        {
            if (int.TryParse(reg.Substring(1), out int num))
            {
                if (num >= 0 && num <= 15)
                    return num;
            }
        }
        
        throw new ArgumentException($"Invalid register: {reg}");
    }
    
    // Helper: Parse register list (e.g., {r0, r1, r2})
    private static uint ParseRegisterList(string regList)
    {
        regList = regList.Trim();
        if (!regList.StartsWith("{") || !regList.EndsWith("}"))
            throw new ArgumentException($"Invalid register list format: {regList}");
        
        var regsStr = regList.Substring(1, regList.Length - 2);
        var regs = regsStr.Split(',');
        
        uint mask = 0;
        foreach (var reg in regs)
        {
            int regNum = GetRegisterNumber(reg.Trim());
            mask |= (uint)(1 << regNum);
        }
        
        return mask;
    }
    
    // Helper: Add 32-bit instruction (little-endian)
    private static void AddInstruction(uint instruction, List<byte> code)
    {
        code.Add((byte)(instruction & 0xFF));
        code.Add((byte)((instruction >> 8) & 0xFF));
        code.Add((byte)((instruction >> 16) & 0xFF));
        code.Add((byte)((instruction >> 24) & 0xFF));
    }
    
    // Helper: Encode ARM32 immediate with rotation
    // ARM32 uses 8-bit immediate + 4-bit rotation (even values only)
    private static bool TryEncodeImmediate(int value, out uint encoded)
    {
        // Try to find a rotation that works
        uint uval = (uint)value;
        
        // Try all even rotation amounts (0, 2, 4, ..., 30)
        for (int rot = 0; rot < 32; rot += 2)
        {
            uint rotated = (uval >> rot) | (uval << (32 - rot));
            if (rotated <= 0xFF)
            {
                // Found valid encoding: 4-bit rotation (divided by 2) + 8-bit immediate
                encoded = ((uint)(rot / 2) << 8) | (rotated & 0xFF);
                return true;
            }
        }
        
        encoded = 0;
        return false;
    }
    
    // BX: Branch and exchange
    // BX Rm: 1110 0001 0010 1111 1111 1111 0001 Rm
    private static void EncodeBx(string operands, List<byte> code)
    {
        var parts = ParseOperands(operands);
        if (parts.Length != 1)
            throw new ArgumentException($"BX requires 1 operand: {operands}");
        
        int rm = GetRegisterNumber(parts[0]);
        
        // BX encoding: cond=1110 0001 0010 1111 1111 1111 0001 Rm
        uint instruction = 0xE12FFF10 | (uint)rm;
        
        AddInstruction(instruction, code);
    }
    
    // NOP: No operation (MOV r0, r0)
    // NOP: 1110 0001 1010 0000 0000 0000 0000 0000
    private static void EncodeNop(List<byte> code)
    {
        // NOP is encoded as MOV r0, r0
        AddInstruction(0xE1A00000, code);
    }
    
    // MOV: Move (register or immediate)
    private static void EncodeMov(string operands, List<byte> code)
    {
        var parts = ParseOperands(operands);
        if (parts.Length != 2)
            throw new ArgumentException($"MOV requires 2 operands: {operands}");
        
        var dst = parts[0];
        var src = parts[1];
        
        int rd = GetRegisterNumber(dst);
        
        // Check if source is immediate
        if (src.StartsWith("#"))
        {
            // MOV with immediate
            var immStr = src.Substring(1);
            if (!int.TryParse(immStr, out int imm))
                throw new ArgumentException($"Invalid immediate: {src}");
            
            if (!TryEncodeImmediate(imm, out uint encoded))
                throw new ArgumentException($"Immediate value {imm} cannot be encoded in ARM32 MOV");
            
            // MOV (immediate): cond=1110 001 1101 S=0 Rn=0000 Rd imm12
            uint instruction = 0xE3A00000;
            instruction |= (uint)rd << 12; // Rd
            instruction |= encoded; // imm12 (4-bit rotate + 8-bit immediate)
            
            AddInstruction(instruction, code);
        }
        else
        {
            // MOV register to register
            int rm = GetRegisterNumber(src);
            
            // MOV (register): cond=1110 000 1101 S=0 Rn=0000 Rd imm5=00000 shift=00 0 Rm
            uint instruction = 0xE1A00000;
            instruction |= (uint)rd << 12; // Rd
            instruction |= (uint)rm; // Rm
            
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
        
        int rdNum = GetRegisterNumber(rd);
        int rnNum = GetRegisterNumber(rn);
        
        if (op2.StartsWith("#"))
        {
            // ADD immediate
            var immStr = op2.Substring(1);
            if (!int.TryParse(immStr, out int imm))
                throw new ArgumentException($"Invalid immediate: {op2}");
            
            if (!TryEncodeImmediate(imm, out uint encoded))
                throw new ArgumentException($"Immediate value {imm} cannot be encoded in ARM32 ADD");
            
            // ADD (immediate): cond=1110 001 0100 S=0 Rn Rd imm12
            uint instruction = 0xE2800000;
            instruction |= (uint)rnNum << 16; // Rn
            instruction |= (uint)rdNum << 12; // Rd
            instruction |= encoded; // imm12
            
            AddInstruction(instruction, code);
        }
        else
        {
            // ADD register
            int rmNum = GetRegisterNumber(op2);
            
            // ADD (register): cond=1110 000 0100 S=0 Rn Rd imm5=00000 shift=00 0 Rm
            uint instruction = 0xE0800000;
            instruction |= (uint)rnNum << 16; // Rn
            instruction |= (uint)rdNum << 12; // Rd
            instruction |= (uint)rmNum; // Rm
            
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
        
        int rdNum = GetRegisterNumber(rd);
        int rnNum = GetRegisterNumber(rn);
        
        if (op2.StartsWith("#"))
        {
            // SUB immediate
            var immStr = op2.Substring(1);
            if (!int.TryParse(immStr, out int imm))
                throw new ArgumentException($"Invalid immediate: {op2}");
            
            if (!TryEncodeImmediate(imm, out uint encoded))
                throw new ArgumentException($"Immediate value {imm} cannot be encoded in ARM32 SUB");
            
            // SUB (immediate): cond=1110 001 0010 S=0 Rn Rd imm12
            uint instruction = 0xE2400000;
            instruction |= (uint)rnNum << 16; // Rn
            instruction |= (uint)rdNum << 12; // Rd
            instruction |= encoded; // imm12
            
            AddInstruction(instruction, code);
        }
        else
        {
            // SUB register
            int rmNum = GetRegisterNumber(op2);
            
            // SUB (register): cond=1110 000 0010 S=0 Rn Rd imm5=00000 shift=00 0 Rm
            uint instruction = 0xE0400000;
            instruction |= (uint)rnNum << 16; // Rn
            instruction |= (uint)rdNum << 12; // Rd
            instruction |= (uint)rmNum; // Rm
            
            AddInstruction(instruction, code);
        }
    }
    
    // MUL: Multiply
    private static void EncodeMul(string operands, List<byte> code)
    {
        var parts = ParseOperands(operands);
        if (parts.Length != 3)
            throw new ArgumentException($"MUL requires 3 operands: {operands}");
        
        int rd = GetRegisterNumber(parts[0]);
        int rn = GetRegisterNumber(parts[1]);
        int rm = GetRegisterNumber(parts[2]);
        
        // MUL encoding: cond=1110 000000 S=0 Rd 0000 Rm 1001 Rn
        uint instruction = 0xE0000090;
        instruction |= (uint)rd << 16; // Rd
        instruction |= (uint)rm << 8; // Rm
        instruction |= (uint)rn; // Rn
        
        AddInstruction(instruction, code);
    }
    
    // SDIV: Signed divide (ARMv7-A and later)
    private static void EncodeSdiv(string operands, List<byte> code)
    {
        var parts = ParseOperands(operands);
        if (parts.Length != 3)
            throw new ArgumentException($"SDIV requires 3 operands: {operands}");
        
        int rd = GetRegisterNumber(parts[0]);
        int rn = GetRegisterNumber(parts[1]);
        int rm = GetRegisterNumber(parts[2]);
        
        // SDIV encoding: cond=1110 0111 0001 Rd 1111 Rm 0001 Rn
        uint instruction = 0xE710F010;
        instruction |= (uint)rd << 16; // Rd
        instruction |= (uint)rm << 8; // Rm
        instruction |= (uint)rn; // Rn
        
        AddInstruction(instruction, code);
    }
    
    // UDIV: Unsigned divide (ARMv7-A and later)
    private static void EncodeUdiv(string operands, List<byte> code)
    {
        var parts = ParseOperands(operands);
        if (parts.Length != 3)
            throw new ArgumentException($"UDIV requires 3 operands: {operands}");
        
        int rd = GetRegisterNumber(parts[0]);
        int rn = GetRegisterNumber(parts[1]);
        int rm = GetRegisterNumber(parts[2]);
        
        // UDIV encoding: cond=1110 0111 0011 Rd 1111 Rm 0001 Rn
        uint instruction = 0xE730F010;
        instruction |= (uint)rd << 16; // Rd
        instruction |= (uint)rm << 8; // Rm
        instruction |= (uint)rn; // Rn
        
        AddInstruction(instruction, code);
    }
    
    // MOV with condition: Conditional move
    private static void EncodeMovCond(string operands, uint cond, List<byte> code)
    {
        var parts = ParseOperands(operands);
        if (parts.Length != 2)
            throw new ArgumentException($"Conditional MOV requires 2 operands: {operands}");
        
        var dst = parts[0];
        var src = parts[1];
        
        int rd = GetRegisterNumber(dst);
        
        // Only support immediate moves for conditional moves
        if (!src.StartsWith("#"))
            throw new ArgumentException($"Conditional MOV only supports immediate values: {src}");
        
        var immStr = src.Substring(1);
        if (!int.TryParse(immStr, out int imm))
            throw new ArgumentException($"Invalid immediate: {src}");
        
        if (!TryEncodeImmediate(imm, out uint encoded))
            throw new ArgumentException($"Immediate value {imm} cannot be encoded in ARM32 MOV");
        
        // MOV (immediate) with condition: cond 001 1101 S=0 Rn=0000 Rd imm12
        uint instruction = (cond << 28) | 0x03A00000;
        instruction |= (uint)rd << 12; // Rd
        instruction |= encoded; // imm12 (4-bit rotate + 8-bit immediate)
        
        AddInstruction(instruction, code);
    }
    
    // AND: Bitwise AND
    private static void EncodeAnd(string operands, List<byte> code)
    {
        var parts = ParseOperands(operands);
        if (parts.Length != 3)
            throw new ArgumentException($"AND requires 3 operands: {operands}");
        
        int rd = GetRegisterNumber(parts[0]);
        int rn = GetRegisterNumber(parts[1]);
        int rm = GetRegisterNumber(parts[2]);
        
        // AND (register): cond=1110 000 0000 S=0 Rn Rd imm5=00000 shift=00 0 Rm
        uint instruction = 0xE0000000;
        instruction |= (uint)rn << 16; // Rn
        instruction |= (uint)rd << 12; // Rd
        instruction |= (uint)rm; // Rm
        
        AddInstruction(instruction, code);
    }
    
    // ORR: Bitwise OR
    private static void EncodeOrr(string operands, List<byte> code)
    {
        var parts = ParseOperands(operands);
        if (parts.Length != 3)
            throw new ArgumentException($"ORR requires 3 operands: {operands}");
        
        int rd = GetRegisterNumber(parts[0]);
        int rn = GetRegisterNumber(parts[1]);
        int rm = GetRegisterNumber(parts[2]);
        
        // ORR (register): cond=1110 000 1100 S=0 Rn Rd imm5=00000 shift=00 0 Rm
        uint instruction = 0xE1800000;
        instruction |= (uint)rn << 16; // Rn
        instruction |= (uint)rd << 12; // Rd
        instruction |= (uint)rm; // Rm
        
        AddInstruction(instruction, code);
    }
    
    // EOR: Bitwise XOR
    private static void EncodeEor(string operands, List<byte> code)
    {
        var parts = ParseOperands(operands);
        if (parts.Length != 3)
            throw new ArgumentException($"EOR requires 3 operands: {operands}");
        
        int rd = GetRegisterNumber(parts[0]);
        int rn = GetRegisterNumber(parts[1]);
        int rm = GetRegisterNumber(parts[2]);
        
        // EOR (register): cond=1110 000 0001 S=0 Rn Rd imm5=00000 shift=00 0 Rm
        uint instruction = 0xE0200000;
        instruction |= (uint)rn << 16; // Rn
        instruction |= (uint)rd << 12; // Rd
        instruction |= (uint)rm; // Rm
        
        AddInstruction(instruction, code);
    }
    
    // CMP: Compare (implemented as SUBS with Rd = 0, S = 1)
    private static void EncodeCmp(string operands, List<byte> code)
    {
        var parts = ParseOperands(operands);
        if (parts.Length != 2)
            throw new ArgumentException($"CMP requires 2 operands: {operands}");
        
        var rn = parts[0];
        var op2 = parts[1];
        
        int rnNum = GetRegisterNumber(rn);
        
        if (op2.StartsWith("#"))
        {
            // CMP immediate
            var immStr = op2.Substring(1);
            if (!int.TryParse(immStr, out int imm))
                throw new ArgumentException($"Invalid immediate: {op2}");
            
            if (!TryEncodeImmediate(imm, out uint encoded))
                throw new ArgumentException($"Immediate value {imm} cannot be encoded in ARM32 CMP");
            
            // CMP (immediate): cond=1110 001 0101 S=1 Rn Rd=0000 imm12
            uint instruction = 0xE3500000;
            instruction |= (uint)rnNum << 16; // Rn
            instruction |= encoded; // imm12
            
            AddInstruction(instruction, code);
        }
        else
        {
            // CMP register
            int rmNum = GetRegisterNumber(op2);
            
            // CMP (register): cond=1110 000 0101 S=1 Rn Rd=0000 imm5=00000 shift=00 0 Rm
            uint instruction = 0xE1500000;
            instruction |= (uint)rnNum << 16; // Rn
            instruction |= (uint)rmNum; // Rm
            
            AddInstruction(instruction, code);
        }
    }
    
    // B: Unconditional branch
    private static void EncodeB(string operands, int currentAddress, List<byte> code, Dictionary<string, int> labels)
    {
        var label = operands.Trim();
        
        if (!labels.TryGetValue(label, out int targetAddress))
            throw new ArgumentException($"Undefined label: {label}");
        
        // Calculate PC-relative offset
        // In ARM mode, PC is current instruction + 8
        int offset = targetAddress - (currentAddress + 8);
        
        // Offset is in bytes, but encoded as word offset (divide by 4)
        int wordOffset = offset / 4;
        
        // Validate 24-bit signed range
        if (wordOffset < -8388608 || wordOffset > 8388607)
            throw new ArgumentException($"Branch offset {offset} bytes out of range for B instruction");
        
        // B encoding: cond=1110 101 L=0 offset24
        uint instruction = 0xEA000000;
        instruction |= (uint)wordOffset & 0xFFFFFF; // offset24
        
        AddInstruction(instruction, code);
    }
    
    // B.cond: Conditional branch
    private static void EncodeBCond(string operands, int cond, int currentAddress, List<byte> code, Dictionary<string, int> labels)
    {
        var label = operands.Trim();
        
        if (!labels.TryGetValue(label, out int targetAddress))
            throw new ArgumentException($"Undefined label: {label}");
        
        // Calculate PC-relative offset
        // In ARM mode, PC is current instruction + 8
        int offset = targetAddress - (currentAddress + 8);
        
        // Offset is in bytes, but encoded as word offset (divide by 4)
        int wordOffset = offset / 4;
        
        // Validate 24-bit signed range
        if (wordOffset < -8388608 || wordOffset > 8388607)
            throw new ArgumentException($"Conditional branch offset {offset} bytes out of range");
        
        // B.cond encoding: cond 101 L=0 offset24
        uint instruction = ((uint)cond << 28) | 0x0A000000;
        instruction |= (uint)wordOffset & 0xFFFFFF; // offset24
        
        AddInstruction(instruction, code);
    }
    
    // BL: Branch with link
    private static void EncodeBL(string operands, int currentAddress, List<byte> code, Dictionary<string, int> labels)
    {
        var label = operands.Trim();
        
        if (!labels.TryGetValue(label, out int targetAddress))
            throw new ArgumentException($"Undefined label: {label}");
        
        // Calculate PC-relative offset
        // In ARM mode, PC is current instruction + 8
        int offset = targetAddress - (currentAddress + 8);
        
        // Offset is in bytes, but encoded as word offset (divide by 4)
        int wordOffset = offset / 4;
        
        // Validate 24-bit signed range
        if (wordOffset < -8388608 || wordOffset > 8388607)
            throw new ArgumentException($"BL offset {offset} bytes out of range");
        
        // BL encoding: cond=1110 101 L=1 offset24
        uint instruction = 0xEB000000;
        instruction |= (uint)wordOffset & 0xFFFFFF; // offset24
        
        AddInstruction(instruction, code);
    }
    
    // PUSH: Push registers onto stack
    private static void EncodePush(string operands, List<byte> code)
    {
        uint regList = ParseRegisterList(operands);
        
        // PUSH is encoded as STMDB (store multiple decrement before) sp!, {reglist}
        // STMDB sp!, {reglist}: cond=1110 100 P=1 U=0 S=0 W=1 L=0 Rn=1101 register_list
        uint instruction = 0xE92D0000;
        instruction |= regList; // register_list
        
        AddInstruction(instruction, code);
    }
    
    // POP: Pop registers from stack
    private static void EncodePop(string operands, List<byte> code)
    {
        uint regList = ParseRegisterList(operands);
        
        // POP is encoded as LDMIA (load multiple increment after) sp!, {reglist}
        // LDMIA sp!, {reglist}: cond=1110 100 P=0 U=1 S=0 W=1 L=1 Rn=1101 register_list
        uint instruction = 0xE8BD0000;
        instruction |= regList; // register_list
        
        AddInstruction(instruction, code);
    }
    
    // LDR: Load register
    private static void EncodeLdr(string operands, List<byte> code)
    {
        var parts = ParseOperands(operands);
        if (parts.Length < 2)
            throw new ArgumentException($"LDR requires at least 2 operands: {operands}");
        
        int rt = GetRegisterNumber(parts[0]);
        var addrMode = parts[1];
        
        // Handle different addressing modes
        // LDR Rt, [Rn, #offset]
        var match = System.Text.RegularExpressions.Regex.Match(addrMode, @"^\[([^,\]]+)(?:,\s*#([+-]?\d+))?\](!)?$");
        if (match.Success)
        {
            int rn = GetRegisterNumber(match.Groups[1].Value);
            int offset = 0;
            bool writeback = match.Groups[3].Success;
            
            if (match.Groups[2].Success)
            {
                if (!int.TryParse(match.Groups[2].Value, out offset))
                    throw new ArgumentException($"Invalid offset: {match.Groups[2].Value}");
            }
            
            bool addOffset = offset >= 0;
            uint absOffset = (uint)Math.Abs(offset);
            
            if (absOffset > 4095)
                throw new ArgumentException($"LDR offset {offset} out of range (max ±4095)");
            
            // LDR (immediate): cond=1110 01 I=0 P=1 U Rn Rt imm12
            uint instruction = 0xE5100000;
            instruction |= (uint)(addOffset ? 1 : 0) << 23; // U (add/subtract)
            instruction |= (uint)rn << 16; // Rn
            instruction |= (uint)rt << 12; // Rt
            instruction |= absOffset; // imm12
            
            if (writeback)
            {
                instruction |= 1u << 21; // W (writeback)
            }
            
            AddInstruction(instruction, code);
        }
        // LDR Rt, =value (literal pool / pseudo-instruction)
        else if (addrMode.StartsWith("="))
        {
            // For now, encode as a simple PC-relative load with offset 0
            // In a full implementation, this would use a literal pool
            var valueStr = addrMode.Substring(1);
            if (!int.TryParse(valueStr, out int value))
                throw new ArgumentException($"Invalid literal value: {valueStr}");
            
            // Use MOV if possible, otherwise would need literal pool
            if (TryEncodeImmediate(value, out uint encoded))
            {
                // Encode as MOV instead
                uint instruction = 0xE3A00000;
                instruction |= (uint)rt << 12; // Rd
                instruction |= encoded; // imm12
                AddInstruction(instruction, code);
            }
            else
            {
                // For values that can't be encoded, use a simple PC-relative LDR
                // This is a simplification - a full implementation would use a literal pool
                uint instruction = 0xE51F0000; // LDR Rt, [PC, #-0]
                instruction |= (uint)rt << 12; // Rt
                AddInstruction(instruction, code);
            }
        }
        else
        {
            throw new ArgumentException($"Unsupported LDR addressing mode: {addrMode}");
        }
    }
    
    // STR: Store register
    private static void EncodeStr(string operands, List<byte> code)
    {
        var parts = ParseOperands(operands);
        if (parts.Length != 2)
            throw new ArgumentException($"STR requires 2 operands: {operands}");
        
        int rt = GetRegisterNumber(parts[0]);
        var addrMode = parts[1];
        
        // Handle addressing mode: STR Rt, [Rn, #offset]
        var match = System.Text.RegularExpressions.Regex.Match(addrMode, @"^\[([^,\]]+)(?:,\s*#([+-]?\d+))?\](!)?$");
        if (!match.Success)
            throw new ArgumentException($"Unsupported STR addressing mode: {addrMode}");
        
        int rn = GetRegisterNumber(match.Groups[1].Value);
        int offset = 0;
        bool writeback = match.Groups[3].Success;
        
        if (match.Groups[2].Success)
        {
            if (!int.TryParse(match.Groups[2].Value, out offset))
                throw new ArgumentException($"Invalid offset: {match.Groups[2].Value}");
        }
        
        bool addOffset = offset >= 0;
        uint absOffset = (uint)Math.Abs(offset);
        
        if (absOffset > 4095)
            throw new ArgumentException($"STR offset {offset} out of range (max ±4095)");
        
        // STR (immediate): cond=1110 01 I=0 P=1 U Rn Rt imm12
        uint instruction = 0xE5000000;
        instruction |= (uint)(addOffset ? 1 : 0) << 23; // U (add/subtract)
        instruction |= (uint)rn << 16; // Rn
        instruction |= (uint)rt << 12; // Rt
        instruction |= absOffset; // imm12
        
        if (writeback)
        {
            instruction |= 1u << 21; // W (writeback)
        }
        
        AddInstruction(instruction, code);
    }
}
