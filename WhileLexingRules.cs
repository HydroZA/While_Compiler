using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace lex
{
    class WhileLexingRules
    {
        // Lexing Rules for WHILE Language
        private static Rexp DIGIT = new RANGE("0123456789".ToHashSet());

        private static Rexp KEYWORD =
            new ALT("skip",
            new ALT("while",
            new ALT("do",
            new ALT("if",
            new ALT("then",
            new ALT("else",
            new ALT("read",
            new ALT("write",
            new ALT("for",
            new ALT("to",
            new ALT("true",
            "false"
        )))))))))));

        private static Rexp OPERATOR =
            new ALT(":=",
            new ALT("=",
            new ALT("-",
            new ALT("+",
            new ALT("*",
            new ALT("!=",
            new ALT("<",
            new ALT(">",
            new ALT("<=",
            new ALT(">=",
            new ALT("||",
            new ALT("&&",
            new ALT("%",
            new ALT("/",
            "=="
        ))))))))))))));

        private static Rexp LETTER = new RANGE("ABCDEFGHIJKLMNOPQRSTUVXYZabcdefghijklmnopqrstuvwxyz".ToHashSet());

        private static Rexp SYMBOL = new ALT(LETTER, new RANGE("._><=;,:\\".ToHashSet()));

        private static Rexp LPAREN = new ALT("{", "(");

        private static Rexp RPAREN = new ALT("}", ")");

        private static Rexp SEMICOLON = ";";

        private static Rexp WHITESPACE = new ALT(new PLUS(" "), new ALT("\n", "\t"));

        private static Rexp IDENTIFIER = new SEQ(LETTER, new STAR(new ALT("-", new ALT(LETTER, DIGIT))));

        private static Rexp NUMBER = new ALT("0", new SEQ(new RANGE("123456789".ToHashSet()), new STAR(DIGIT)));

        private static Rexp COMMENT = new SEQ("//", new STAR(new ALT(DIGIT, new ALT(" ", SYMBOL))));

        private static Rexp STRING = new SEQ("\"", new SEQ(new STAR(new ALT(SYMBOL, new ALT(WHITESPACE, DIGIT))), "\""));

        public Rexp WHILE_REGS =
            new STAR(
                new ALT(
                    new RECD("KEYWORD", KEYWORD),
                    new ALT(
                        new RECD("IDENTIFIER", IDENTIFIER),
                        new ALT(
                            new RECD("OPERATOR", OPERATOR),
                            new ALT(
                                new RECD("NUMBER", NUMBER),
                                new ALT(
                                    new RECD("SEMI-COLON", SEMICOLON),
                                    new ALT(
                                        new RECD("STRING", STRING),
                                        new ALT(
                                            new RECD("LPAR", LPAREN),
                                            new ALT(
                                                new RECD("RPAR", RPAREN),
                                                new ALT(
                                                    new RECD("COMMENT", COMMENT),
                                                    new RECD("WHITESPACE", WHITESPACE)
                                                )
                                            )
                                        )
                                    )
                                )
                            )
                        )
                    )
                )
            );
    }
}
