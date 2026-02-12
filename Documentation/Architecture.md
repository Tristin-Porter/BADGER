# BADGER Architectures

## Overview

BADGER supports five target architectures, each with its own instruction encoding and assembly dialect. All architectures follow a two-pass assembly process and support the same high-level operations translated from WebAssembly Text (WAT).

## Supported Architectures

### x86_64 (AMD64 / Intel 64)

**Status**: ✅ Complete  
**Bit Width**: 64-bit  
**Endianness**: Little-endian

#### Registers

- **General Purpose**: rax, rbx, rcx, rdx, rsi, rdi, r8-r15
- **Stack/Frame**: rsp (stack pointer), rbp (base pointer)
- **32-bit access**: eax, ebx, ecx, edx, esi, edi
- **16-bit access**: ax, bx, cx, dx, si, di
- **8-bit access**: al, bl, cl, dl, ah, bh, ch, dh

#### Key Features

- REX prefix (0x48) for 64-bit operations
- ModR/M byte for register/memory encoding
- Supports both 8-bit and 32-bit immediate values
- Variable-length instruction encoding (1-15 bytes)

#### Supported Instructions

- **Stack**: push, pop
- **Data Movement**: mov, movzx
- **Arithmetic**: add, sub, imul, idiv, div
- **Logical**: and, or, xor
- **Comparison**: cmp, test
- **Control Flow**: jmp, je, jne, jl, jg, jnz, call, ret
- **Conditional Set**: sete, setne, setl, setg
- **Special**: nop, cqo

#### Example Assembly

```asm
main:
    push rbp
    mov rbp, rsp
    sub rsp, 16
    mov rax, 42
    mov rsp, rbp
    pop rbp
    ret
```

---

### x86_32 (IA-32 / i386)

**Status**: ✅ Complete  
**Bit Width**: 32-bit  
**Endianness**: Little-endian

#### Registers

- **General Purpose**: eax, ebx, ecx, edx, esi, edi
- **Stack/Frame**: esp (stack pointer), ebp (base pointer)
- **16-bit access**: ax, bx, cx, dx, si, di
- **8-bit access**: al, bl, cl, dl, ah, bh, ch, dh

#### Key Features

- NO REX prefix (32-bit native)
- ModR/M byte for register/memory encoding
- Supports both 8-bit and 32-bit immediate values
- Variable-length instruction encoding (1-11 bytes)

#### Supported Instructions

Same as x86_64 but using 32-bit registers and encodings.

#### Example Assembly

```asm
main:
    push ebp
    mov ebp, esp
    sub esp, 16
    mov eax, 42
    mov esp, ebp
    pop ebp
    ret
```

---

### x86_16 (Real Mode)

**Status**: ✅ Complete  
**Bit Width**: 16-bit  
**Endianness**: Little-endian

#### Registers

- **General Purpose**: ax, bx, cx, dx, si, di
- **Stack/Frame**: sp (stack pointer), bp (base pointer)
- **8-bit access**: al, bl, cl, dl, ah, bh, ch, dh

#### Key Features

- Real mode (no protected mode features)
- ModR/M byte for register/memory encoding
- 16-bit immediates and offsets
- Variable-length instruction encoding
- Supports far return (retf) for segment transitions

#### Supported Instructions

Similar to x86_32 but with 16-bit registers and far return support.

#### Example Assembly

```asm
main:
    push bp
    mov bp, sp
    sub sp, 8
    mov ax, 42
    mov sp, bp
    pop bp
    retf
```

---

### ARM64 (AArch64)

**Status**: ✅ Complete  
**Bit Width**: 64-bit  
**Endianness**: Little-endian

#### Registers

- **64-bit**: x0-x30, sp, xzr (zero register)
- **32-bit**: w0-w30, wzr
- **Special**: x29 (frame pointer), x30 (link register)

#### Key Features

- Fixed 32-bit instruction size
- All instructions are 4 bytes aligned
- PC-relative addressing for branches
- Load/store architecture (no direct memory operations in arithmetic)
- Pre-index and post-index addressing modes

#### Supported Instructions

- **Control Flow**: ret, nop, b, b.eq, b.ne, b.lt, b.gt, bl
- **Data Movement**: mov, ldr, str, ldp, stp
- **Arithmetic**: add, sub, mul, cmp
- **Logical**: and, orr, eor

#### Example Assembly

```asm
main:
    stp x29, x30, [sp, #-16]!
    mov x29, sp
    mov w0, #42
    mov sp, x29
    ldp x29, x30, [sp], #16
    ret
```

---

### ARM32 (ARMv7)

**Status**: ✅ Complete  
**Bit Width**: 32-bit  
**Endianness**: Little-endian

#### Registers

- **General**: r0-r12
- **Special**: sp (r13), lr (r14), pc (r15)

#### Key Features

- Fixed 32-bit instruction size
- All instructions are 4 bytes aligned
- Immediate encoding uses 8-bit value with 4-bit rotation
- Condition codes on every instruction (0xE = always)
- PC+8 offset convention for branches

#### Supported Instructions

- **Control Flow**: bx, nop, b, beq, bne, blt, bgt, bl
- **Data Movement**: mov, ldr, str
- **Arithmetic**: add, sub, mul, cmp
- **Logical**: and, orr, eor
- **Stack**: push, pop

#### Example Assembly

```asm
main:
    push {r11, lr}
    mov r11, sp
    mov r0, #42
    mov sp, r11
    pop {r11, pc}
```

---

## Architecture Comparison

| Feature | x86_64 | x86_32 | x86_16 | ARM64 | ARM32 |
|---------|--------|--------|--------|-------|-------|
| Instruction Size | Variable | Variable | Variable | Fixed (32-bit) | Fixed (32-bit) |
| Endianness | Little | Little | Little | Little | Little |
| REX Prefix | Yes | No | No | N/A | N/A |
| GP Registers | 16 | 8 | 8 | 31 | 13 |
| Stack Direction | Down | Down | Down | Down | Down |
| Alignment | Byte | Byte | Byte | Word (4B) | Word (4B) |

## WAT Lowering

All architectures support the same WAT operations through their respective MapSets:

- Module structure
- Function definitions with prologue/epilogue
- i32 arithmetic (add, sub, mul, div)
- i32 logical operations (and, or, xor)
- Local variables (get/set/tee)
- Control flow (br, br_if, return, call)
- Stack operations (drop)

Each architecture's MapSet translates these operations into architecture-specific assembly instructions.

## Assembly Dialect

Each architecture uses its own canonical assembly dialect:

- **x86**: Intel syntax (destination first)
- **ARM**: ARM syntax with register prefixes

Example translations for `i32.add`:

```
x86_64:  pop rax; pop rbx; add rax, rbx; push rax
x86_32:  pop eax; pop ebx; add eax, ebx; push eax
x86_16:  pop ax; pop bx; add ax, bx; push ax
ARM64:   ldr w0, [sp], #4; ldr w1, [sp], #4; add w0, w0, w1; str w0, [sp, #-4]!
ARM32:   pop {r0}; pop {r1}; add r0, r0, r1; push {r0}
```

## Implementation Details

Each architecture implementation consists of:

1. **MapSet** - WAT to assembly templates
2. **Assembler** - Two-pass assembly (labels → encoding)
3. **Instruction Encoders** - Binary encoding for each instruction
4. **Helper Methods** - Register parsing, immediate encoding, etc.

All architectures are modular and independent - changes to one do not affect others.

## Testing

Each architecture has comprehensive test coverage:

- Instruction encoding verification
- Register encoding tests
- Label resolution tests
- Function prologue/epilogue tests
- Integration tests with containers

Total test count: 266+ tests across all architectures.

## References

- [x86 Instruction Reference](https://www.intel.com/content/www/us/en/developer/articles/technical/intel-sdm.html)
- [ARM Architecture Reference Manual](https://developer.arm.com/documentation/)
- [WebAssembly Specification](https://webassembly.github.io/spec/)
