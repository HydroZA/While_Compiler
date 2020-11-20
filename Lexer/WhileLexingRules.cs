using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace lex
{
    public static class WhileLexingRules
    {
        // Lexing Rules for WHILE Language
        private static readonly Rexp DIGIT = new RANGE("0123456789".ToHashSet());

        private static readonly Rexp KEYWORD = new ALT(new ALT(new ALT(new ALT(new ALT(new ALT(new ALT(new ALT(new ALT(new ALT(new ALT("skip", "while"), "do"), "if"), "then"), "else"), "read"), "write"), "for"), "to"), "true"), "false");

        private static readonly Rexp OPERATOR = new ALT(new ALT(new ALT(new ALT(new ALT(new ALT(new ALT(new ALT(new ALT(new ALT(new ALT(new ALT(new ALT(new ALT(":=", "="), "-"), "+"), "*"), "!="), "<"), ">"), "<="), ">="), "||"), "&&"), "%"), "!"), "==");

        private static readonly Rexp LETTER = new RANGE("ABCDEFGHIJKLMNOPQRSTUVXYZabcdefghijklmnopqrstuvwxyz".ToHashSet());

        private static readonly Rexp SYMBOL = new ALT(LETTER, new RANGE("._><=;,:\\".ToHashSet()));

        private static readonly Rexp LPAREN = new ALT("{", "(");

        private static readonly Rexp RPAREN = new ALT("}", ")");

        private static readonly Rexp SEMICOLON = ";";

        private static readonly Rexp WHITESPACE = new ALT(new ALT(new PLUS(new CHAR(' ')), Environment.NewLine), new CHAR('\t'));

        private static readonly Rexp IDENTIFIER = new SEQ(LETTER, new STAR(new ALT(new ALT(new CHAR('_'), LETTER),  DIGIT)));

        private static readonly Rexp NUMBER = new ALT(new CHAR('0'), new SEQ(new RANGE("123456789".ToHashSet()), new STAR(DIGIT)));

        private static readonly Rexp COMMENT = new SEQ("//", new STAR(new ALT(new ALT(new ALT(DIGIT, " "), SYMBOL), LETTER)));

        private static readonly Rexp STRING = new SEQ(new SEQ(new CHAR('\"'), new STAR(new ALT(new ALT(SYMBOL, WHITESPACE), DIGIT))), "\"");

        public static readonly Rexp rules =
            new STAR(
                new ALT(
                    new ALT(
                        new ALT(
                            new ALT(
                                new ALT(
                                    new ALT(
                                        new ALT(
                                            new ALT(
                                                new ALT(
                                                    new RECD("KEYWORD", KEYWORD),
                                                new RECD("IDENTIFIER", IDENTIFIER)),
                                            new RECD("OPERATOR", OPERATOR)),
                                        new RECD("NUMBER", NUMBER)),
                                    new RECD("SEMI-COLON", SEMICOLON)),
                                new RECD("STRING", STRING)),
                            new RECD("LPAR", LPAREN)),
                        new RECD("RPAR", RPAREN)),
                    new RECD("COMMENT", COMMENT)),
                new RECD("WHITESPACE", WHITESPACE))
            );

    }
}
