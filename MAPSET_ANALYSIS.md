# MapSet Stub Analysis - BADGER Assembler

## Summary

This document identifies the MapSet stubs in the BADGER assembler that need to be replaced with complete implementations.

## What are MapSets?

MapSets are CDTk components that translate WebAssembly Text (WAT) AST nodes into target architecture assembly code. Each architecture (x86_64, x86_32, x86_16, ARM64, ARM32) has its own MapSet class that defines `Map` declarations - template mappings from WAT operations to assembly instructions.

## Current State

### Files and Line Counts
```
ARM32.cs:    790 lines (11 Map declarations)
ARM64.cs:    813 lines (11 Map declarations)
x86_16.cs:   500 lines (11 Map declarations)
x86_32.cs:   560 lines (26 Map declarations)
x86_64.cs:   518 lines (27 Map declarations)
```

## Identified MapSet Stubs

### 1. **ARM32 MapSet** (`Architectures/ARM32.cs`, lines 8-35)

**Status:** INCOMPLETE - Only 11 Map declarations (needs ~16 more)

**Current Maps:**
- ✅ Module
- ✅ Function
- ✅ I32Add
- ✅ I32Sub
- ✅ I32Mul
- ✅ I32Const
- ✅ LocalGet
- ✅ LocalSet
- ✅ Return
- ✅ Call
- ✅ Drop

**Missing Maps (comparing to x86_64/x86_32):**
- ❌ I32DivS (signed division)
- ❌ I32DivU (unsigned division)
- ❌ I32And (bitwise AND)
- ❌ I32Or (bitwise OR)
- ❌ I32Xor (bitwise XOR)
- ❌ I32Eq (equality comparison)
- ❌ I32Ne (not equal comparison)
- ❌ I32LtS (less than signed)
- ❌ I32GtS (greater than signed)
- ❌ LocalTee (set local and keep value on stack)
- ❌ Br (unconditional branch)
- ❌ BrIf (conditional branch)
- ❌ I32Load (load from memory)
- ❌ I32Store (store to memory)
- ❌ Nop (no operation)

**Location:** `/home/runner/work/BADGER/BADGER/Architectures/ARM32.cs` lines 8-35

---

### 2. **ARM64 MapSet** (`Architectures/ARM64.cs`, lines 9-37)

**Status:** INCOMPLETE - Only 11 Map declarations (needs ~16 more)

**Current Maps:**
- ✅ Module
- ✅ Function
- ✅ I32Add
- ✅ I32Sub
- ✅ I32Mul
- ✅ I32Const
- ✅ LocalGet
- ✅ LocalSet
- ✅ Return
- ✅ Call
- ✅ Drop

**Missing Maps:**
- ❌ I32DivS (signed division)
- ❌ I32DivU (unsigned division)
- ❌ I32And (bitwise AND)
- ❌ I32Or (bitwise OR)
- ❌ I32Xor (bitwise XOR)
- ❌ I32Eq (equality comparison)
- ❌ I32Ne (not equal comparison)
- ❌ I32LtS (less than signed)
- ❌ I32GtS (greater than signed)
- ❌ LocalTee (set local and keep value on stack)
- ❌ Br (unconditional branch)
- ❌ BrIf (conditional branch)
- ❌ I32Load (load from memory)
- ❌ I32Store (store to memory)
- ❌ Nop (no operation)

**Location:** `/home/runner/work/BADGER/BADGER/Architectures/ARM64.cs` lines 9-37

---

### 3. **x86_16 MapSet** (`Architectures/x86_16.cs`, lines 8-36)

**Status:** INCOMPLETE - Only 11 Map declarations (needs ~16 more)

**Current Maps:**
- ✅ Module
- ✅ Function
- ✅ I32Add
- ✅ I32Sub
- ✅ I32Mul
- ✅ I32Const
- ✅ LocalGet
- ✅ LocalSet
- ✅ Return
- ✅ Call
- ✅ Drop

**Missing Maps:**
- ❌ I32DivS (signed division)
- ❌ I32DivU (unsigned division)
- ❌ I32And (bitwise AND)
- ❌ I32Or (bitwise OR)
- ❌ I32Xor (bitwise XOR)
- ❌ I32Eq (equality comparison)
- ❌ I32Ne (not equal comparison)
- ❌ I32LtS (less than signed)
- ❌ I32GtS (greater than signed)
- ❌ LocalTee (set local and keep value on stack)
- ❌ Br (unconditional branch)
- ❌ BrIf (conditional branch)
- ❌ I32Load (load from memory)
- ❌ I32Store (store to memory)
- ❌ Nop (no operation)

**Location:** `/home/runner/work/BADGER/BADGER/Architectures/x86_16.cs` lines 8-36

---

## Reference Implementations (Complete)

### x86_64 MapSet (`Architectures/x86_64.cs`)
**Status:** COMPLETE - 27 Map declarations

### x86_32 MapSet (`Architectures/x86_32.cs`)
**Status:** COMPLETE - 26 Map declarations

These two architectures have full implementations and should be used as reference when implementing the missing Maps for ARM32, ARM64, and x86_16.

---

## What Complete Implementations Should Look Like

### Example: x86_64 Complete MapSet Structure

```csharp
public class WATToX86_64MapSet : MapSet
{
    // Module structure
    public Map Module = @"...{fields}...";
    public Map Function = @"...{body}...";
    
    // Arithmetic operations
    public Map I32Add = "...";
    public Map I32Sub = "...";
    public Map I32Mul = "...";
    public Map I32DivS = "...";
    public Map I32DivU = "...";
    
    // Constants
    public Map I32Const = "...";
    public Map I64Const = "...";
    
    // Logical operations
    public Map I32And = "...";
    public Map I32Or = "...";
    public Map I32Xor = "...";
    
    // Comparison operations
    public Map I32Eq = "...";
    public Map I32Ne = "...";
    public Map I32LtS = "...";
    public Map I32GtS = "...";
    
    // Local variables
    public Map LocalGet = "...";
    public Map LocalSet = "...";
    public Map LocalTee = "...";
    
    // Control flow
    public Map Return = "...";
    public Map Call = "...";
    public Map Br = "...";
    public Map BrIf = "...";
    
    // Memory operations
    public Map I32Load = "...";
    public Map I32Store = "...";
    
    // Stack operations
    public Map Drop = "...";
    public Map Nop = "...";
}
```

---

## Implementation Template Examples

### ARM32 Template Examples (to be added)

```csharp
// Division operations
public Map I32DivS = "    pop {{r1}}\n    pop {{r0}}\n    sdiv r0, r0, r1\n    push {{r0}}";
public Map I32DivU = "    pop {{r1}}\n    pop {{r0}}\n    udiv r0, r0, r1\n    push {{r0}}";

// Logical operations
public Map I32And = "    pop {{r1}}\n    pop {{r0}}\n    and r0, r0, r1\n    push {{r0}}";
public Map I32Or = "    pop {{r1}}\n    pop {{r0}}\n    orr r0, r0, r1\n    push {{r0}}";
public Map I32Xor = "    pop {{r1}}\n    pop {{r0}}\n    eor r0, r0, r1\n    push {{r0}}";

// Comparison operations
public Map I32Eq = "    pop {{r1}}\n    pop {{r0}}\n    cmp r0, r1\n    moveq r0, #1\n    movne r0, #0\n    push {{r0}}";
public Map I32Ne = "    pop {{r1}}\n    pop {{r0}}\n    cmp r0, r1\n    movne r0, #1\n    moveq r0, #0\n    push {{r0}}";
public Map I32LtS = "    pop {{r1}}\n    pop {{r0}}\n    cmp r0, r1\n    movlt r0, #1\n    movge r0, #0\n    push {{r0}}";
public Map I32GtS = "    pop {{r1}}\n    pop {{r0}}\n    cmp r0, r1\n    movgt r0, #1\n    movle r0, #0\n    push {{r0}}";

// Local variable with tee
public Map LocalTee = "    pop {{r0}}\n    str r0, [r11, #-{offset}]\n    push {{r0}}";

// Branching
public Map Br = "    b {labelidx}";
public Map BrIf = "    pop {{r0}}\n    cmp r0, #0\n    bne {labelidx}";

// Memory operations
public Map I32Load = "    pop {{r0}}\n    ldr r1, [r0, #{offset}]\n    push {{r1}}";
public Map I32Store = "    pop {{r1}}\n    pop {{r0}}\n    str r1, [r0, #{offset}]";

// No operation
public Map Nop = "    nop";
```

### ARM64 Template Examples (to be added)

```csharp
// Division operations
public Map I32DivS = "    ldr w1, [sp], #4\n    ldr w0, [sp], #4\n    sdiv w0, w0, w1\n    str w0, [sp, #-4]!";
public Map I32DivU = "    ldr w1, [sp], #4\n    ldr w0, [sp], #4\n    udiv w0, w0, w1\n    str w0, [sp, #-4]!";

// Logical operations
public Map I32And = "    ldr w1, [sp], #4\n    ldr w0, [sp], #4\n    and w0, w0, w1\n    str w0, [sp, #-4]!";
public Map I32Or = "    ldr w1, [sp], #4\n    ldr w0, [sp], #4\n    orr w0, w0, w1\n    str w0, [sp, #-4]!";
public Map I32Xor = "    ldr w1, [sp], #4\n    ldr w0, [sp], #4\n    eor w0, w0, w1\n    str w0, [sp, #-4]!";

// Comparison operations
public Map I32Eq = "    ldr w1, [sp], #4\n    ldr w0, [sp], #4\n    cmp w0, w1\n    cset w0, eq\n    str w0, [sp, #-4]!";
public Map I32Ne = "    ldr w1, [sp], #4\n    ldr w0, [sp], #4\n    cmp w0, w1\n    cset w0, ne\n    str w0, [sp, #-4]!";
public Map I32LtS = "    ldr w1, [sp], #4\n    ldr w0, [sp], #4\n    cmp w0, w1\n    cset w0, lt\n    str w0, [sp, #-4]!";
public Map I32GtS = "    ldr w1, [sp], #4\n    ldr w0, [sp], #4\n    cmp w0, w1\n    cset w0, gt\n    str w0, [sp, #-4]!";

// Local variable with tee
public Map LocalTee = "    ldr w0, [sp], #4\n    str w0, [x29, #-{offset}]\n    str w0, [sp, #-4]!";

// Branching
public Map Br = "    b {labelidx}";
public Map BrIf = "    ldr w0, [sp], #4\n    cmp w0, #0\n    b.ne {labelidx}";

// Memory operations
public Map I32Load = "    ldr w0, [sp], #4\n    ldr w1, [x0, #{offset}]\n    str w1, [sp, #-4]!";
public Map I32Store = "    ldr w1, [sp], #4\n    ldr w0, [sp], #4\n    str w1, [x0, #{offset}]";

// No operation
public Map Nop = "    nop";
```

### x86_16 Template Examples (to be added)

```csharp
// Division operations
public Map I32DivS = "    pop bx\n    pop ax\n    cwd\n    idiv bx\n    push ax";
public Map I32DivU = "    pop bx\n    pop ax\n    xor dx, dx\n    div bx\n    push ax";

// Logical operations
public Map I32And = "    pop ax\n    pop bx\n    and ax, bx\n    push ax";
public Map I32Or = "    pop ax\n    pop bx\n    or ax, bx\n    push ax";
public Map I32Xor = "    pop ax\n    pop bx\n    xor ax, bx\n    push ax";

// Comparison operations
public Map I32Eq = "    pop bx\n    pop ax\n    cmp ax, bx\n    sete al\n    movzx ax, al\n    push ax";
public Map I32Ne = "    pop bx\n    pop ax\n    cmp ax, bx\n    setne al\n    movzx ax, al\n    push ax";
public Map I32LtS = "    pop bx\n    pop ax\n    cmp ax, bx\n    setl al\n    movzx ax, al\n    push ax";
public Map I32GtS = "    pop bx\n    pop ax\n    cmp ax, bx\n    setg al\n    movzx ax, al\n    push ax";

// Local variable with tee
public Map LocalTee = "    pop ax\n    mov [bp - {offset}], ax\n    push ax";

// Branching
public Map Br = "    jmp {labelidx}";
public Map BrIf = "    pop ax\n    test ax, ax\n    jnz {labelidx}";

// Memory operations
public Map I32Load = "    pop ax\n    mov bx, [ax + {offset}]\n    push bx";
public Map I32Store = "    pop bx\n    pop ax\n    mov [ax + {offset}], bx";

// No operation
public Map Nop = "    nop";
```

---

## Action Items

To complete the MapSet implementations:

1. **ARM32.cs** - Add 15 missing Map declarations (lines 8-35)
2. **ARM64.cs** - Add 15 missing Map declarations (lines 9-37)
3. **x86_16.cs** - Add 15 missing Map declarations (lines 8-36)

For each architecture:
- Use architecture-specific assembly syntax
- Follow the stack-based WebAssembly model
- Maintain consistency with the existing Maps in that file
- Reference x86_64.cs and x86_32.cs for complete examples

---

## Notes

- All MapSets use the CDTk `Map` type which are template strings
- Templates support placeholders like `{value}`, `{offset}`, `{labelidx}`, `{funcidx}`, `{fields}`, `{body}`, `{id}`, `{local_space}`
- The assembly generated is then processed by the Part 2 Assembler in each file
- ARM32 and ARM64 have complete Assembler implementations (Part 2) but incomplete MapSets (Part 1)
- x86_16 has both parts needing work
- x86_64 and x86_32 are fully complete and serve as reference implementations
