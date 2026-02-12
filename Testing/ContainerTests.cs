using System;

namespace Badger.Testing;

/// <summary>
/// Tests for container emission (Native and PE formats)
/// Verifies correct binary structure for each container type
/// </summary>
public static class ContainerTests
{
    public static void RunTests()
    {
        TestRunner.RunTest("Native: Emit flat binary", TestNativeEmit);
        TestRunner.RunTest("Native: Preserve machine code", TestNativePreserveCode);
        
        TestRunner.RunTest("PE: Emit valid PE structure", TestPEStructure);
        TestRunner.RunTest("PE: DOS header present", TestPEDOSHeader);
        TestRunner.RunTest("PE: PE signature present", TestPESignature);
        TestRunner.RunTest("PE: Code section present", TestPECodeSection);
    }

    // Native Container Tests
    private static void TestNativeEmit()
    {
        byte[] machineCode = new byte[] { 0x55, 0x48, 0x89, 0xE5, 0x5D, 0xC3 };
        byte[] binary = Badger.Containers.Native.Emit(machineCode);
        
        TestRunner.Assert(binary != null, "Should produce binary output");
        TestRunner.Assert(binary.Length > 0, "Binary should not be empty");
    }

    private static void TestNativePreserveCode()
    {
        byte[] machineCode = new byte[] { 0x55, 0x48, 0x89, 0xE5, 0x5D, 0xC3 };
        byte[] binary = Badger.Containers.Native.Emit(machineCode);
        
        // Native format should be exactly the machine code (no headers)
        TestRunner.AssertArrayEqual(machineCode, binary, "Native binary should be raw machine code");
    }

    // PE Container Tests
    private static void TestPEStructure()
    {
        byte[] machineCode = new byte[] { 0x55, 0x48, 0x89, 0xE5, 0x5D, 0xC3 };
        byte[] binary = Badger.Containers.PE.Emit(machineCode);
        
        TestRunner.Assert(binary != null, "Should produce PE binary");
        TestRunner.Assert(binary.Length > machineCode.Length, "PE binary should be larger than machine code");
        TestRunner.Assert(binary.Length >= 512, "PE binary should be at least one sector");
    }

    private static void TestPEDOSHeader()
    {
        byte[] machineCode = new byte[] { 0x55, 0x48, 0x89, 0xE5, 0x5D, 0xC3 };
        byte[] binary = Badger.Containers.PE.Emit(machineCode);
        
        // Check DOS header magic number "MZ"
        TestRunner.AssertEqual(0x4D, binary[0], "DOS header should start with 'M'");
        TestRunner.AssertEqual(0x5A, binary[1], "DOS header should have 'Z' as second byte");
    }

    private static void TestPESignature()
    {
        byte[] machineCode = new byte[] { 0x55, 0x48, 0x89, 0xE5, 0x5D, 0xC3 };
        byte[] binary = Badger.Containers.PE.Emit(machineCode);
        
        // PE signature offset is at 0x3C in DOS header
        int peOffset = binary[0x3C] | (binary[0x3D] << 8) | (binary[0x3E] << 16) | (binary[0x3F] << 24);
        
        // Check PE signature "PE\0\0"
        TestRunner.AssertEqual(0x50, binary[peOffset], "PE signature should start with 'P'");
        TestRunner.AssertEqual(0x45, binary[peOffset + 1], "PE signature should have 'E' as second byte");
        TestRunner.AssertEqual(0x00, binary[peOffset + 2], "PE signature should have null byte");
        TestRunner.AssertEqual(0x00, binary[peOffset + 3], "PE signature should have null byte");
    }

    private static void TestPECodeSection()
    {
        byte[] machineCode = new byte[] { 0x55, 0x48, 0x89, 0xE5, 0x5D, 0xC3 };
        byte[] binary = Badger.Containers.PE.Emit(machineCode);
        
        // Code should be present in the binary after headers
        // PE headers are typically 512 bytes (aligned to file alignment)
        TestRunner.Assert(binary.Length >= 512, "Should have space for headers");
        
        // Verify machine code is present somewhere in the binary
        bool foundCode = false;
        for (int i = 0; i <= binary.Length - machineCode.Length; i++)
        {
            bool match = true;
            for (int j = 0; j < machineCode.Length; j++)
            {
                if (binary[i + j] != machineCode[j])
                {
                    match = false;
                    break;
                }
            }
            if (match)
            {
                foundCode = true;
                break;
            }
        }
        
        TestRunner.Assert(foundCode, "Machine code should be embedded in PE binary");
    }
}
