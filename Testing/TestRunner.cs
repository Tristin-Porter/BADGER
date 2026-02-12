using System;
using System.Collections.Generic;
using System.Linq;

namespace Badger.Testing;

/// <summary>
/// BADGER Test Runner - Executes comprehensive test suite
/// Tests verify WAT parsing, lowering, assembly encoding, and container emission
/// </summary>
public static class TestRunner
{
    private static int totalTests = 0;
    private static int passedTests = 0;
    private static int failedTests = 0;
    private static List<string> failures = new List<string>();

    public static void RunAllTests()
    {
        Console.WriteLine("================================================================================");
        Console.WriteLine("BADGER Test Suite");
        Console.WriteLine("================================================================================");
        Console.WriteLine();

        // WAT Parser Tests
        Console.WriteLine("WAT Parser Tests:");
        Console.WriteLine("----------------");
        WATParserTests.RunTests();
        Console.WriteLine();

        // Architecture Lowering Tests
        Console.WriteLine("Architecture Lowering Tests:");
        Console.WriteLine("---------------------------");
        LoweringTests.RunTests();
        Console.WriteLine();

        // Assembly Encoding Tests
        Console.WriteLine("Assembly Encoding Tests:");
        Console.WriteLine("-----------------------");
        AssemblyEncodingTests.RunTests();
        Console.WriteLine();

        // Container Emission Tests
        Console.WriteLine("Container Emission Tests:");
        Console.WriteLine("------------------------");
        ContainerTests.RunTests();
        Console.WriteLine();

        // Integration Tests
        Console.WriteLine("Integration Tests:");
        Console.WriteLine("-----------------");
        IntegrationTests.RunTests();
        Console.WriteLine();

        // Summary
        Console.WriteLine("================================================================================");
        Console.WriteLine($"Test Results: {passedTests}/{totalTests} passed, {failedTests} failed");
        Console.WriteLine("================================================================================");

        if (failedTests > 0)
        {
            Console.WriteLine();
            Console.WriteLine("Failed Tests:");
            foreach (var failure in failures)
            {
                Console.WriteLine($"  ✗ {failure}");
            }
        }
    }

    public static void RunTest(string name, Action test)
    {
        totalTests++;
        try
        {
            test();
            passedTests++;
            Console.WriteLine($"  ✓ {name}");
        }
        catch (Exception ex)
        {
            failedTests++;
            failures.Add(name);
            Console.WriteLine($"  ✗ {name}: {ex.Message}");
        }
    }

    public static void Assert(bool condition, string message)
    {
        if (!condition)
        {
            throw new Exception(message);
        }
    }

    public static void AssertEqual<T>(T expected, T actual, string message)
    {
        if (!EqualityComparer<T>.Default.Equals(expected, actual))
        {
            throw new Exception($"{message} - Expected: {expected}, Actual: {actual}");
        }
    }

    public static void AssertArrayEqual(byte[] expected, byte[] actual, string message)
    {
        if (expected.Length != actual.Length)
        {
            throw new Exception($"{message} - Length mismatch. Expected: {expected.Length}, Actual: {actual.Length}");
        }

        for (int i = 0; i < expected.Length; i++)
        {
            if (expected[i] != actual[i])
            {
                throw new Exception($"{message} - Byte mismatch at index {i}. Expected: 0x{expected[i]:X2}, Actual: 0x{actual[i]:X2}");
            }
        }
    }
}
