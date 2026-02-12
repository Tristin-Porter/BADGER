# Getting Started with BADGER

## Overview

BADGER (Better Assembler for Dependable Generation of Efficient Results) is a WebAssembly Text (WAT) assembler that compiles WAT to native machine code for multiple architectures.

## Installation

### Prerequisites

- .NET 10.0 SDK or later
- Windows, Linux, or macOS

### Building from Source

```bash
git clone https://github.com/Tristin-Porter/BADGER.git
cd BADGER
dotnet build
```

## Basic Usage

### Command Line Interface

```bash
dotnet run -- <input.wat> [options]
```

### Options

- `-o <output>` - Specify output file path (default: output.bin)
- `--arch <architecture>` - Target architecture: x86_64, x86_32, x86_16, arm64, arm32 (default: x86_64)
- `--format <format>` - Output format: native, pe (default: native)

### Examples

#### Compile WAT to x86_64 native binary

```bash
dotnet run -- input.wat -o output.bin
```

#### Compile to ARM64 PE executable

```bash
dotnet run -- input.wat -o output.exe --arch arm64 --format pe
```

#### Compile to 32-bit x86 for bare metal

```bash
dotnet run -- input.wat -o bootloader.bin --arch x86_32 --format native
```

## Supported Architectures

- **x86_64** - 64-bit x86 (AMD64/Intel 64)
- **x86_32** - 32-bit x86 (IA-32/i386)
- **x86_16** - 16-bit x86 (Real Mode)
- **ARM64** - 64-bit ARM (AArch64)
- **ARM32** - 32-bit ARM (ARMv7)

## Container Formats

### Native Format

Produces a flat binary with no headers or metadata. The entry point is at offset 0.

**Use cases:**
- Bootloaders
- Bare metal programming
- Embedded systems
- QEMU direct execution
- SHARK virtual machines

### PE Format

Produces a Windows Portable Executable with minimal headers.

**Use cases:**
- Windows console applications
- Windows kernel drivers
- Windows bootloaders

## Quick Start Example

Create a simple WAT file `hello.wat`:

```wasm
(module
  (func $main (result i32)
    (i32.const 42)
  )
  (export "main" (func $main))
)
```

Compile it:

```bash
dotnet run -- hello.wat -o hello.bin --arch x86_64 --format native
```

This will generate a native binary with the compiled machine code.

## Testing

BADGER includes a comprehensive test suite. Run all tests:

```bash
dotnet run
```

Tests are automatically executed on program start and include:
- WAT token parsing tests
- Instruction encoding tests for all architectures
- Container format validation tests
- End-to-end integration tests

## Next Steps

- Read [Architecture.md](Architecture.md) to understand each target architecture
- Read [ContainerFormats.md](ContainerFormats.md) for container format details
- Read [API.md](API.md) for programmatic API usage
- Read [Testing.md](Testing.md) for testing guidelines

## Troubleshooting

### Build Errors

If you encounter build errors, ensure:
1. .NET 10.0 SDK is installed: `dotnet --version`
2. All dependencies are restored: `dotnet restore`
3. Project builds cleanly: `dotnet clean && dotnet build`

### Runtime Errors

If the program crashes:
1. Verify your WAT file syntax
2. Check that the target architecture is supported
3. Ensure output path is writable
4. Review error messages for specific issues

## Support

For issues, feature requests, or contributions:
- GitHub Issues: https://github.com/Tristin-Porter/BADGER/issues
- Documentation: See the Documentation/ folder
- Specification: See `.github/agents/badger-spec.txt`
