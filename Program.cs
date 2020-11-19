using ConsoleTables;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;

namespace lex
{
    class Program
    {
        public static void Main ()
        {
            Lexer lexer = new Lexer();

            Rexp rules = WhileLexingRules.rules;

            string prog = @"start := 1000;
x := start;
y := start;
z := start;
while 0 < x do {
 while 0 < y do {
  while 0 < z do {
    z := z - 1
  };
  z := start;
  y := y - 1
 };     
 y := start;
 x := x - 1
}
";
            foreach ((string, string) x  in lexer.RemoveWhitespace(lexer.Lex(rules, prog)))
            {
                Console.WriteLine($"(\"{x.Item1}\", \"{x.Item2}\"),");
            }
        }
    }
}
