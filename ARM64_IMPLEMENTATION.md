# ARM64 (AArch64) Assembler Implementation

## Overview

The ARM64 assembler in BADGER provides a complete instruction encoder for the AArch64 architecture, following the same two-pass pattern as the x86 assemblers. All instructions are encoded as fixed 32-bit (4-byte) little-endian values.

## Architecture

### Two-Pass Assembly

**Pass 1: Label Collection**
- Scans through assembly text
- Records label positions (in bytes)
- Each instruction is exactly 4 bytes (fixed-width encoding)
- Ignores comments (starting with `//` or `;`)

**Pass 2: Instruction Encoding**
- Encodes each instruction to 32-bit machine code
- Resolves label references using PC-relative addressing
- Outputs little-endian byte order

### Fixed-Width Instructions

Unlike x86 architectures (variable-length instructions), ARM64 uses a fixed 32-bit instruction format:
- Every instruction is exactly 4 bytes
- Simplifies instruction decoding in hardware
- Makes label resolution straightforward (offset / 4)
- All offsets are in instruction counts, not bytes

## Supported Instructions

### Control Flow

| Instruction | Description | Encoding |
|-------------|-------------|----------|
| `ret` | Return from subroutine | `0xD65F03C0` |
| `nop` | No operation | `0xD503201F` |
| `b label` | Unconditional branch | PC-relative, 26-bit offset |
| `b.eq label` | Branch if equal | PC-relative, 19-bit offset, cond=0000 |
| `b.ne label` | Branch if not equal | PC-relative, 19-bit offset, cond=0001 |
| `b.lt label` | Branch if less than | PC-relative, 19-bit offset, cond=1011 |
| `b.gt label` | Branch if greater than | PC-relative, 19-bit offset, cond=1100 |
| `bl label` | Branch with link (call) | PC-relative, 26-bit offset |

### Data Movement

| Instruction | Description | Operands |
|-------------|-------------|----------|
| `mov` | Move register or immediate | `Rd, Rm` or `Rd, #imm` |
| `ldr` | Load register | `Rt, [Rn], #imm` or `Rt, [Rn, #imm]!` |
| `str` | Store register | `Rt, [Rn], #imm` or `Rt, [Rn, #imm]!` |
| `ldp` | Load pair of registers | `Rt1, Rt2, [Rn], #imm` |
| `stp` | Store pair of registers | `Rt1, Rt2, [Rn, #imm]!` |

### Arithmetic

| Instruction | Description | Operands |
|-------------|-------------|----------|
| `add` | Add | `Rd, Rn, Rm` or `Rd, Rn, #imm` |
| `sub` | Subtract | `Rd, Rn, Rm` or `Rd, Rn, #imm` |
| `mul` | Multiply | `Rd, Rn, Rm` |
| `cmp` | Compare (SUBS with Rd=XZR) | `Rn, Rm` or `Rn, #imm` |

### Logical Operations

| Instruction | Description | Operands |
|-------------|-------------|----------|
| `and` | Bitwise AND | `Rd, Rn, Rm` |
| `orr` | Bitwise OR | `Rd, Rn, Rm` |
| `eor` | Bitwise Exclusive OR | `Rd, Rn, Rm` |

## Register Set

### 64-bit Registers (X registers)
- `x0` - `x30`: General purpose 64-bit registers
- `sp`: Stack pointer (register 31)
- `xzr`: Zero register (register 31, reads as 0, writes discarded)

### 32-bit Registers (W registers)
- `w0` - `w30`: Lower 32 bits of X registers
- `wzr`: 32-bit zero register

### Special Registers
- `x29`: Frame pointer (FP) by convention
- `x30`: Link register (LR) - stores return address
- `sp`: Stack pointer - can be register 31

## Instruction Encoding Details

### Bit Field Layout

ARM64 instructions use specific bit fields for encoding:

```
31 30 29 28 27 26 25 24 23 22 21 20 19 18 17 16 15 14 13 12 11 10 09 08 07 06 05 04 03 02 01 00
[sf][opc ][      opcode/fixed bits      ][  operand fields  ][  register fields  ]
```

- `sf` (bit 31): Size flag (0=32-bit, 1=64-bit)
- `opc`: Operation code variant
- Fixed bits: Identify instruction class
- Operand fields: Immediates, shift amounts, conditions
- Register fields: Source/destination registers (5 bits each)

### Example Encodings

**RET (Return)**
```
Encoding: 0xD65F03C0
Binary:   1101 0110 0101 1111 0000 0011 1100 0000
Little-endian bytes: C0 03 5F D6
```

**NOP (No Operation)**
```
Encoding: 0xD503201F
Binary:   1101 0101 0000 0011 0010 0000 0001 1111
Little-endian bytes: 1F 20 03 D5
```

**ADD (Register, 64-bit): add x0, x1, x2**
```
sf=1, opc=00, fixed=01011, shift=00, N=0
Rm=x2, imm6=000000, Rn=x1, Rd=x0
Encoding: 1 00 01011 00 0 00010 000000 00001 00000
Result: 0x8B020020
Little-endian bytes: 20 00 02 8B
```

**MOV (Immediate, 64-bit): mov x0, #42**
```
Uses MOVZ (Move wide with zero)
sf=1, opc=10, fixed=100101, hw=00, imm16=42, Rd=x0
Encoding: 1 10 100101 0 00 0000000000101010 00000
Result: 0xD2800540
Little-endian bytes: 40 05 80 D2
```

**B (Unconditional branch): b label**
```
Fixed=000101, imm26=(offset/4)
Encoding: 0 00101 [26-bit signed offset]
For forward jump of 4 instructions:
offset = 4, imm26 = 000000 00000000 00000000 000100
Result: 0x14000004
Little-endian bytes: 04 00 00 14
```

**B.EQ (Conditional branch): b.eq label**
```
Fixed=0101010, o1=0, imm19=(offset/4), o0=0, cond=0000
For backward jump of 1 instruction:
offset = -1, imm19 = 1111111111111111111
Result: 0x54FFFFE0
Little-endian bytes: E0 FF FF 54
```

## PC-Relative Addressing

ARM64 branches use PC-relative addressing:

1. **Calculate offset in bytes**: `offset_bytes = target_address - current_address`
2. **Convert to instruction count**: `offset_insns = offset_bytes / 4`
3. **Encode as signed immediate**:
   - `B` and `BL`: 26-bit signed immediate (±128 MB range)
   - `B.cond`: 19-bit signed immediate (±1 MB range)
4. **PC points to current instruction** (not PC+8 like ARM32)

### Example

```assembly
0x00: start:
0x00:     nop              // 4 bytes
0x04:     b start          // Jump back to 0x00
```

Offset calculation:
- Current PC: 0x04
- Target: 0x00
- Offset (bytes): 0x00 - 0x04 = -4
- Offset (instructions): -4 / 4 = -1
- Encoded: 26-bit signed -1 = 0x3FFFFFF (two's complement)

## Addressing Modes

### Load/Store Immediate Modes

1. **Post-index**: `ldr x0, [sp], #16`
   - Load from [sp], then sp = sp + 16
   - Mode bits: 01

2. **Pre-index**: `str x0, [sp, #-16]!`
   - sp = sp - 16, then store to [sp]
   - Mode bits: 11

3. **Unsigned offset**: `ldr x0, [sp, #8]`
   - Load from [sp + 8], sp unchanged
   - Mode bits: 10

### Pair Operations

STP/LDP use scaled 7-bit signed offsets:
- 64-bit: offset scaled by 8 (range: -512 to +504 bytes)
- 32-bit: offset scaled by 4 (range: -256 to +252 bytes)

Example: `stp x29, x30, [sp, #-16]!`
- Offset: -16 bytes / 8 = -2
- imm7: 7-bit signed -2 = 0b1111110

## Function Calling Convention

Standard ARM64 function prologue/epilogue:

```assembly
function:
    // Prologue: save frame pointer and link register
    stp x29, x30, [sp, #-16]!   // Push FP and LR
    mov x29, sp                  // Set up frame pointer
    sub sp, sp, #32              // Allocate local space
    
    // Function body
    // ...
    
    // Epilogue: restore and return
    mov sp, x29                  // Restore stack pointer
    ldp x29, x30, [sp], #16      // Pop FP and LR
    ret                          // Return (jumps to x30)
```

## Register Usage Convention

| Register | Role | Saved by |
|----------|------|----------|
| x0-x7 | Argument/result registers | Caller |
| x8 | Indirect result location | Caller |
| x9-x15 | Temporary registers | Caller |
| x16-x17 | Intra-procedure-call registers | Caller |
| x18 | Platform register | Platform |
| x19-x28 | Callee-saved registers | Callee |
| x29 | Frame pointer | Callee |
| x30 | Link register | Callee |
| sp | Stack pointer | Callee |

## 32-bit vs 64-bit Operations

The `sf` bit (bit 31) determines operation width:
- `sf=0`: 32-bit operation (W registers)
- `sf=1`: 64-bit operation (X registers)

32-bit operations zero-extend to 64 bits:
```assembly
mov w0, #42    // w0 = 0x0000002A, x0 = 0x000000000000002A
```

## Implementation Notes

### Little-Endian Encoding

All instructions are stored in little-endian byte order:
```csharp
void AddInstruction(uint instruction)
{
    code.Add((byte)(instruction & 0xFF));           // Byte 0
    code.Add((byte)((instruction >> 8) & 0xFF));    // Byte 1
    code.Add((byte)((instruction >> 16) & 0xFF));   // Byte 2
    code.Add((byte)((instruction >> 24) & 0xFF));   // Byte 3
}
```

### Register Encoding

Special register handling:
```csharp
int GetRegisterNumber(string reg)
{
    if (reg == "sp") return 31;      // Stack pointer
    if (reg == "xzr" || reg == "wzr") return 31;  // Zero register
    if (reg.StartsWith("x") || reg.StartsWith("w"))
        return int.Parse(reg.Substring(1));  // x0-x30 or w0-w30
}
```

### MOV Pseudo-Instruction

`MOV` is implemented using different instructions:
- `mov Rd, #imm` → MOVZ (Move wide with zero)
- `mov Rd, Rm` → ORR Rd, XZR, Rm (OR with zero register)

### CMP Pseudo-Instruction

`CMP` is implemented as SUBS with destination = XZR:
```assembly
cmp x0, x1  →  subs xzr, x0, x1  // Sets flags, discards result
```

## Testing

The ARM64 assembler includes comprehensive tests:
- Basic instruction encoding (RET, NOP)
- Arithmetic operations (ADD, SUB, MUL)
- Logical operations (AND, ORR, EOR)
- Comparison and branches
- Load/store operations (LDR, STR, LDP, STP)
- 32-bit vs 64-bit operations
- Label resolution
- PC-relative addressing
- Function prologue/epilogue patterns

All tests verify:
- Exact 4-byte instruction size
- Correct opcode encoding
- Little-endian byte order
- Proper register field encoding
- PC-relative offset calculation

## References

- ARM Architecture Reference Manual (ARMv8-A)
- ARM Cortex-A Series Programmer's Guide
- ARM Instruction Set Reference (AArch64)
- ARM ABI (Application Binary Interface) for the ARM Architecture

## Example: Complete Function

```assembly
// Calculate sum of two numbers
add_numbers:
    // Prologue
    stp x29, x30, [sp, #-16]!
    mov x29, sp
    sub sp, sp, #16
    
    // Store parameters (x0, x1 contain arguments)
    str w0, [sp, #12]
    str w1, [sp, #8]
    
    // Load and add
    ldr w0, [sp, #12]
    ldr w1, [sp, #8]
    add w0, w0, w1
    
    // Epilogue
    mov sp, x29
    ldp x29, x30, [sp], #16
    ret
```

This function:
1. Saves frame pointer and return address
2. Allocates 16 bytes of local storage
3. Stores input parameters to stack
4. Loads parameters and performs addition
5. Returns result in w0
6. Restores stack and returns

Encoded size: 9 instructions × 4 bytes = 36 bytes of machine code.
