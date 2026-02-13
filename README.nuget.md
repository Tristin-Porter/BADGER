# BADGER - Better Assembler for Dependable Generation of Efficient Results

A C# library that compiles WebAssembly Text (WAT) format to native machine code for multiple architectures.

## Features

- **Multi-Architecture Support**: Compile to x86_64, x86_32, x86_16, ARM64, and ARM32
- **Container Formats**: Output as Native (bare metal) or PE (Portable Executable) format
- **Simple API**: Easy-to-use static methods for compilation
- **Pure C#**: Built entirely in C# with no external dependencies except CDTk

## Installation

```bash
dotnet add package Badger
```

## Quick Start

### Basic Usage

```csharp
using Badger;

// Compile WAT source to native x86_64 binary
var watSource = "(module)";
byte[] binary = BadgerAssembler.Compile(watSource);

// Save to file
File.WriteAllBytes("output.bin", binary);
```

### Compile for Different Architectures

```csharp
// ARM64
byte[] armBinary = BadgerAssembler.Compile(
    watSource, 
    BadgerAssembler.Architecture.ARM64);

// x86_32
byte[] x86Binary = BadgerAssembler.Compile(
    watSource,
    BadgerAssembler.Architecture.x86_32);
```

### PE Format Output

```csharp
// Generate PE executable
byte[] peBinary = BadgerAssembler.Compile(
    watSource,
    BadgerAssembler.Architecture.x86_64,
    BadgerAssembler.ContainerFormat.PE);
```

### File-Based Operations

```csharp
// Compile from file
byte[] binary = BadgerAssembler.CompileFile("input.wat");

// Compile file to file
BadgerAssembler.CompileToFile(
    "input.wat", 
    "output.bin",
    BadgerAssembler.Architecture.x86_64,
    BadgerAssembler.ContainerFormat.Native);
```

## Supported Architectures

- `Architecture.x86_64` - 64-bit x86 (default)
- `Architecture.x86_32` - 32-bit x86
- `Architecture.x86_16` - 16-bit x86
- `Architecture.ARM64` - 64-bit ARM (AArch64)
- `Architecture.ARM32` - 32-bit ARM

## Supported Container Formats

- `ContainerFormat.Native` - Bare metal binary (default)
- `ContainerFormat.PE` - Portable Executable format

## API Reference

### BadgerAssembler.Compile

```csharp
public static byte[] Compile(
    string watSource,
    Architecture architecture = Architecture.x86_64,
    ContainerFormat format = ContainerFormat.Native)
```

Compiles WAT source code to native machine code.

### BadgerAssembler.CompileFile

```csharp
public static byte[] CompileFile(
    string inputFilePath,
    Architecture architecture = Architecture.x86_64,
    ContainerFormat format = ContainerFormat.Native)
```

Compiles WAT from a file to native machine code.

### BadgerAssembler.CompileToFile

```csharp
public static void CompileToFile(
    string inputFilePath,
    string outputFilePath,
    Architecture architecture = Architecture.x86_64,
    ContainerFormat format = ContainerFormat.Native)
```

Compiles WAT from a file and writes the output to a file.

## License

MIT License - See LICENSE.txt for details

## Links

- [GitHub Repository](https://github.com/Tristin-Porter/BADGER)
- [Documentation](https://github.com/Tristin-Porter/BADGER/tree/main/Documentation)
- [Report Issues](https://github.com/Tristin-Porter/BADGER/issues)
