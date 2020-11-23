using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace lex
{
    public enum TokenType
    {
        KEYWORD,
        IDENTIFIER,
        ARITH_OP,
        OPERATOR,
        NUMBER,
        SEMICOLON,
        STRING,
        LPAREN,
        RPAREN,
        COMMENT,
        WHITESPACE
    }
}
