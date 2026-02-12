# ARM32 Assembler

Complete ARMv7 assembler implementation for BADGER.

## Quick Start

```csharp
using Badger.Architectures.ARM32;

var assembly = @"
main:
    push {r11, lr}
    mov r0, #42
    pop {r11, pc}
";

byte[] machineCode = Assembler.Assemble(assembly);
// machineCode contains ARM32 machine code bytes
```

## Instruction Reference

### Control Flow
| Instruction | Format | Example | Description |
|-------------|--------|---------|-------------|
| `bx` | `bx <reg>` | `bx lr` | Branch and exchange |
| `b` | `b <label>` | `b loop` | Unconditional branch |
| `beq` | `beq <label>` | `beq equal` | Branch if equal |
| `bne` | `bne <label>` | `bne notequal` | Branch if not equal |
| `blt` | `blt <label>` | `blt less` | Branch if less than |
| `bgt` | `bgt <label>` | `bgt greater` | Branch if greater than |
| `bl` | `bl <label>` | `bl func` | Branch with link (call) |
| `nop` | `nop` | `nop` | No operation |

### Data Movement
| Instruction | Format | Example | Description |
|-------------|--------|---------|-------------|
| `mov` | `mov <rd>, <op2>` | `mov r0, r1` | Move register |
| `mov` | `mov <rd>, #<imm>` | `mov r0, #42` | Move immediate |

### Arithmetic
| Instruction | Format | Example | Description |
|-------------|--------|---------|-------------|
| `add` | `add <rd>, <rn>, <op2>` | `add r0, r1, r2` | Add registers |
| `add` | `add <rd>, <rn>, #<imm>` | `add r0, r1, #8` | Add immediate |
| `sub` | `sub <rd>, <rn>, <op2>` | `sub r0, r1, r2` | Subtract registers |
| `sub` | `sub <rd>, <rn>, #<imm>` | `sub sp, sp, #16` | Subtract immediate |
| `mul` | `mul <rd>, <rn>, <rm>` | `mul r0, r1, r2` | Multiply |

### Logical Operations
| Instruction | Format | Example | Description |
|-------------|--------|---------|-------------|
| `and` | `and <rd>, <rn>, <rm>` | `and r0, r1, r2` | Bitwise AND |
| `orr` | `orr <rd>, <rn>, <rm>` | `orr r0, r1, r2` | Bitwise OR |
| `eor` | `eor <rd>, <rn>, <rm>` | `eor r0, r1, r2` | Bitwise XOR |

### Comparison
| Instruction | Format | Example | Description |
|-------------|--------|---------|-------------|
| `cmp` | `cmp <rn>, <op2>` | `cmp r0, r1` | Compare registers |
| `cmp` | `cmp <rn>, #<imm>` | `cmp r0, #5` | Compare immediate |

### Stack Operations
| Instruction | Format | Example | Description |
|-------------|--------|---------|-------------|
| `push` | `push {<regs>}` | `push {r0, r1}` | Push registers |
| `push` | `push {<regs>}` | `push {r11, lr}` | Push frame/link |
| `pop` | `pop {<regs>}` | `pop {r0, r1}` | Pop registers |
| `pop` | `pop {<regs>}` | `pop {r11, pc}` | Return from function |

### Memory Access
| Instruction | Format | Example | Description |
|-------------|--------|---------|-------------|
| `ldr` | `ldr <rt>, [<rn>, #<offset>]` | `ldr r0, [r1, #4]` | Load register |
| `ldr` | `ldr <rt>, [<rn>, #-<offset>]` | `ldr r0, [r11, #-4]` | Load with neg offset |
| `str` | `str <rt>, [<rn>, #<offset>]` | `str r0, [r1, #8]` | Store register |

## Register Reference

| Register | Alias | Purpose |
|----------|-------|---------|
| r0-r12 | - | General purpose |
| r13 | sp | Stack pointer |
| r14 | lr | Link register (return address) |
| r15 | pc | Program counter |

## Example Programs

### Simple Function
```arm
main:
    push {r11, lr}      @ Save frame and return
    mov r11, sp         @ Setup frame pointer
    
    mov r0, #42         @ Return value
    
    mov sp, r11         @ Restore stack
    pop {r11, pc}       @ Return
```

### Arithmetic
```arm
calculate:
    push {r11, lr}
    mov r11, sp
    
    mov r0, #10         @ a = 10
    mov r1, #5          @ b = 5
    add r2, r0, r1      @ c = a + b = 15
    sub r3, r2, r1      @ d = c - b = 10
    mul r4, r0, r1      @ e = a * b = 50
    
    mov r0, r4          @ Return e
    mov sp, r11
    pop {r11, pc}
```

### Conditional Logic
```arm
max:
    push {r11, lr}
    mov r11, sp
    
    @ r0 = first value
    @ r1 = second value
    cmp r0, r1          @ Compare
    bgt first_bigger    @ Branch if r0 > r1
    
    mov r0, r1          @ Return r1
    b done
    
first_bigger:
    @ r0 already has max value
    
done:
    mov sp, r11
    pop {r11, pc}
```

### Loop
```arm
sum_to_n:
    push {r11, lr}
    mov r11, sp
    
    @ r0 = n (input)
    mov r1, #0          @ sum = 0
    
loop:
    cmp r0, #0          @ Check if n == 0
    beq done            @ If so, exit loop
    
    add r1, r1, r0      @ sum += n
    sub r0, r0, #1      @ n--
    b loop              @ Continue
    
done:
    mov r0, r1          @ Return sum
    mov sp, r11
    pop {r11, pc}
```

## Technical Details

### Encoding
- All instructions are **32-bit** (4 bytes)
- **Little-endian** byte order
- **ARMv7** encoding patterns
- **ARM mode** (not Thumb)

### Condition Codes
| Code | Mnemonic | Description |
|------|----------|-------------|
| 0xE | AL | Always (unconditional) |
| 0x0 | EQ | Equal (Z set) |
| 0x1 | NE | Not equal (Z clear) |
| 0xB | LT | Less than (N != V) |
| 0xC | GT | Greater than (Z=0, N=V) |

### Branch Offsets
- **PC-relative** addressing
- PC = current instruction + 8 (ARM pipeline)
- Offset in **words** (4-byte units)
- **24-bit signed** range (±32MB)

### Immediate Encoding
ARM32 immediates use rotation encoding:
- **8-bit** immediate value
- **4-bit** rotation (even values: 0, 2, 4, ..., 30)
- Final value = immediate ROR (rotation × 2)

Examples:
- `#42` → `0x2A` with rotation 0
- `#255` → `0xFF` with rotation 0
- `#1024` → `0x04` with rotation 30 (0x400)

## Assembly Format

### Comments
```arm
@ This is a comment
mov r0, #1  @ Inline comment
```

### Labels
```arm
function_name:
    @ Instructions
loop_start:
    @ Loop body
    b loop_start
```

### Register Lists
```arm
push {r0, r1, r2}       @ Multiple registers
push {r11, lr}          @ Frame pointer and link register
pop {r11, pc}           @ Return (load PC)
```

## Error Handling

The assembler provides clear error messages:

```
Invalid register: r99
Immediate value 999999 cannot be encoded
Undefined label: missing_function
Branch offset 40000000 bytes out of range
```

## Testing

Run comprehensive tests:
```bash
dotnet run
```

All tests should show:
```
Test Results: 266 passed, 0 failed
```

ARM32-specific tests include:
- Basic instruction encoding
- Arithmetic operations
- Logical operations
- Comparisons
- Branch/label resolution
- Stack operations
- Load/store addressing
- Complete function assembly

## Integration with BADGER

### From WAT
```bash
badger input.wat --arch arm32 --format native -o output.bin
```

### Programmatic Usage
```csharp
// 1. Parse WAT
var watText = File.ReadAllText("input.wat");

// 2. Lower to ARM32 assembly
var mapSet = new WATToARM32MapSet();
// ... CDTk lowering ...

// 3. Assemble to machine code
var machineCode = Assembler.Assemble(assemblyText);

// 4. Emit container
var binary = Badger.Containers.Native.Emit(machineCode);
File.WriteAllBytes("output.bin", binary);
```

## Documentation

- **ARM32_IMPLEMENTATION.md** - Technical implementation details
- **ARM32_COMPLETION_SUMMARY.md** - Feature summary and test results
- **ARMv7 Architecture Reference Manual** - Official ARM specification

## License

Part of the BADGER project. See repository LICENSE file.
