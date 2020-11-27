using System;
using System.Collections.Generic;
using System.Diagnostics;
using Lexer;
using Parser;

namespace Interpreter
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length == 0)
            {
                Console.WriteLine("Enter a program to run");
                Environment.Exit(1);
            }

            string input = "";
            try
            {
                input = System.IO.File.ReadAllText(args[0]);
            }
            catch (Exception e) when (e is System.IO.FileNotFoundException || e is System.IO.DirectoryNotFoundException)
            {
                Console.WriteLine("File not found");
                Environment.Exit(1);
            }

            if (!args[0].EndsWith(".while"))
            {
                Console.WriteLine("Input file must end with .while");
                Environment.Exit(1);
            }



            try
            {
                Console.WriteLine("Lexing...");
                var ast = new Lexer.Lexer(WhileLexingRules.rules).Lex(input);

                Console.WriteLine("Parsing...");
                Block code = new WhileTokenParser().Parse(ast);

                Console.WriteLine("Interpreting...\n");
                Stopwatch sw = new Stopwatch();
                Interpreter inter = new Interpreter();

                sw.Start();
                Dictionary<string, int> mem = inter.Interpret(code);
                sw.Stop();

                Console.WriteLine($"\nInterpret time: {sw.ElapsedMilliseconds}ms");
            }
            catch (Exception)
            {
                Console.WriteLine("Failed to parse");
                throw;
            }
        }
    }
}
