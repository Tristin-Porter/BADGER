# BADGER API Reference

## Overview

BADGER provides both a command-line interface and a programmatic C# API for integration into other projects.

## Programmatic API

### Namespaces

```csharp
using Badger;
using Badger.Architectures.x86_64;
using Badger.Architectures.x86_32;
using Badger.Architectures.x86_16;
using Badger.Architectures.ARM64;
using Badger.Architectures.ARM32;
using Badger.Containers;
using CDTk;
```

---

## Architecture Assemblers

Each architecture provides a static `Assembler` class with an `Assemble` method.

### x86_64.Assembler

```csharp
namespace Badger.Architectures.x86_64;

public static class Assembler
{
    /// <summary>
    /// Assembles x86_64 assembly text to machine code.
    /// </summary>
    /// <param name="assemblyText">Assembly source code</param>
    /// <returns>Machine code bytes</returns>
    public static byte[] Assemble(string assemblyText);
}
```

**Example Usage:**

```csharp
string asm = @"
main:
    push rbp
    mov rbp, rsp
    mov rax, 42
    pop rbp
    ret
";

byte[] machineCode = Badger.Architectures.x86_64.Assembler.Assemble(asm);
```

### x86_32.Assembler

```csharp
namespace Badger.Architectures.x86_32;

public static class Assembler
{
    /// <summary>
    /// Assembles x86_32 assembly text to machine code.
    /// </summary>
    public static byte[] Assemble(string assemblyText);
}
```

**Example Usage:**

```csharp
string asm = @"
main:
    push ebp
    mov ebp, esp
    mov eax, 42
    pop ebp
    ret
";

byte[] machineCode = Badger.Architectures.x86_32.Assembler.Assemble(asm);
```

### x86_16.Assembler

```csharp
namespace Badger.Architectures.x86_16;

public static class Assembler
{
    /// <summary>
    /// Assembles x86_16 assembly text to machine code.
    /// </summary>
    public static byte[] Assemble(string assemblyText);
}
```

### ARM64.Assembler

```csharp
namespace Badger.Architectures.ARM64;

public static class Assembler
{
    /// <summary>
    /// Assembles ARM64/AArch64 assembly text to machine code.
    /// </summary>
    public static byte[] Assemble(string assemblyText);
}
```

**Example Usage:**

```csharp
string asm = @"
main:
    stp x29, x30, [sp, #-16]!
    mov x29, sp
    mov w0, #42
    ldp x29, x30, [sp], #16
    ret
";

byte[] machineCode = Badger.Architectures.ARM64.Assembler.Assemble(asm);
```

### ARM32.Assembler

```csharp
namespace Badger.Architectures.ARM32;

public static class Assembler
{
    /// <summary>
    /// Assembles ARM32 assembly text to machine code.
    /// </summary>
    public static byte[] Assemble(string assemblyText);
}
```

**Example Usage:**

```csharp
string asm = @"
main:
    push {r11, lr}
    mov r11, sp
    mov r0, #42
    pop {r11, pc}
";

byte[] machineCode = Badger.Architectures.ARM32.Assembler.Assemble(asm);
```

---

## Container Emitters

### Native.Emit

```csharp
namespace Badger.Containers;

public static class Native
{
    /// <summary>
    /// Emits a flat binary (pass-through).
    /// </summary>
    /// <param name="machineCode">Machine code bytes</param>
    /// <returns>Native binary (same as input)</returns>
    public static byte[] Emit(byte[] machineCode);
}
```

**Example Usage:**

```csharp
byte[] machineCode = /* ... */;
byte[] nativeBinary = Badger.Containers.Native.Emit(machineCode);

// Write to file
File.WriteAllBytes("output.bin", nativeBinary);
```

### PE.Emit

```csharp
namespace Badger.Containers;

public static class PE
{
    /// <summary>
    /// Emits a Windows PE executable.
    /// </summary>
    /// <param name="machineCode">Machine code bytes</param>
    /// <returns>PE binary with headers</returns>
    public static byte[] Emit(byte[] machineCode);
}
```

**Example Usage:**

```csharp
byte[] machineCode = /* ... */;
byte[] peBinary = Badger.Containers.PE.Emit(machineCode);

// Write to file
File.WriteAllBytes("output.exe", peBinary);
```

---

## WAT Parsing (CDTk Integration)

### WATTokens

```csharp
public class WATTokens : TokenSet
{
    // Module structure
    public Token Module;
    public Token Func;
    public Token Param;
    public Token Result;
    public Token Local;
    
    // Instructions
    public Token I32Add;
    public Token I32Sub;
    public Token I32Mul;
    public Token I32Const;
    
    // ... and 200+ more tokens
}
```

### WATRules

```csharp
public class WATRules : RuleSet
{
    // Module structure rules
    public Rule Module;
    public Rule ModuleField;
    public Rule FunctionDef;
    
    // Instruction rules
    public Rule Instruction;
    public Rule NumericInstr;
    public Rule ControlInstr;
    
    // ... and 70+ more rules
}
```

**Example Usage:**

```csharp
// Create token set
var tokens = new WATTokens();

// Create rule set
var rules = new WATRules();

// Parse WAT (when CDTk GLL parser is integrated)
// var ast = CDTkParser.Parse(watSource, tokens, rules);
```

---

## Complete Pipeline Example

### End-to-End Compilation

```csharp
using System;
using System.IO;
using Badger.Architectures.x86_64;
using Badger.Containers;

public class Example
{
    public static void CompileProgram(string watFile, string outputFile, 
                                     string arch, string format)
    {
        // 1. Read WAT input
        string watSource = File.ReadAllText(watFile);
        
        // 2. Parse WAT (simplified - actual parsing uses CDTk)
        // For now, we work with assembly directly
        
        // 3. Lower to assembly (architecture-specific)
        string assembly = GenerateAssembly(watSource, arch);
        
        // 4. Assemble to machine code
        byte[] machineCode = arch.ToLower() switch
        {
            "x86_64" => x86_64.Assembler.Assemble(assembly),
            "x86_32" => x86_32.Assembler.Assemble(assembly),
            "x86_16" => x86_16.Assembler.Assemble(assembly),
            "arm64" => ARM64.Assembler.Assemble(assembly),
            "arm32" => ARM32.Assembler.Assemble(assembly),
            _ => throw new ArgumentException($"Unknown architecture: {arch}")
        };
        
        // 5. Emit container
        byte[] binary = format.ToLower() switch
        {
            "native" => Native.Emit(machineCode),
            "pe" => PE.Emit(machineCode),
            _ => throw new ArgumentException($"Unknown format: {format}")
        };
        
        // 6. Write output
        File.WriteAllBytes(outputFile, binary);
        
        Console.WriteLine($"Compiled {watFile} to {outputFile}");
        Console.WriteLine($"Architecture: {arch}, Format: {format}");
        Console.WriteLine($"Machine code: {machineCode.Length} bytes");
        Console.WriteLine($"Binary size: {binary.Length} bytes");
    }
    
    private static string GenerateAssembly(string wat, string arch)
    {
        // Simplified - actual implementation uses CDTk MapSets
        // This is just for demonstration
        return @"
main:
    push rbp
    mov rbp, rsp
    mov rax, 42
    pop rbp
    ret
";
    }
}
```

### Usage

```csharp
// Compile WAT to x86_64 native binary
Example.CompileProgram("input.wat", "output.bin", "x86_64", "native");

// Compile WAT to ARM64 PE executable
Example.CompileProgram("input.wat", "output.exe", "arm64", "pe");
```

---

## Error Handling

All assemblers throw exceptions on errors:

```csharp
try
{
    byte[] code = x86_64.Assembler.Assemble(assembly);
}
catch (ArgumentException ex)
{
    // Invalid instruction or operand
    Console.WriteLine($"Assembly error: {ex.Message}");
}
catch (Exception ex)
{
    // Other errors
    Console.WriteLine($"Error: {ex.Message}");
}
```

Common errors:
- `Unknown register: reg` - Invalid register name
- `Unknown instruction: instr` - Unsupported instruction
- `Invalid operands for INSTR` - Wrong operand types
- `Label not found: label` - Undefined label

---

## Testing API

### Running Tests Programmatically

```csharp
using Badger;

// Run all tests
Testing.RunAllTests();

// Tests print results to console
// Returns void, but sets exit code on failure
```

---

## MapSet API (WAT → Assembly)

Each architecture provides a MapSet for WAT lowering:

```csharp
using CDTk;

// x86_64 MapSet
var mapSet = new Badger.Architectures.x86_64.WATToX86_64MapSet();

// Access templates
string funcTemplate = mapSet.Function;
string addTemplate = mapSet.I32Add;
string constTemplate = mapSet.I32Const;

// Templates use {placeholder} syntax for substitution
// Example: Function = "{id}:\n    push rbp\n    mov rbp, rsp\n{body}\n    pop rbp\n    ret"
```

---

## Best Practices

### 1. Assembly Input Validation

Always validate assembly input before passing to assemblers:

```csharp
if (string.IsNullOrWhiteSpace(assembly))
{
    throw new ArgumentException("Assembly cannot be empty");
}
```

### 2. Output Verification

Verify output size is reasonable:

```csharp
byte[] code = Assembler.Assemble(assembly);
if (code.Length == 0)
{
    throw new InvalidOperationException("No code generated");
}
```

### 3. Container Selection

Choose appropriate container for target platform:

```csharp
bool isWindows = RuntimeInformation.IsOSPlatform(OSPlatform.Windows);
string format = isWindows ? "pe" : "native";
```

### 4. Error Messages

Provide helpful context in error messages:

```csharp
try
{
    byte[] code = Assembler.Assemble(asm);
}
catch (Exception ex)
{
    Console.WriteLine($"Failed to assemble {filename}: {ex.Message}");
    Console.WriteLine($"Assembly:\n{asm}");
}
```

---

## Performance Considerations

### Assembly Size

- x86 instructions: 1-15 bytes (variable)
- ARM instructions: 4 bytes (fixed)
- Labels: No overhead in final code

### Memory Usage

- Assemblers allocate ~1KB for label tables
- Output size is proportional to instruction count
- PE headers add ~512 bytes minimum

### Speed

- Two-pass assembly is O(n) where n = instruction count
- Label resolution is O(m) where m = label count
- Typical speed: 10,000+ instructions/second

---

## Integration Examples

### ASP.NET Core

```csharp
[ApiController]
[Route("api/[controller]")]
public class CompileController : ControllerBase
{
    [HttpPost]
    public IActionResult Compile([FromBody] CompileRequest request)
    {
        try
        {
            byte[] code = x86_64.Assembler.Assemble(request.Assembly);
            byte[] binary = Native.Emit(code);
            return File(binary, "application/octet-stream", "output.bin");
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }
}
```

### Console Application

```csharp
static void Main(string[] args)
{
    if (args.Length < 2)
    {
        Console.WriteLine("Usage: badger <input> <output>");
        return;
    }
    
    string asm = File.ReadAllText(args[0]);
    byte[] code = x86_64.Assembler.Assemble(asm);
    byte[] binary = Native.Emit(code);
    File.WriteAllBytes(args[1], binary);
}
```

---

## Reference

For more details, see:
- [Architecture.md](Architecture.md) - Architecture-specific details
- [ContainerFormats.md](ContainerFormats.md) - Container format specifications
- [Testing.md](Testing.md) - Testing guidelines
- Source code in `Architectures/` and `Containers/` directories
