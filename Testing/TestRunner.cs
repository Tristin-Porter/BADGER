using System;
using System.Collections.Generic;

namespace Badger.Testing;

/// <summary>
/// Main test runner for BADGER assembler test suite.
/// Orchestrates all test categories and provides assertion helpers.
/// </summary>
public static class TestRunner
{
    private static int passedTests = 0;
    private static int failedTests = 0;
    private static List<string> failures = new List<string>();
    
    public static void RunAllTests()
    {
        Console.WriteLine("=".PadRight(70, '='));
        Console.WriteLine("BADGER Test Suite");
        Console.WriteLine("=".PadRight(70, '='));
        Console.WriteLine();
        
        passedTests = 0;
        failedTests = 0;
        failures.Clear();
        
        // Run all test suites
        WATTests.RunTests();
        X86_64Tests.RunTests();
        X86_32Tests.RunTests();
        X86_16Tests.RunTests();
        ARM64Tests.RunTests();
        ARM32Tests.RunTests();
        ContainerTests.RunTests();
        IntegrationTests.RunTests();
        
        // Print summary
        Console.WriteLine();
        Console.WriteLine("=".PadRight(70, '='));
        Console.WriteLine($"Test Results: {passedTests} passed, {failedTests} failed");
        Console.WriteLine("=".PadRight(70, '='));
        
        if (failedTests > 0)
        {
            Console.WriteLine();
            Console.WriteLine("Failed tests:");
            foreach (var failure in failures)
            {
                Console.WriteLine($"  - {failure}");
            }
        }
    }
    
    public static void Assert(bool condition, string testName, string message = "")
    {
        if (condition)
        {
            passedTests++;
            Console.WriteLine($"✓ {testName}");
        }
        else
        {
            failedTests++;
            failures.Add(string.IsNullOrEmpty(message) ? testName : $"{testName}: {message}");
            Console.WriteLine($"✗ {testName}");
            if (!string.IsNullOrEmpty(message))
            {
                Console.WriteLine($"  {message}");
            }
        }
    }
    
    public static void AssertArrayEqual(byte[] expected, byte[] actual, string testName)
    {
        if (expected.Length != actual.Length)
        {
            Assert(false, testName, $"Length mismatch: expected {expected.Length}, got {actual.Length}");
            return;
        }
        
        for (int i = 0; i < expected.Length; i++)
        {
            if (expected[i] != actual[i])
            {
                Assert(false, testName, $"Byte mismatch at index {i}: expected 0x{expected[i]:X2}, got 0x{actual[i]:X2}");
                return;
            }
        }
        
        Assert(true, testName);
    }
}
