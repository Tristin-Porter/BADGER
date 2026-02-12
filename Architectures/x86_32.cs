using CDTk;
using System;
using System.Collections.Generic;

namespace Badger.Architectures.x86_32;

// Part 1: CDTk MapSet for WAT → x86_32 assembly translation
public class WATToX86_32MapSet : MapSet
{
    public Map Module = @"
; x86_32 Assembly
; Module: {id}
{fields}
";

    public Map Function = @"
{id}:
    push ebp
    mov ebp, esp
    sub esp, {local_space}
{body}
    mov esp, ebp
    pop ebp
    ret
";

    // Instruction mappings - Numeric operations
    public Map I32Add = "    pop eax\n    pop ebx\n    add eax, ebx\n    push eax";
    public Map I32Sub = "    pop ebx\n    pop eax\n    sub eax, ebx\n    push eax";
    public Map I32Mul = "    pop eax\n    pop ebx\n    imul eax, ebx\n    push eax";
    public Map I32DivS = "    pop ebx\n    pop eax\n    cdq\n    idiv ebx\n    push eax";
    public Map I32DivU = "    pop ebx\n    pop eax\n    xor edx, edx\n    div ebx\n    push eax";
    
    public Map I32Const = "    mov eax, {value}\n    push eax";
    
    // Logical operations
    public Map I32And = "    pop eax\n    pop ebx\n    and eax, ebx\n    push eax";
    public Map I32Or = "    pop eax\n    pop ebx\n    or eax, ebx\n    push eax";
    public Map I32Xor = "    pop eax\n    pop ebx\n    xor eax, ebx\n    push eax";
    
    // Comparison operations
    public Map I32Eq = "    pop ebx\n    pop eax\n    cmp eax, ebx\n    sete al\n    movzx eax, al\n    push eax";
    public Map I32Ne = "    pop ebx\n    pop eax\n    cmp eax, ebx\n    setne al\n    movzx eax, al\n    push eax";
    public Map I32LtS = "    pop ebx\n    pop eax\n    cmp eax, ebx\n    setl al\n    movzx eax, al\n    push eax";
    public Map I32GtS = "    pop ebx\n    pop eax\n    cmp eax, ebx\n    setg al\n    movzx eax, al\n    push eax";
    
    // Local variables
    public Map LocalGet = "    mov eax, [ebp - {offset}]\n    push eax";
    public Map LocalSet = "    pop eax\n    mov [ebp - {offset}], eax";
    public Map LocalTee = "    pop eax\n    mov [ebp - {offset}], eax\n    push eax";
    
    // Control flow
    public Map Return = "    jmp .function_exit";
    public Map Call = "    call {funcidx}";
    public Map Br = "    jmp {labelidx}";
    public Map BrIf = "    pop eax\n    test eax, eax\n    jnz {labelidx}";
    
    // Memory operations
    public Map I32Load = "    pop eax\n    mov eax, [eax + {offset}]\n    push eax";
    public Map I32Store = "    pop ebx\n    pop eax\n    mov [eax + {offset}], ebx";
    
    // Stack operations
    public Map Drop = "    add esp, 4";
    public Map Nop = "    nop";
}

// Part 2: x86_32 Assembler - converts assembly text to machine code
public static class Assembler
{
    private static Dictionary<string, int> labels = new Dictionary<string, int>();
    private static List<byte> code = new List<byte>();
    
    public static byte[] Assemble(string assemblyText)
    {
        labels.Clear();
        code.Clear();
        
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
                
            EncodeInstruction(trimmed);
        }
        
        return code.ToArray();
    }
    
    private static int EstimateInstructionSize(string instruction)
    {
        // Simplified size estimation for 32-bit
        if (instruction.StartsWith("mov")) return 3;
        if (instruction.StartsWith("push") || instruction.StartsWith("pop")) return 1;
        if (instruction.StartsWith("add") || instruction.StartsWith("sub")) return 3;
        if (instruction.StartsWith("call") || instruction.StartsWith("jmp")) return 5;
        return 2;
    }
    
    private static void EncodeInstruction(string instruction)
    {
        var parts = instruction.Split(new[] { ' ', ',' }, StringSplitOptions.RemoveEmptyEntries);
        var opcode = parts[0].ToLower();
        
        switch (opcode)
        {
            case "push":
                EncodePush(parts[1]);
                break;
            case "pop":
                EncodePop(parts[1]);
                break;
            case "mov":
                EncodeMov(parts[1], parts[2]);
                break;
            case "add":
                EncodeAdd(parts[1], parts[2]);
                break;
            case "sub":
                EncodeSub(parts[1], parts[2]);
                break;
            case "imul":
                EncodeIMul(parts[1], parts[2]);
                break;
            case "idiv":
                EncodeIDiv(parts[1]);
                break;
            case "div":
                EncodeDiv(parts[1]);
                break;
            case "and":
                EncodeAnd(parts[1], parts[2]);
                break;
            case "or":
                EncodeOr(parts[1], parts[2]);
                break;
            case "xor":
                EncodeXor(parts[1], parts[2]);
                break;
            case "cmp":
                EncodeCmp(parts[1], parts[2]);
                break;
            case "test":
                EncodeTest(parts[1], parts[2]);
                break;
            case "jmp":
                EncodeJmp(parts[1]);
                break;
            case "jnz":
                EncodeJnz(parts[1]);
                break;
            case "je":
                EncodeJe(parts[1]);
                break;
            case "jne":
                EncodeJne(parts[1]);
                break;
            case "jl":
                EncodeJl(parts[1]);
                break;
            case "jg":
                EncodeJg(parts[1]);
                break;
            case "call":
                EncodeCall(parts[1]);
                break;
            case "ret":
                EncodeRet();
                break;
            case "nop":
                EncodeNop();
                break;
            case "cdq":
                EncodeCdq();
                break;
            case "sete":
            case "setne":
            case "setl":
            case "setg":
                EncodeSet(opcode, parts[1]);
                break;
            case "movzx":
                EncodeMovzx(parts[1], parts[2]);
                break;
            default:
                throw new NotImplementedException($"Instruction not implemented: {opcode}");
        }
    }
    
    // Register encoding helpers
    private static byte GetRegisterCode(string reg)
    {
        return reg.ToLower() switch
        {
            "eax" or "ax" or "al" => 0,
            "ecx" or "cx" or "cl" => 1,
            "edx" or "dx" or "dl" => 2,
            "ebx" or "bx" or "bl" => 3,
            "esp" or "sp" or "ah" => 4,
            "ebp" or "bp" or "ch" => 5,
            "esi" or "si" or "dh" => 6,
            "edi" or "di" or "bh" => 7,
            _ => throw new ArgumentException($"Unknown register: {reg}")
        };
    }
    
    private static bool IsImmediate(string operand)
    {
        return !string.IsNullOrEmpty(operand) && (char.IsDigit(operand[0]) || operand[0] == '-');
    }
    
    // Instruction encoders
    private static void EncodePush(string operand)
    {
        if (operand.StartsWith("e"))
        {
            // PUSH r32
            code.Add((byte)(0x50 + GetRegisterCode(operand)));
        }
        else
        {
            // PUSH imm32
            code.Add(0x68);
            AddImmediate32(int.Parse(operand));
        }
    }
    
    private static void EncodePop(string operand)
    {
        // POP r32
        code.Add((byte)(0x58 + GetRegisterCode(operand)));
    }
    
    private static void EncodeMov(string dst, string src)
    {
        // Guard against empty strings
        if (string.IsNullOrEmpty(dst) || string.IsNullOrEmpty(src))
        {
            throw new ArgumentException("Invalid operands for MOV instruction");
        }
        
        if (src.StartsWith("[") && src.EndsWith("]"))
        {
            // MOV r32, [m32]
            code.Add(0x8B); // MOV r32, r/m32
            // ModR/M byte (simplified)
            code.Add(0x00);
        }
        else if (dst.StartsWith("[") && dst.EndsWith("]"))
        {
            // MOV [m32], r32
            code.Add(0x89); // MOV r/m32, r32
            code.Add(0x00);
        }
        else if (IsImmediate(src))
        {
            // MOV r32, imm32
            code.Add((byte)(0xB8 + GetRegisterCode(dst)));
            AddImmediate32(int.Parse(src));
        }
        else
        {
            // MOV r32, r32
            code.Add(0x89); // MOV r/m32, r32
            code.Add((byte)(0xC0 | (GetRegisterCode(src) << 3) | GetRegisterCode(dst)));
        }
    }
    
    private static void EncodeAdd(string dst, string src)
    {
        // Check if src is immediate
        if (IsImmediate(src))
        {
            int imm = int.Parse(src);
            if (imm >= -128 && imm <= 127)
            {
                // ADD r/m32, imm8
                code.Add(0x83);
                code.Add((byte)(0xC0 | GetRegisterCode(dst)));
                code.Add((byte)imm);
            }
            else
            {
                // ADD r/m32, imm32
                code.Add(0x81);
                code.Add((byte)(0xC0 | GetRegisterCode(dst)));
                AddImmediate32(imm);
            }
        }
        else
        {
            // ADD r/m32, r32
            code.Add(0x01);
            code.Add((byte)(0xC0 | (GetRegisterCode(src) << 3) | GetRegisterCode(dst)));
        }
    }
    
    private static void EncodeSub(string dst, string src)
    {
        // Check if src is immediate
        if (IsImmediate(src))
        {
            int imm = int.Parse(src);
            if (imm >= -128 && imm <= 127)
            {
                // SUB r/m32, imm8
                code.Add(0x83);
                code.Add((byte)(0xE8 | GetRegisterCode(dst)));
                code.Add((byte)imm);
            }
            else
            {
                // SUB r/m32, imm32
                code.Add(0x81);
                code.Add((byte)(0xE8 | GetRegisterCode(dst)));
                AddImmediate32(imm);
            }
        }
        else
        {
            // SUB r/m32, r32
            code.Add(0x29);
            code.Add((byte)(0xC0 | (GetRegisterCode(src) << 3) | GetRegisterCode(dst)));
        }
    }
    
    private static void EncodeIMul(string dst, string src)
    {
        // IMUL r32, r/m32
        code.Add(0x0F);
        code.Add(0xAF);
        code.Add((byte)(0xC0 | (GetRegisterCode(dst) << 3) | GetRegisterCode(src)));
    }
    
    private static void EncodeIDiv(string operand)
    {
        // IDIV r/m32
        code.Add(0xF7);
        code.Add((byte)(0xF8 | GetRegisterCode(operand)));
    }
    
    private static void EncodeDiv(string operand)
    {
        // DIV r/m32
        code.Add(0xF7);
        code.Add((byte)(0xF0 | GetRegisterCode(operand)));
    }
    
    private static void EncodeAnd(string dst, string src)
    {
        // AND r/m32, r32
        code.Add(0x21);
        code.Add((byte)(0xC0 | (GetRegisterCode(src) << 3) | GetRegisterCode(dst)));
    }
    
    private static void EncodeOr(string dst, string src)
    {
        // OR r/m32, r32
        code.Add(0x09);
        code.Add((byte)(0xC0 | (GetRegisterCode(src) << 3) | GetRegisterCode(dst)));
    }
    
    private static void EncodeXor(string dst, string src)
    {
        // XOR r/m32, r32
        code.Add(0x31);
        code.Add((byte)(0xC0 | (GetRegisterCode(src) << 3) | GetRegisterCode(dst)));
    }
    
    private static void EncodeCmp(string dst, string src)
    {
        // CMP r/m32, r32
        code.Add(0x39);
        code.Add((byte)(0xC0 | (GetRegisterCode(src) << 3) | GetRegisterCode(dst)));
    }
    
    private static void EncodeTest(string dst, string src)
    {
        // TEST r/m32, r32
        code.Add(0x85);
        code.Add((byte)(0xC0 | (GetRegisterCode(src) << 3) | GetRegisterCode(dst)));
    }
    
    private static void EncodeJmp(string label)
    {
        // JMP rel32
        code.Add(0xE9);
        AddImmediate32(0); // Placeholder for offset
    }
    
    private static void EncodeJnz(string label)
    {
        // JNZ rel32
        code.Add(0x0F);
        code.Add(0x85);
        AddImmediate32(0); // Placeholder
    }
    
    private static void EncodeJe(string label)
    {
        // JE rel32
        code.Add(0x0F);
        code.Add(0x84);
        AddImmediate32(0);
    }
    
    private static void EncodeJne(string label)
    {
        // JNE rel32
        code.Add(0x0F);
        code.Add(0x85);
        AddImmediate32(0);
    }
    
    private static void EncodeJl(string label)
    {
        // JL rel32
        code.Add(0x0F);
        code.Add(0x8C);
        AddImmediate32(0);
    }
    
    private static void EncodeJg(string label)
    {
        // JG rel32
        code.Add(0x0F);
        code.Add(0x8F);
        AddImmediate32(0);
    }
    
    private static void EncodeCall(string target)
    {
        // CALL rel32
        code.Add(0xE8);
        AddImmediate32(0); // Placeholder
    }
    
    private static void EncodeRet()
    {
        // RET
        code.Add(0xC3);
    }
    
    private static void EncodeNop()
    {
        // NOP
        code.Add(0x90);
    }
    
    private static void EncodeCdq()
    {
        // CDQ - sign-extend EAX to EDX:EAX
        code.Add(0x99);
    }
    
    private static void EncodeSet(string op, string reg)
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
    
    private static void EncodeMovzx(string dst, string src)
    {
        // MOVZX r32, r/m8
        code.Add(0x0F);
        code.Add(0xB6);
        code.Add((byte)(0xC0 | (GetRegisterCode(dst) << 3) | GetRegisterCode(src)));
    }
    
    private static void AddImmediate32(int value)
    {
        code.Add((byte)(value & 0xFF));
        code.Add((byte)((value >> 8) & 0xFF));
        code.Add((byte)((value >> 16) & 0xFF));
        code.Add((byte)((value >> 24) & 0xFF));
    }
}
