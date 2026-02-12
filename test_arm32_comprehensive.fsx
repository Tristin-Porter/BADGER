#!/usr/bin/env dotnet fsi

open System
open System.IO

// Load the assembly
#r "bin/Debug/net8.0/Badger.dll"

printfn "=== ARM32 Comprehensive Assembly Test ==="
printfn ""

let assembly = File.ReadAllText("test_arm32_sample.txt")
let machineCode = Badger.Architectures.ARM32.Assembler.Assemble(assembly)

printfn "✓ ARM32 assembly compiled successfully!"
printfn "✓ Generated %d bytes of machine code" machineCode.Length
printfn "✓ Number of instructions: %d" (machineCode.Length / 4)

File.WriteAllBytes("test_arm32_output.bin", machineCode)
printfn "✓ Binary written to test_arm32_output.bin"

printfn ""
printfn "First 64 bytes (hex):"
for i in 0..4..Math.Min(63, machineCode.Length - 1) do
    printf "  %04X: " i
    for j in 0..3 do
        if i + j < machineCode.Length then
            printf "%02X " machineCode.[i + j]
    printfn ""

printfn ""
printfn "=== ARM32 Comprehensive Test Complete ==="
