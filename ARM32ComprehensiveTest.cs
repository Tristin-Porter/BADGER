using System;
using System.IO;
using Badger.Architectures.ARM32;

namespace Badger.Tests;

public static class ARM32ComprehensiveTest
{
    public static void Run()
    {
        Console.WriteLine("\n=== ARM32 Comprehensive Assembly Test ===\n");
        
        var assembly = File.ReadAllText("test_arm32_sample.txt");
        var machineCode = Assembler.Assemble(assembly);

        Console.WriteLine($"✓ ARM32 assembly compiled successfully!");
        Console.WriteLine($"✓ Generated {machineCode.Length} bytes of machine code");
        Console.WriteLine($"✓ Number of instructions: {machineCode.Length / 4}");

        // Write to binary file
        File.WriteAllBytes("test_arm32_output.bin", machineCode);
        Console.WriteLine("✓ Binary written to test_arm32_output.bin");

        // Show first 64 bytes in hex
        Console.WriteLine("\nFirst 64 bytes (hex):");
        for (int i = 0; i < Math.Min(64, machineCode.Length); i += 4)
        {
            Console.Write($"  {i:X4}: ");
            for (int j = 0; j < 4 && i + j < machineCode.Length; j++)
            {
                Console.Write($"{machineCode[i + j]:X2} ");
            }
            Console.WriteLine();
        }
        
        Console.WriteLine("\n=== ARM32 Comprehensive Test Complete ===\n");
    }
}
