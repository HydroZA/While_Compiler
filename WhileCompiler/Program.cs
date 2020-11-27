using System;
using Lexer;
using Parser;
using CodeGen;
using Interpreter;
using System.IO;
using System.Text.RegularExpressions;

namespace WhileCompiler
{
    class Program
    {
        private static readonly string helpMsg =
            @"usage: WhileCompiler.exe [-interpret] [-o <outputpath>] <file>";

        private static readonly Lexer.Lexer lexer = new Lexer.Lexer(Lexer.WhileLexingRules.rules);
        private static readonly WhileTokenParser parser = new WhileTokenParser();
        private static readonly Interpreter.Interpreter interpreter = new Interpreter.Interpreter();
        private static readonly CodeGenerator codegen = new CodeGenerator();

        static void Main(string[] args)
        {
            bool interpret = false;
            string outpath = null;
            string input = "";
            string filename = "";

            if (args.Length == 0)
            {
                Console.WriteLine(helpMsg);
                Environment.Exit(1);
            }

            for (int i = 0; i < args.Length; i++)
            {
                string arg = args[i];
                if (arg == "-i" || arg == "-interpret")
                {
                    interpret = true;
                }
                else if (arg == "-i")
                {
                    outpath = args[++i];
                }
                else
                {
                    try
                    {
                        input = File.ReadAllText(arg);

                        // This regular expression extracts the filename and discards the rest of the path and the file extension
                        filename = Regex.Match(arg, @"^.*[\\|\/](.+?)\.[^\.]+$").ToString();
                    }
                    catch (Exception e) when (e is FileNotFoundException || e is DirectoryNotFoundException)
                    {
                        Console.WriteLine("Input file not found at: " + args[i]);
                        Environment.Exit(1);
                    }
                }
            }

            Console.WriteLine("Lexing...");
            var lexout = lexer.Lex(input);

            Console.WriteLine("Parsing...");
            var parseOut = parser.Parse(lexout);

            if (interpret)
            {
                Console.WriteLine("Interpreting...\n");
                interpreter.Interpret(parseOut);
            }
            else
            {
                Console.WriteLine("Compiling...");
                var jasmin = codegen.Compile(parseOut, filename);
                File.WriteAllText(outpath, jasmin);
                Console.WriteLine("Done!");
            }
        }
    }
}
