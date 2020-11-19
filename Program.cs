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
            /*  Rexp LPAR = new ALT(new CHAR('('), new CHAR('{'));
              Rexp RPAR = new ALT(new CHAR(')'), new CHAR('}'));
              Rexp rules = new STAR(new ALT(new RECD("LPAR", LPAR), new RECD("RPAR", RPAR)));
            */
            //Rexp ch = new CHAR('(');
            //Rexp rules = new STAR(new RECD("LPAR", ch));
            Rexp rules = WhileLexingRules.rules;

            string prog = "memes";
            foreach ((string, string) x  in lexer.Lex(rules, prog))
            {
                Console.WriteLine($"{x.Item1}\t{x.Item2}");
            }
        }
    }
}
