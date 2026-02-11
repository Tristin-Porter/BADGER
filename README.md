# BADGER

**B**etter **A**ssembler for **D**ependable **G**eneration of **E**fficient **R**esults

BADGER is a WebAssembly Text (WAT) to machine code assembler built with CDTk. It's designed as the canonical backend for the CRAB toolchain, supporting multiple architectures and output formats.

## Features

- ✅ **Complete WAT Token Set**: 200+ tokens covering the entire WebAssembly text format specification
- ✅ **CDTk-Based Pipeline**: Tokenization → Parsing → Code Generation → Assembly → Container Emission
- ✅ **Multi-Architecture Support**:
  - x86_64 (primary, full implementation)
  - x86_32
  - x86_16 (real mode)
  - ARM64 (AArch64)
  - ARM32
- ✅ **Multiple Output Formats**:
  - Native (bare metal flat binaries)
  - PE (Windows Portable Executable)
- ✅ **Modular Design**: Each architecture and container format is self-contained

## Quick Start

### Prerequisites

- .NET 10.0 SDK or later
- CDTk package (included via NuGet)

### Building

```bash
cd BADGER
dotnet build
```

### Running

```bash
# Basic usage
dotnet run -- input.wat

# Specify output file
dotnet run -- input.wat -o output.bin

# Target specific architecture
dotnet run -- input.wat --arch arm64 -o output.bin

# Generate Windows PE executable
dotnet run -- input.wat --format pe -o output.exe

# All options
dotnet run -- input.wat -o myprogram.bin --arch x86_32 --format native
```

### Command Line Options

- `-o <file>` - Output file path (default: `output.bin`)
- `--arch <arch>` - Target architecture: `x86_64`, `x86_32`, `x86_16`, `arm64`, `arm32` (default: `x86_64`)
- `--format <fmt>` - Output format: `native`, `pe` (default: `native`)

## Architecture

### Pipeline

```
WAT Input → [Tokens] → [Rules] → [MapSet] → Assembly → [Assembler] → Machine Code → [Emitter] → Binary
```

### Components

1. **Program.cs**
   - Complete WAT token definitions
   - CDTk grammar rules
   - Main compilation pipeline

2. **Architectures/** (Part 1: MapSet, Part 2: Assembler)
   - `x86_64.cs` - Full x86-64 implementation with instruction encoding
   - `x86_32.cs` - 32-bit x86 support
   - `x86_16.cs` - 16-bit real mode support
   - `ARM64.cs` - ARM AArch64 support
   - `ARM32.cs` - 32-bit ARM support

3. **Containers/**
   - `Native.cs` - Bare metal flat binary emission
   - `PE.cs` - Windows PE executable emission

## Example

Input WAT file (`add.wat`):
```wasm
(module
  (func (param i32) (param i32) (result i32)
    local.get 0
    local.get 1
    i32.add
  )
)
```

Compile to x86_64 native binary:
```bash
dotnet run -- add.wat -o add.bin
```

Compile to Windows PE executable:
```bash
dotnet run -- add.wat --format pe -o add.exe
```

## Project Structure

```
BADGER/
├── Program.cs              # Main pipeline with WAT tokens and rules
├── Architectures/          # Architecture-specific code generation
│   ├── x86_64.cs          # x86-64 (primary)
│   ├── x86_32.cs          # x86-32
│   ├── x86_16.cs          # x86-16
│   ├── ARM64.cs           # ARM AArch64
│   └── ARM32.cs           # ARM 32-bit
├── Containers/             # Binary container emitters
│   ├── Native.cs          # Flat binary
│   └── PE.cs              # Windows PE
├── Dependencies/           # CDTk framework
├── IMPLEMENTATION.md       # Detailed implementation docs
└── README.md              # This file
```

## Design Principles

1. **Spec Compliance**: Follows the BADGER specification exactly
2. **C# Only**: 100% C# implementation, no external languages
3. **CDTk Pipeline**: All parsing through CDTk framework
4. **Modular**: Each architecture is independent and isolated
5. **Deterministic**: Same input always produces same output
6. **Minimal**: Only supports what CRAB needs

## Status

Current implementation provides:
- ✅ Complete WAT token set (200+ tokens)
- ✅ CDTk pipeline architecture
- ✅ 5 target architectures
- ✅ 2 container formats
- ✅ End-to-end compilation pipeline
- 🚧 Full WAT grammar parsing (scaffolded, expanding with CDTk)
- 🚧 Complete instruction support (expanding)
- 🚧 Optimizations (future)

## Development

### Adding a New Architecture

1. Create `Architectures/NewArch.cs`
2. Implement `WATToNewArchMapSet` (Part 1: WAT→Assembly mapping)
3. Implement `Assembler.Assemble()` (Part 2: Assembly→Machine code)
4. Add to switch statements in `Program.cs`

### Adding a New Container Format

1. Create `Containers/NewFormat.cs`
2. Implement `Emit(byte[] machineCode)` method
3. Add to switch statement in `Program.cs`

## Contributing

This project is part of the CRAB toolchain. See contributing guidelines for details.

## License

[License information to be added]

## Related Projects

- **CRAB**: The compiler toolchain that BADGER serves
- **CDTk**: The Compiler Description Toolkit used for parsing
- **SHARK**: The runtime environment for BADGER binaries

---

**BADGER** - Transforming WAT into efficient machine code across multiple architectures.
