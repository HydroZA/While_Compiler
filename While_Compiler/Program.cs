using System;
using Parser;
using CodeGen;
using System.IO;
using System.Text.RegularExpressions;
using System.Diagnostics;

namespace WhileCompiler
{
    class Program
    {
        private static readonly string helpMsg =
@"usage: WhileCompiler.exe [args] <file>

Without arguments, WhileCompiler will compile the specified program and output Jasmin code to a file of the same name.

Optional Arguments:
    -i, -interpret      Parses the input file and runs an interpreter, does not output Jasmin Code
    -o <path>           Specifies the path to output the Jasmin Code";
        
        private static readonly Lexer.Lexer lexer = new Lexer.Lexer(Lexer.WhileLexingRules.rules);
        private static readonly WhileTokenParser parser = new WhileTokenParser();
        private static readonly Interpreter.Interpreter interpreter = new Interpreter.Interpreter();
        private static readonly CodeGenerator codegen = new CodeGenerator();

        static void Main(string[] args)
        {
            bool interpret = false;
            string input = "", outpath = "", filename = "";

            if (args.Length == 0)
            {
                Console.Error.WriteLine(helpMsg);
                Environment.Exit(1);
            }

            for (int i = 0; i < args.Length; i++)
            {
                string arg = args[i];
                if (arg == "-i" || arg == "-interpret")
                {
                    interpret = true;
                }
                else if (arg == "-o")
                {
                    outpath = args[++i];

                    // -i and -o cant be used at the same time
                    interpret = !interpret && interpret;
                }
                else
                {
                    try
                    {
                        input = File.ReadAllText(arg);

                        // This regular expression extracts the filename and discards the rest of the path and the file extension
                        filename = Regex.Match(arg, @"^.*[\\|\/](.+?)\.[^\.]+$").Groups[1].ToString();
                    }
                    catch (Exception e) when (e is FileNotFoundException || e is DirectoryNotFoundException)
                    {
                        Console.WriteLine("Input file not found at: " + arg);
                        Environment.Exit(1);
                    }
                }
            }

            // if no output path given, simply output the file to the working dir
            if (outpath == "")
                outpath = ".";

            Console.WriteLine("Lexing...");
            var lexout = lexer.Lex(input);

            Console.WriteLine("Parsing...");
            var parseOut = parser.Parse(lexout);

            Stopwatch sw = new Stopwatch();
            if (interpret)
            {
                Console.WriteLine("Interpreting...\n");
                
                sw.Start();
                interpreter.Interpret(parseOut);
                sw.Stop();

                Console.WriteLine($"Interpret Time: {sw.ElapsedMilliseconds}ms");
            }
            else
            {
                Console.WriteLine("Compiling...");

                sw.Start();
                var jasmin = codegen.Compile(parseOut, filename);
                sw.Stop();

                Console.WriteLine($"Compile Time: {sw.ElapsedMilliseconds}ms");

                string outFile = outpath + "\\" + filename + ".j";
                File.WriteAllText(outFile, jasmin);
                Console.WriteLine($"{outFile} created");
            }
            Console.WriteLine("Done!");
        }
    }
}
