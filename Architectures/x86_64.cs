using CDTk;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace Badger.Architectures.x86_64;

// Part 1: CDTk MapSet for WAT → x86_64 assembly translation
public class WATToX86_64MapSet : MapSet
{
    // Module mapping
    public Map Module = @"
; x86_64 Assembly
; Module: {id}
{fields}
";

    // Function mapping with prologue/epilogue
    public Map Function = @"
{id}:
    push rbp
    mov rbp, rsp
    sub rsp, {local_space}  ; Allocate space for locals
{body}
    mov rsp, rbp
    pop rbp
    ret
";

    // Instruction mappings - Numeric operations
    public Map I32Add = "    pop rax\n    pop rbx\n    add rax, rbx\n    push rax";
    public Map I32Sub = "    pop rbx\n    pop rax\n    sub rax, rbx\n    push rax";
    public Map I32Mul = "    pop rax\n    pop rbx\n    imul rax, rbx\n    push rax";
    public Map I32DivS = "    pop rbx\n    pop rax\n    cqo\n    idiv rbx\n    push rax";
    public Map I32DivU = "    pop rbx\n    pop rax\n    xor rdx, rdx\n    div rbx\n    push rax";
    
    public Map I32Const = "    mov rax, {value}\n    push rax";
    public Map I64Const = "    mov rax, {value}\n    push rax";
    
    // Logical operations
    public Map I32And = "    pop rax\n    pop rbx\n    and rax, rbx\n    push rax";
    public Map I32Or = "    pop rax\n    pop rbx\n    or rax, rbx\n    push rax";
    public Map I32Xor = "    pop rax\n    pop rbx\n    xor rax, rbx\n    push rax";
    
    // Comparison operations
    public Map I32Eq = "    pop rbx\n    pop rax\n    cmp rax, rbx\n    sete al\n    movzx rax, al\n    push rax";
    public Map I32Ne = "    pop rbx\n    pop rax\n    cmp rax, rbx\n    setne al\n    movzx rax, al\n    push rax";
    public Map I32LtS = "    pop rbx\n    pop rax\n    cmp rax, rbx\n    setl al\n    movzx rax, al\n    push rax";
    public Map I32GtS = "    pop rbx\n    pop rax\n    cmp rax, rbx\n    setg al\n    movzx rax, al\n    push rax";
    
    // Local variables
    public Map LocalGet = "    mov rax, [rbp - {offset}]\n    push rax";
    public Map LocalSet = "    pop rax\n    mov [rbp - {offset}], rax";
    public Map LocalTee = "    pop rax\n    mov [rbp - {offset}], rax\n    push rax";
    
    // Control flow
    public Map Return = "    jmp .function_exit";
    public Map Call = "    call {funcidx}";
    public Map Br = "    jmp {labelidx}";
    public Map BrIf = "    pop rax\n    test rax, rax\n    jnz {labelidx}";
    
    // Memory operations
    public Map I32Load = "    pop rax\n    mov eax, [rax + {offset}]\n    push rax";
    public Map I32Store = "    pop rbx\n    pop rax\n    mov [rax + {offset}], ebx";
    
    // Stack operations
    public Map Drop = "    add rsp, 8";
    public Map Nop = "    nop";
}

// Part 2: x86_64 Assembler - converts assembly text to machine code
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
        // Simplified size estimation
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
            case "cqo":
                EncodeCqo();
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
            "rax" or "eax" or "ax" or "al" => 0,
            "rcx" or "ecx" or "cx" or "cl" => 1,
            "rdx" or "edx" or "dx" or "dl" => 2,
            "rbx" or "ebx" or "bx" or "bl" => 3,
            "rsp" or "esp" or "sp" or "ah" => 4,
            "rbp" or "ebp" or "bp" or "ch" => 5,
            "rsi" or "esi" or "si" or "dh" => 6,
            "rdi" or "edi" or "di" or "bh" => 7,
            _ => throw new ArgumentException($"Unknown register: {reg}")
        };
    }
    
    // Instruction encoders (simplified)
    private static void EncodePush(string operand)
    {
        if (operand.StartsWith("r") || operand.StartsWith("e"))
        {
            code.Add((byte)(0x50 + GetRegisterCode(operand)));
        }
        else
        {
            // Push immediate
            code.Add(0x68);
            AddImmediate32(int.Parse(operand));
        }
    }
    
    private static void EncodePop(string operand)
    {
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
            // MOV from memory
            code.Add(0x48); // REX.W
            code.Add(0x8B); // MOV r64, r/m64
            // ModR/M byte (simplified)
            code.Add(0x00);
        }
        else if (dst.StartsWith("[") && dst.EndsWith("]"))
        {
            // MOV to memory
            code.Add(0x48); // REX.W
            code.Add(0x89); // MOV r/m64, r64
            code.Add(0x00);
        }
        else if (char.IsDigit(src[0]) || src[0] == '-')
        {
            // MOV immediate
            code.Add(0x48); // REX.W
            code.Add((byte)(0xB8 + GetRegisterCode(dst)));
            AddImmediate64(long.Parse(src));
        }
        else
        {
            // MOV register to register
            code.Add(0x48); // REX.W
            code.Add(0x89); // MOV r/m64, r64
            code.Add((byte)(0xC0 | (GetRegisterCode(src) << 3) | GetRegisterCode(dst)));
        }
    }
    
    private static void EncodeAdd(string dst, string src)
    {
        // Check if src is immediate
        if (char.IsDigit(src[0]) || src[0] == '-')
        {
            int imm = int.Parse(src);
            code.Add(0x48); // REX.W
            if (imm >= -128 && imm <= 127)
            {
                // ADD r/m64, imm8
                code.Add(0x83);
                code.Add((byte)(0xC0 | GetRegisterCode(dst)));
                code.Add((byte)imm);
            }
            else
            {
                // ADD r/m64, imm32
                code.Add(0x81);
                code.Add((byte)(0xC0 | GetRegisterCode(dst)));
                AddImmediate32(imm);
            }
        }
        else
        {
            code.Add(0x48); // REX.W
            code.Add(0x01); // ADD r/m64, r64
            code.Add((byte)(0xC0 | (GetRegisterCode(src) << 3) | GetRegisterCode(dst)));
        }
    }
    
    private static void EncodeSub(string dst, string src)
    {
        // Check if src is immediate
        if (char.IsDigit(src[0]) || src[0] == '-')
        {
            int imm = int.Parse(src);
            code.Add(0x48); // REX.W
            if (imm >= -128 && imm <= 127)
            {
                // SUB r/m64, imm8
                code.Add(0x83);
                code.Add((byte)(0xE8 | GetRegisterCode(dst)));
                code.Add((byte)imm);
            }
            else
            {
                // SUB r/m64, imm32
                code.Add(0x81);
                code.Add((byte)(0xE8 | GetRegisterCode(dst)));
                AddImmediate32(imm);
            }
        }
        else
        {
            code.Add(0x48); // REX.W
            code.Add(0x29); // SUB r/m64, r64
            code.Add((byte)(0xC0 | (GetRegisterCode(src) << 3) | GetRegisterCode(dst)));
        }
    }
    
    private static void EncodeIMul(string dst, string src)
    {
        code.Add(0x48); // REX.W
        code.Add(0x0F);
        code.Add(0xAF); // IMUL r64, r/m64
        code.Add((byte)(0xC0 | (GetRegisterCode(dst) << 3) | GetRegisterCode(src)));
    }
    
    private static void EncodeIDiv(string operand)
    {
        code.Add(0x48); // REX.W
        code.Add(0xF7); // IDIV r/m64
        code.Add((byte)(0xF8 | GetRegisterCode(operand)));
    }
    
    private static void EncodeDiv(string operand)
    {
        code.Add(0x48); // REX.W
        code.Add(0xF7); // DIV r/m64
        code.Add((byte)(0xF0 | GetRegisterCode(operand)));
    }
    
    private static void EncodeAnd(string dst, string src)
    {
        code.Add(0x48); // REX.W
        code.Add(0x21); // AND r/m64, r64
        code.Add((byte)(0xC0 | (GetRegisterCode(src) << 3) | GetRegisterCode(dst)));
    }
    
    private static void EncodeOr(string dst, string src)
    {
        code.Add(0x48); // REX.W
        code.Add(0x09); // OR r/m64, r64
        code.Add((byte)(0xC0 | (GetRegisterCode(src) << 3) | GetRegisterCode(dst)));
    }
    
    private static void EncodeXor(string dst, string src)
    {
        code.Add(0x48); // REX.W
        code.Add(0x31); // XOR r/m64, r64
        code.Add((byte)(0xC0 | (GetRegisterCode(src) << 3) | GetRegisterCode(dst)));
    }
    
    private static void EncodeCmp(string dst, string src)
    {
        code.Add(0x48); // REX.W
        code.Add(0x39); // CMP r/m64, r64
        code.Add((byte)(0xC0 | (GetRegisterCode(src) << 3) | GetRegisterCode(dst)));
    }
    
    private static void EncodeTest(string dst, string src)
    {
        code.Add(0x48); // REX.W
        code.Add(0x85); // TEST r/m64, r64
        code.Add((byte)(0xC0 | (GetRegisterCode(src) << 3) | GetRegisterCode(dst)));
    }
    
    private static void EncodeJmp(string label)
    {
        code.Add(0xE9); // JMP rel32
        AddImmediate32(0); // Placeholder for offset
    }
    
    private static void EncodeJnz(string label)
    {
        code.Add(0x0F);
        code.Add(0x85); // JNZ rel32
        AddImmediate32(0); // Placeholder
    }
    
    private static void EncodeJe(string label)
    {
        code.Add(0x0F);
        code.Add(0x84); // JE rel32
        AddImmediate32(0);
    }
    
    private static void EncodeJne(string label)
    {
        code.Add(0x0F);
        code.Add(0x85); // JNE rel32
        AddImmediate32(0);
    }
    
    private static void EncodeJl(string label)
    {
        code.Add(0x0F);
        code.Add(0x8C); // JL rel32
        AddImmediate32(0);
    }
    
    private static void EncodeJg(string label)
    {
        code.Add(0x0F);
        code.Add(0x8F); // JG rel32
        AddImmediate32(0);
    }
    
    private static void EncodeCall(string target)
    {
        code.Add(0xE8); // CALL rel32
        AddImmediate32(0); // Placeholder
    }
    
    private static void EncodeRet()
    {
        code.Add(0xC3);
    }
    
    private static void EncodeNop()
    {
        code.Add(0x90);
    }
    
    private static void EncodeCqo()
    {
        code.Add(0x48); // REX.W
        code.Add(0x99); // CQO
    }
    
    private static void EncodeSet(string op, string reg)
    {
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
        code.Add(0x48); // REX.W
        code.Add(0x0F);
        code.Add(0xB6); // MOVZX
        code.Add((byte)(0xC0 | (GetRegisterCode(dst) << 3) | GetRegisterCode(src)));
    }
    
    private static void AddImmediate32(int value)
    {
        code.Add((byte)(value & 0xFF));
        code.Add((byte)((value >> 8) & 0xFF));
        code.Add((byte)((value >> 16) & 0xFF));
        code.Add((byte)((value >> 24) & 0xFF));
    }
    
    private static void AddImmediate64(long value)
    {
        code.Add((byte)(value & 0xFF));
        code.Add((byte)((value >> 8) & 0xFF));
        code.Add((byte)((value >> 16) & 0xFF));
        code.Add((byte)((value >> 24) & 0xFF));
        code.Add((byte)((value >> 32) & 0xFF));
        code.Add((byte)((value >> 40) & 0xFF));
        code.Add((byte)((value >> 48) & 0xFF));
        code.Add((byte)((value >> 56) & 0xFF));
    }
}