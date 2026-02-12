# Container Formats

## Overview

BADGER produces executable machine code in two container formats:
- **Native** - Flat binary for bare metal execution
- **PE** - Windows Portable Executable format

BADGER does **not** support ELF, .deb, or any other binary formats as per the specification.

---

## Native Container Format

### Description

The Native container format produces a **flat binary** with no headers, metadata, or relocation information. The machine code starts at offset 0, making it suitable for direct execution in bare metal environments.

### Structure

```
[Byte 0]: First instruction
[Byte 1]: ...
[Byte N]: Last instruction
```

### Characteristics

- **No headers**: Binary is pure machine code
- **No metadata**: No symbol table, debug info, or version info
- **No relocations**: Code must be position-independent or loaded at a fixed address
- **Entry point**: Always at offset 0
- **File size**: Exactly the size of the machine code

### Use Cases

1. **Bootloaders** - BIOS/UEFI boot sectors
2. **Bare Metal** - Direct hardware execution
3. **Embedded Systems** - Microcontroller firmware
4. **Virtual Machines** - QEMU, SHARK, custom VMs
5. **Operating System Kernels** - Early boot code
6. **Testing** - Direct CPU testing without OS overhead

### Loading

To load a Native binary:

```
1. Read entire file into memory buffer
2. Set instruction pointer to buffer start
3. Execute
```

### Example

```bash
# Compile to native binary
dotnet run -- program.wat -o boot.bin --arch x86_16 --format native

# Run in QEMU
qemu-system-i386 -drive format=raw,file=boot.bin

# Or hex dump to inspect
hexdump -C boot.bin
```

### File Extension

Typically `.bin` but can be anything (`.img`, `.raw`, etc.)

---

## PE Container Format

### Description

The PE (Portable Executable) container format produces a **Windows executable** with minimal headers sufficient for the Windows loader to execute the program.

### Structure

```
[DOS Header]        64 bytes  - MZ signature
[DOS Stub]          64 bytes  - "This program cannot be run in DOS mode"
[PE Signature]       4 bytes  - PE\0\0
[COFF Header]       20 bytes  - Machine type, sections, etc.
[Optional Header]  240 bytes  - Entry point, image base, etc.
[Section Table]     40 bytes  - .text section descriptor
[Padding]          variable  - Align to 512 bytes
[.text Section]    variable  - Machine code
[Padding]          variable  - Align to 512 bytes
```

### DOS Header (64 bytes)

```c
Offset  Field               Value
------  -----               -----
0x00    e_magic            0x5A4D ('MZ')
0x02    e_cblp             0x0090
0x04    e_cp               0x0003
0x08    e_cparhdr          0x0004
0x10    e_sp               0x00B8
0x3C    e_lfanew           0x0080 (PE header offset)
```

### PE Signature (4 bytes)

```
0x50 0x45 0x00 0x00  ('PE\0\0')
```

### COFF Header (20 bytes)

```c
Offset  Field                  Value
------  -----                  -----
0x00    Machine                0x8664 (x86-64) or arch-specific
0x02    NumberOfSections       0x0001
0x04    TimeDateStamp          0x00000000
0x08    PointerToSymbolTable   0x00000000
0x0C    NumberOfSymbols        0x00000000
0x10    SizeOfOptionalHeader   0x00F0 (240 bytes)
0x12    Characteristics        0x0022 (executable, large address aware)
```

### Optional Header (240 bytes for PE32+)

```c
Offset  Field                   Value
------  -----                   -----
0x00    Magic                   0x020B (PE32+)
0x02    Linker Version          14.0
0x04    SizeOfCode              [code size]
0x10    AddressOfEntryPoint     0x1000 (RVA)
0x14    BaseOfCode              0x1000
0x18    ImageBase               0x0000000000400000
0x20    SectionAlignment        0x1000 (4KB)
0x24    FileAlignment           0x0200 (512B)
0x28    OS Version              5.2
0x38    SizeOfImage             0x3000
0x3C    SizeOfHeaders           0x0200
0x44    Subsystem               0x0003 (console)
0x6C    NumberOfRvaAndSizes     16
```

### Section Table (40 bytes per section)

```c
Offset  Field                   Value
------  -----                   -----
0x00    Name                    ".text\0\0\0"
0x08    VirtualSize             [code size]
0x0C    VirtualAddress          0x1000
0x10    SizeOfRawData           [aligned size]
0x14    PointerToRawData        0x0200
0x24    Characteristics         0x60000020 (code, executable, readable)
```

### Characteristics

- **Minimal**: Only essential headers included
- **Single section**: Just .text for code
- **No imports**: Self-contained
- **No relocations**: Fixed base address
- **No debug info**: Stripped for size
- **Console subsystem**: Text-based execution

### Use Cases

1. **Windows Applications** - Console programs
2. **Windows Drivers** - Kernel-mode drivers (with modifications)
3. **Windows Bootloaders** - UEFI boot applications
4. **Testing** - Running on Windows without OS loader complexity

### Loading

The Windows PE loader:

```
1. Reads DOS header, validates MZ signature
2. Reads PE signature at e_lfanew offset
3. Parses COFF and Optional headers
4. Maps sections to memory at ImageBase
5. Jumps to AddressOfEntryPoint
```

### Example

```bash
# Compile to PE executable
dotnet run -- program.wat -o program.exe --arch x86_64 --format pe

# Run on Windows
program.exe

# Or inspect with PE tools
dumpbin /headers program.exe  # Windows
objdump -x program.exe         # Linux/WSL
```

### File Extension

Typically `.exe` for executables, `.dll` for libraries (though BADGER only produces executables).

---

## Container Comparison

| Feature | Native | PE |
|---------|--------|----|
| Headers | None | Full PE structure |
| File Size | Minimal (code only) | Larger (headers + code) |
| Entry Point | Offset 0 | Specified in header |
| Metadata | None | Version, subsystem, etc. |
| Relocations | None | None (fixed base) |
| Platform | Any (bare metal) | Windows only |
| Debugging | Difficult | Supported by debuggers |
| Loading | Manual | OS loader |

## Implementation

### Native Container (`Containers/Native.cs`)

```csharp
public static byte[] Emit(byte[] machineCode)
{
    // Native is just pass-through
    return machineCode;
}
```

### PE Container (`Containers/PE.cs`)

```csharp
public static byte[] Emit(byte[] machineCode)
{
    var pe = new List<byte>();
    
    // Add DOS header
    pe.AddRange(CreateDOSHeader());
    
    // Add DOS stub
    pe.AddRange(CreateDOSStub());
    
    // Add PE signature
    pe.AddRange(new byte[] { 0x50, 0x45, 0x00, 0x00 });
    
    // Add COFF header
    pe.AddRange(CreateCOFFHeader());
    
    // Add Optional header
    pe.AddRange(CreateOptionalHeader(imageBase, entryRVA, codeSize));
    
    // Add section table
    pe.AddRange(CreateCodeSection(codeSize));
    
    // Align headers to 512 bytes
    while (pe.Count % 512 != 0)
        pe.Add(0);
    
    // Add machine code
    pe.AddRange(machineCode);
    
    // Align to 512 bytes
    while (pe.Count % 512 != 0)
        pe.Add(0);
    
    return pe.ToArray();
}
```

## Architecture Compatibility

Both container formats work with all architectures:

| Container | x86_64 | x86_32 | x86_16 | ARM64 | ARM32 |
|-----------|--------|--------|--------|-------|-------|
| Native    | ✅     | ✅     | ✅     | ✅    | ✅    |
| PE        | ✅     | ✅     | ✅     | ✅    | ✅    |

The PE machine type field is automatically set based on the target architecture:
- x86_64: 0x8664
- x86_32: 0x014C
- x86_16: 0x014C
- ARM64: 0xAA64
- ARM32: 0x01C4

## Testing

Both container formats have comprehensive test coverage:

### Native Tests
- Pass-through verification
- Size preservation
- Multiple architectures

### PE Tests
- DOS header validation (MZ signature)
- PE signature validation
- COFF header machine type
- Optional header structure
- Section table format
- File alignment verification

Total: 8 container-specific tests + architecture integration tests.

## Specification Compliance

✅ **Native format**: Flat binary, entry at offset 0  
✅ **PE format**: Minimal Windows executable  
✅ **NO ELF**: Not implemented (per spec)  
✅ **NO .deb**: Not implemented (per spec)  
✅ **NO other formats**: Only Native and PE supported

## References

- [PE Format Specification](https://docs.microsoft.com/en-us/windows/win32/debug/pe-format)
- [Flat Binary Format](https://en.wikipedia.org/wiki/Flat_binary)
