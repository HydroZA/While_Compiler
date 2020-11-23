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
            switch (args[0])
            {
                case "fibs":
                    {
                        input = @"
                            write ""Fibs"";
                            write ""Enter a value for n:"";
                            read n;
                            minus1:= 0;
                            minus2:= 1;
                            while n > 0 do
                                {
                                temp:= minus2;
                                minus2:= minus1 + minus2;
                                minus1:= temp;
                                n:= n - 1
                            };
                            write ""Result"";
                            write minus2";
                        break;
                    }
                case "loops":
                    {
                        input = @"
                            write ""Loops"";
                            write ""Enter a starting value"";
                            read start;
                            x := start;
                            y := start;
                            z := start;
                            while 0 < x do {
                            while 0 < y do {
                            while 0 < z do { z := z - 1 };
                            z := start;
                            y := y - 1
                            };
                            y := start;
                            x := x - 1
                            }";
                        break;
                    }
                case "maths":
                    {
                        input = @"
                            write ""Maths"";
                            x := ((9*2) + 3) / 7;
                            write x
                            ";
                        break;
                    }
                default:
                    {
                        Console.WriteLine("Select a valid program");
                        Environment.Exit(1);
                        break;
                    }

            }

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
            Console.WriteLine($"Interpret time: {sw.ElapsedMilliseconds}ms");

        }
    }
}
