using CDTk;
using System;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace Badger;

// WAT Token definitions
public class WATTokens : TokenSet
{
    // Keywords
    public Token Module = @"module";
    public Token Func = @"func";
    public Token Param = @"param";
    public Token Result = @"result";
    public Token Local = @"local";
    
    // Block keywords
    public Token Block = @"block";
    public Token Loop = @"loop";
    public Token If = @"if";
    public Token Then = @"then";
    public Token Else = @"else";
    public Token End = @"end";
    
    // Control flow
    public Token Br = @"br";
    public Token BrIf = @"br_if";
    public Token Return = @"return";
    public Token Call = @"call";
    
    // Variable instructions
    public Token LocalGet = @"local\.get";
    public Token LocalSet = @"local\.set";
    public Token LocalTee = @"local\.tee";
    
    // Memory instructions
    public Token I32Load = @"i32\.load";
    public Token I32Store = @"i32\.store";
    
    // Numeric instructions - i32
    public Token I32Const = @"i32\.const";
    public Token I32Add = @"i32\.add";
    public Token I32Sub = @"i32\.sub";
    public Token I32Mul = @"i32\.mul";
    public Token I32DivS = @"i32\.div_s";
    public Token I32And = @"i32\.and";
    public Token I32Or = @"i32\.or";
    public Token I32Xor = @"i32\.xor";
    public Token I32Eq = @"i32\.eq";
    public Token I32Ne = @"i32\.ne";
    public Token I32LtS = @"i32\.lt_s";
    public Token I32GtS = @"i32\.gt_s";
    
    // Parametric instructions
    public Token Drop = @"drop";
    public Token Nop = @"nop";
    
    // Value types
    public Token I32Type = @"i32";
    public Token I64Type = @"i64";
    
    // Literals
    public Token Identifier = @"\$[a-zA-Z_][a-zA-Z0-9_]*";
    public Token Integer = @"-?[0-9]+";
    public Token String = "\"([^\"\\\\]|\\\\.)*\"";
    
    // Structural
    public Token LeftParen = @"\(";
    public Token RightParen = @"\)";
    
    // Whitespace and comments
    public Token Whitespace = new Token(@"\s+").Ignore();
    public Token LineComment = new Token(@";;[^\n]*").Ignore();
    public Token BlockComment = new Token(@"\(\;.*?\;\)", RegexOptions.Singleline).Ignore();
}

// Simplified WAT Grammar rules for initial implementation
public class WATRules : RuleSet
{
    // Module is the top-level structure
    public Rule Module = new Rule("'(' 'module' fields:Field* ')'")
        .Returns("fields");
    
    // A field can be a function
    public Rule Field = new Rule("func:Function");
    
    // Function definition
    public Rule Function = new Rule("'(' 'func' id:@Identifier? params:FuncParam* results:FuncResult* locals:LocalDef* body:Instruction* ')'")
        .Returns("id", "params", "results", "locals", "body");
    
    public Rule FuncParam = new Rule("'(' 'param' id:@Identifier? valtype:ValueType ')'")
        .Returns("id", "valtype");
    
    public Rule FuncResult = new Rule("'(' 'result' valtype:ValueType ')'")
        .Returns("valtype");
    
    public Rule LocalDef = new Rule("'(' 'local' id:@Identifier? valtype:ValueType ')'")
        .Returns("id", "valtype");
    
    public Rule ValueType = new Rule("type:@I32Type");
    
    // Instructions (simplified set)
    public Rule Instruction = new Rule("instr:PlainInstr");
    
    public Rule PlainInstr = new Rule("op:@I32Add");
}

public class Program
{
    public static void Main(string[] args)
    {
        if (args.Length == 0)
        {
            Console.WriteLine("BADGER - Better Assembler for Dependable Generation of Efficient Results");
            Console.WriteLine("Usage: badger <input.wat> [options]");
            Console.WriteLine();
            Console.WriteLine("Options:");
            Console.WriteLine("  -o <output>      Output file path");
            Console.WriteLine("  --arch <arch>    Target architecture (x86_64, x86_32, x86_16, arm64, arm32)");
            Console.WriteLine("  --format <fmt>   Output format (native, pe)");
            return;
        }
        
        string inputFile = args[0];
        string outputFile = "output.bin";
        string architecture = "x86_64";
        string format = "native";
        
        // Parse command line arguments
        for (int i = 1; i < args.Length; i++)
        {
            if (args[i] == "-o" && i + 1 < args.Length)
            {
                outputFile = args[++i];
            }
            else if (args[i] == "--arch" && i + 1 < args.Length)
            {
                architecture = args[++i];
            }
            else if (args[i] == "--format" && i + 1 < args.Length)
            {
                format = args[++i];
            }
        }
        
        try
        {
            // Read WAT input
            string watInput = File.ReadAllText(inputFile);
            
            // Build CDTk compiler with WAT grammar
            var tokens = new WATTokens();
            var rules = new WATRules();
            
            // Create architecture-specific mapset
            MapSet targetMapSet = architecture.ToLower() switch
            {
                "x86_64" => new Badger.Architectures.x86_64.WATToX86_64MapSet(),
                "x86_32" => new Badger.Architectures.x86_32.WATToX86_32MapSet(),
                "x86_16" => new Badger.Architectures.x86_16.WATToX86_16MapSet(),
                "arm64" => new Badger.Architectures.ARM64.WATToARM64MapSet(),
                "arm32" => new Badger.Architectures.ARM32.WATToARM32MapSet(),
                _ => throw new ArgumentException($"Unknown architecture: {architecture}")
            };
            
            var compiler = new Compiler()
                .WithTokens(tokens)
                .WithRules(rules)
                .WithTarget(targetMapSet)
                .Build();
            
            // Compile WAT to assembly text
            var result = compiler.Compile(watInput);
            
            // Check for errors
            if (result.Diagnostics != null && result.Diagnostics.HasErrors)
            {
                Console.WriteLine("Compilation errors:");
                foreach (var diagnostic in result.Diagnostics.Items)
                {
                    Console.WriteLine($"  {diagnostic}");
                }
                Environment.Exit(1);
            }
            
            string assemblyText = result.Output ?? "";
            Console.WriteLine("Generated assembly:");
            Console.WriteLine(assemblyText);
            
            // Assemble to machine code
            byte[] machineCode = architecture.ToLower() switch
            {
                "x86_64" => Badger.Architectures.x86_64.Assembler.Assemble(assemblyText),
                "x86_32" => Badger.Architectures.x86_32.Assembler.Assemble(assemblyText),
                "x86_16" => Badger.Architectures.x86_16.Assembler.Assemble(assemblyText),
                "arm64" => Badger.Architectures.ARM64.Assembler.Assemble(assemblyText),
                "arm32" => Badger.Architectures.ARM32.Assembler.Assemble(assemblyText),
                _ => throw new ArgumentException($"Unknown architecture: {architecture}")
            };
            
            // Emit container
            byte[] binary = format.ToLower() switch
            {
                "native" => Badger.Containers.Native.Emit(machineCode),
                "pe" => Badger.Containers.PE.Emit(machineCode),
                _ => throw new ArgumentException($"Unknown format: {format}")
            };
            
            File.WriteAllBytes(outputFile, binary);
            Console.WriteLine($"Successfully wrote {binary.Length} bytes to {outputFile}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
            Console.WriteLine(ex.StackTrace);
            Environment.Exit(1);
        }
    }
}