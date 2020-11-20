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
        private static readonly Rexp rDIGIT = new RANGE("0123456789".ToHashSet());

        private static readonly Rexp rKEYWORD = new ALT(new ALT(new ALT(new ALT(new ALT(new ALT(new ALT(new ALT(new ALT(new ALT(new ALT("skip", "while"), "do"), "if"), "then"), "else"), "read"), "write"), "for"), "to"), "true"), "false");

        private static readonly Rexp rOPERATOR = new ALT(new ALT(new ALT(new ALT(new ALT(new ALT(new ALT(new ALT(new ALT(new ALT(new ALT(new ALT(new ALT(new ALT(":=", "="), "-"), "+"), "*"), "!="), "<"), ">"), "<="), ">="), "||"), "&&"), "%"), "!"), "==");

        private static readonly Rexp rLETTER = new RANGE("ABCDEFGHIJKLMNOPQRSTUVXYZabcdefghijklmnopqrstuvwxyz".ToHashSet());

        private static readonly Rexp rSYMBOL = new ALT(rLETTER, new RANGE("._><=;,:\\".ToHashSet()));

        private static readonly Rexp rLPAREN = new ALT("{", "(");

        private static readonly Rexp rRPAREN = new ALT("}", ")");

        private static readonly Rexp rSEMICOLON = ";";

        private static readonly Rexp rWHITESPACE = new ALT(new ALT(new PLUS(new CHAR(' ')), Environment.NewLine), new CHAR('\t'));

        private static readonly Rexp rIDENTIFIER = new SEQ(rLETTER, new STAR(new ALT(new ALT(new CHAR('_'), rLETTER),  rDIGIT)));

        private static readonly Rexp rNUMBER = new ALT(new CHAR('0'), new SEQ(new RANGE("123456789".ToHashSet()), new STAR(rDIGIT)));

        private static readonly Rexp rCOMMENT = new SEQ("//", new STAR(new ALT(new ALT(new ALT(rDIGIT, " "), rSYMBOL), rLETTER)));

        private static readonly Rexp rSTRING = new SEQ(new SEQ(new CHAR('\"'), new STAR(new ALT(new ALT(rSYMBOL, rWHITESPACE), rDIGIT))), "\"");

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
                                                    new RECD(TokenType.KEYWORD, rKEYWORD),
                                                new RECD(TokenType.IDENTIFIER, rIDENTIFIER)),
                                            new RECD(TokenType.OPERATOR, rOPERATOR)),
                                        new RECD(TokenType.NUMBER, rNUMBER)),
                                    new RECD(TokenType.SEMICOLON, rSEMICOLON)),
                                new RECD(TokenType.STRING, rSTRING)),
                            new RECD(TokenType.LPAREN, rLPAREN)),
                        new RECD(TokenType.RPAREN, rRPAREN)),
                    new RECD(TokenType.COMMENT, rCOMMENT)),
                new RECD(TokenType.WHITESPACE, rWHITESPACE))
            );

    }
}
