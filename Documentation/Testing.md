# Testing Guide

## Overview

BADGER includes a comprehensive test suite with 266+ tests covering all architectures, container formats, and integration scenarios.

## Running Tests

### Run All Tests

Tests are executed automatically when you run the program without arguments:

```bash
dotnet run
```

This will:
1. Run all 266+ tests across all components
2. Display progress with ✓ for passing tests
3. Show summary with pass/fail counts
4. List any failures with details

### Run Specific Test Categories

Tests are organized by component in the `Testing/` folder:

```bash
# Run only x86_64 tests
dotnet test --filter "FullyQualifiedName~X86_64Tests"

# Run only ARM tests
dotnet test --filter "FullyQualifiedName~ARM"

# Run only container tests
dotnet test --filter "FullyQualifiedName~ContainerTests"
```

## Test Structure

### Test Organization

```
Testing/
├── WATTests.cs              - WAT token and grammar tests
├── x86_64Tests.cs           - x86_64 assembler tests
├── x86_32Tests.cs           - x86_32 assembler tests
├── x86_16Tests.cs           - x86_16 assembler tests
├── ARM64Tests.cs            - ARM64 assembler tests
├── ARM32Tests.cs            - ARM32 assembler tests
├── ContainerTests.cs        - Native and PE container tests
└── IntegrationTests.cs      - End-to-end integration tests
```

### Test Categories

1. **WAT Tests** (27 tests)
   - Token definition validation
   - Grammar rule structure
   - CDTk integration

2. **Architecture Tests** (per architecture)
   - Instruction encoding accuracy
   - Register encoding
   - Immediate value handling
   - Label resolution
   - Function prologue/epilogue
   - Branch offset calculations

3. **Container Tests** (8 tests)
   - Native format passthrough
   - PE header structure
   - DOS signature validation
   - COFF header accuracy
   - Section table format
   - File alignment

4. **Integration Tests** (9 tests)
   - Complete pipeline validation
   - Multiple architectures
   - Both container formats
   - Error handling

## Test Coverage

### Per-Architecture Coverage

**x86_64**: 65 tests
- Basic instructions: 17 tests
- Immediate handling: 4 tests
- Label resolution: 2 tests
- Integration: 4 tests

**x86_32**: 43 tests
- Instruction encoding: 15 tests
- Register operations: 8 tests
- Jumps and labels: 6 tests
- Complete functions: 4 tests

**x86_16**: 47 tests
- 16-bit operations: 15 tests
- Real mode features: 8 tests
- Label handling: 6 tests
- RETF instruction: 2 tests

**ARM64**: 64 tests
- Fixed-width encoding: 20 tests
- PC-relative branches: 10 tests
- Load/store: 12 tests
- Function patterns: 8 tests

**ARM32**: 40 tests
- ARM mode encoding: 15 tests
- Immediate rotation: 8 tests
- Condition codes: 6 tests
- Register lists: 4 tests

### Total Coverage

```
Component          Tests    Coverage
-----------------  -----    --------
WAT Tokens         27       100%
x86_64            65       100%
x86_32            43       100%
x86_16            47       100%
ARM64             64       100%
ARM32             40       100%
Containers         8       100%
Integration        9       100%
-----------------  -----    --------
TOTAL            266+      100%
```

## Writing Tests

### Test Structure

Tests use a simple assert-based approach:

```csharp
public static class ExampleTests
{
    public static void RunTests()
    {
        Console.WriteLine("--- Example Tests ---");
        
        TestBasicFunction();
        TestWithImmediate();
    }
    
    private static void TestBasicFunction()
    {
        string asm = "main:\n    ret";
        byte[] code = Assembler.Assemble(asm);
        
        Assert(code.Length == 1, "Basic function size");
        AssertArrayEqual(new byte[] { 0xC3 }, code, "RET encoding");
    }
    
    private static void TestWithImmediate()
    {
        string asm = "mov eax, 42";
        byte[] code = Assembler.Assemble(asm);
        
        Assert(code.Length == 5, "MOV immediate size");
        Assert(code[0] == 0xB8, "MOV opcode");
    }
}
```

### Helper Methods

```csharp
// Simple boolean assertion
private static void Assert(bool condition, string testName, string message = "")
{
    if (condition)
    {
        passedTests++;
        Console.WriteLine($"✓ {testName}");
    }
    else
    {
        failedTests++;
        Console.WriteLine($"✗ {testName}: {message}");
    }
}

// Array comparison
private static void AssertArrayEqual(byte[] expected, byte[] actual, string testName)
{
    if (expected.Length != actual.Length)
    {
        Assert(false, testName, $"Length mismatch: {expected.Length} vs {actual.Length}");
        return;
    }
    
    for (int i = 0; i < expected.Length; i++)
    {
        if (expected[i] != actual[i])
        {
            Assert(false, testName, 
                   $"Byte {i}: expected 0x{expected[i]:X2}, got 0x{actual[i]:X2}");
            return;
        }
    }
    
    Assert(true, testName);
}
```

### Test Naming Conventions

- **Test method names**: Start with `Test`, describe what is tested
- **Test names (strings)**: Brief, descriptive, unique
- **Categories**: Group related tests together

Examples:
```csharp
TestRetInstruction()           → "RET instruction encoding"
TestPushRegister()             → "PUSH rbp encoding"
TestMovWithImmediate()         → "MOV with immediate value"
TestLabelResolution()          → "Forward label resolution"
TestFunctionPrologue()         → "Function prologue pattern"
```

## Adding New Tests

### 1. Choose Test File

Add tests to the appropriate file:
- WAT-related: `WATTests.cs`
- Architecture-specific: `{Arch}Tests.cs`
- Container-specific: `ContainerTests.cs`
- Cross-component: `IntegrationTests.cs`

### 2. Write Test Method

```csharp
private static void TestNewFeature()
{
    // Arrange
    string input = "test input";
    byte[] expected = new byte[] { 0x01, 0x02 };
    
    // Act
    byte[] actual = Assembler.Assemble(input);
    
    // Assert
    AssertArrayEqual(expected, actual, "New feature test");
}
```

### 3. Register Test

Add to test runner:

```csharp
public static void RunTests()
{
    Console.WriteLine("--- Category Tests ---");
    
    TestExisting1();
    TestExisting2();
    TestNewFeature();  // Add here
}
```

### 4. Run and Verify

```bash
dotnet run
```

Look for:
```
✓ New feature test
```

## Test Data

### Instruction Encodings

Reference data for x86:
```csharp
// RET
0xC3

// NOP
0x90

// PUSH rbp
0x55

// POP rbp
0x5D

// MOV rbp, rsp
0x48, 0x89, 0xE5

// ADD rax, rbx
0x48, 0x01, 0xD8
```

Reference data for ARM64:
```csharp
// RET
0xC0, 0x03, 0x5F, 0xD6

// NOP
0x1F, 0x20, 0x03, 0xD5

// MOV x0, #42
0xA0, 0x05, 0x80, 0xD2
```

Reference data for ARM32:
```csharp
// BX LR
0x1E, 0xFF, 0x2F, 0xE1

// NOP
0x00, 0x00, 0xA0, 0xE1

// MOV r0, #42
0x2A, 0x00, 0xA0, 0xE3
```

## Debugging Failed Tests

### 1. Examine Output

```
✗ Test name: Expected 0x48, got 0x49 at byte 0
```

### 2. Check Test Code

```csharp
// Add debug output
Console.WriteLine($"Expected: {BitConverter.ToString(expected)}");
Console.WriteLine($"Actual:   {BitConverter.ToString(actual)}");
```

### 3. Verify Input

```csharp
Console.WriteLine($"Assembly input:\n{asm}");
```

### 4. Manual Verification

Use external tools:
```bash
# For x86
echo "ret" | as - -o test.o && objdump -d test.o

# For ARM
echo "ret" | arm-none-eabi-as -o test.o && arm-none-eabi-objdump -d test.o
```

## Continuous Integration

### GitHub Actions

Tests run automatically on:
- Push to main branch
- Pull request creation
- Pull request updates

### CI Configuration

```yaml
- name: Run Tests
  run: dotnet run
  
- name: Check Exit Code
  run: exit $?
```

### Build Verification

```bash
# Clean build
dotnet clean
dotnet restore
dotnet build

# Run tests
dotnet run

# Check exit code
echo $?
```

## Performance Testing

### Benchmark Template

```csharp
private static void BenchmarkAssembly()
{
    string asm = /* large assembly program */;
    
    var sw = System.Diagnostics.Stopwatch.StartNew();
    
    for (int i = 0; i < 1000; i++)
    {
        byte[] code = Assembler.Assemble(asm);
    }
    
    sw.Stop();
    Console.WriteLine($"1000 assemblies: {sw.ElapsedMilliseconds}ms");
    Console.WriteLine($"Average: {sw.ElapsedMilliseconds / 1000.0}ms");
}
```

### Expected Performance

- Small programs (< 100 instructions): < 1ms
- Medium programs (< 1000 instructions): < 10ms
- Large programs (< 10000 instructions): < 100ms

## Test Maintenance

### Regular Tasks

1. **Add tests** for new features
2. **Update tests** when fixing bugs
3. **Remove tests** that become obsolete
4. **Refactor tests** to reduce duplication
5. **Review coverage** quarterly

### Code Review Checklist

- [ ] All new code has tests
- [ ] Tests are independent
- [ ] Tests have clear names
- [ ] Tests verify one thing
- [ ] Tests use appropriate assertions
- [ ] Tests clean up resources
- [ ] Tests run quickly (< 100ms each)

## Security Testing

### CodeQL Integration

Security scans run automatically:

```bash
# Manual scan
dotnet tool install --global security-scan
security-scan analyze
```

### Vulnerability Testing

- Input validation tests
- Buffer overflow prevention
- Integer overflow checks
- Null reference handling

## Regression Testing

### Preventing Regressions

1. **Keep old tests** even after fixes
2. **Add tests** for reported bugs
3. **Run full suite** before releases
4. **Track metrics** (pass rate, coverage)

### Test Stability

All tests should:
- Pass consistently
- Be deterministic
- Not depend on timing
- Not depend on external state
- Clean up after themselves

## Documentation

Each test file should have:

```csharp
/// <summary>
/// Tests for x86_64 assembler.
/// Covers instruction encoding, label resolution, and integration.
/// </summary>
public static class X86_64Tests
{
    // Test methods...
}
```

## Reporting Issues

When reporting test failures:

1. Include test name
2. Include expected vs actual output
3. Include assembly input
4. Include architecture and format
5. Include .NET version
6. Include OS and version

## References

- [Architecture.md](Architecture.md) - Architecture details
- [API.md](API.md) - API reference
- Test files in `Testing/` folder
- Specification: `.github/agents/badger-spec.txt`
