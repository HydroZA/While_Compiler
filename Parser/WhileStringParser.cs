using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Pidgin;
using static Pidgin.Parser;
using Lexer;

namespace Parser
{
    public class WhileStringParser
    {
        public WhileStringParser()
        {

        }

        // For parsing strings directly without the Lexer
        public Block Parse(string s)
        {
            s = Regex.Replace(s, @" |\r|\t|\n", "");
            try
            {
                //Console.WriteLine(s);
                return BlockParser.ParseOrThrow(s);
            }
            catch (ParseException)
            {
                Console.WriteLine("Unable to Parse");
                throw;
            }
        }

        // For accepting the output of the Lexer
        public Block Parse(List<(TokenType, string)> tokens)
        {
            try
            {
                string inp = string.Join("", tokens.Select(x => x.Item2).ToList());
                Console.WriteLine(inp);
                return BlockParser.ParseOrThrow(inp);
            }
            catch (ParseException)
            {
                Console.WriteLine("Unable to Parse");
                throw;
            }
        }


        // Identifier Parser
        static readonly Pidgin.Parser<char, string> IdentifierParser =
            from x in OneOf(LetterOrDigit, Char('_'), Symbol).ManyString()
            select x;


        // String parser
        static readonly Pidgin.Parser<char, string> StringParser =
            Try (
                from x in String("\"")
                from str in OneOf(LetterOrDigit, Symbol, Whitespace).ManyString()
                from y in String("\"")
                select str
            );

        // (9/3)+4
        // Arithmetic Parser
        static readonly Pidgin.Parser<char, ArithmeticExpression> Fa =
            OneOf(
                // Option 1 -- In brackets
                Try(
                    from x in Char('(')
                    from aexp in ArithmeticParser
                    from y in Char(')')
                    select aexp
                    ),

                // Option 2 -- Number
                Try(Num.Select(x => (ArithmeticExpression)new Number(x))),

                // Option 3 -- Identifier
                Try(IdentifierParser.Select(x => (ArithmeticExpression)new Var(x)))
            );
        
        static readonly Pidgin.Parser<char, ArithmeticExpression> Te =
            OneOf(
                // Option 1 -- Times
                Try(
                    from x in Fa
                    from op in Char('*')
                    from y in Te
                    select (ArithmeticExpression)new ArithmeticOperation(ArithOperationType.TIMES, x, y)
                    ),

                // Option 2 -- Divide
                Try(
                    from x in Fa
                    from op in Char('/')
                    from y in Te
                    select (ArithmeticExpression)new ArithmeticOperation(ArithOperationType.DIVIDE, x, y)
                ),
                
                // Option 3 -- Modulus
                Try (
                    from x in Fa
                    from op in Char('%')
                    from y in Te
                    select (ArithmeticExpression) new ArithmeticOperation(ArithOperationType.MODULO, x, y)
                    ),

                // Option 4 -- Fa
                Fa
            );

        static readonly Pidgin.Parser<char, ArithmeticExpression> ArithmeticParser = 
            OneOf(
                // Option 1 -- Plus
                Try (
                    from x in Te
                    from op in Char('+')
                    from y in ArithmeticParser
                    select (ArithmeticExpression) new ArithmeticOperation(ArithOperationType.PLUS, x, y)
                ),
            
                // Option 2 -- Minus
                Try (
                    from x in Te
                    from op in Char('-')
                    from y in ArithmeticParser
                    select (ArithmeticExpression) new ArithmeticOperation(ArithOperationType.MINUS, x, y)
                    ),

                // Option 3 -- Te
                Te
            );


        // (x==2)
        // boolean expression parser
        static Pidgin.Parser<char, BooleanExpression> BooleanParser =
            OneOf(
                // Option 1 -- Equals
                Try (
                    from x in ArithmeticParser
                    from op in String("==")
                    from y in ArithmeticParser
                    select (BooleanExpression)new BooleanOperation(BoolOperationType.EQUAL, x, y)
                    ),

                // Option 2 -- Not Equal
                Try (
                    from x in ArithmeticParser
                    from op in String("!=")
                    from y in ArithmeticParser
                    select (BooleanExpression)new BooleanOperation(BoolOperationType.NOT_EQUAL, x, y)
                    ),

                // Option 3 -- Less Than
                Try (
                    from x in ArithmeticParser
                    from op in Char('<')
                    from y in ArithmeticParser
                    select (BooleanExpression)new BooleanOperation(BoolOperationType.LESS_THAN, x, y)
                    ),

                // Option 4 -- Greater Than
                Try (
                    from x in ArithmeticParser
                    from op in Char('>')
                    from y in ArithmeticParser
                    select (BooleanExpression)new BooleanOperation(BoolOperationType.GREATER_THAN, x, y)
                    ),

                // Option 5 -- Less Than or Equal
                Try (
                    from x in ArithmeticParser
                    from op in String("<=")
                    from y in ArithmeticParser
                    select (BooleanExpression)new BooleanOperation(BoolOperationType.LESS_THAN_OR_EQUAL, x, y)
                    ),

                // Option 6 -- Greater Than or Equal
                Try (
                    from x in ArithmeticParser
                    from op in String(">=")
                    from y in ArithmeticParser
                    select (BooleanExpression)new BooleanOperation(BoolOperationType.GREATER_THAN_OR_EQUAL, x, y)
                    ),

                // Option 7 -- And
                Try(
                    from a in Char('(')
                    from bexp1 in BooleanParser
                    from b in Char(')')
                    from c in String("&&")
                    from bexp2 in BooleanParser
                    select (BooleanExpression)new And(bexp1, bexp2)
                    ),

                // Option 8 -- Or
                Try(
                    from a in Char('(')
                    from bexp1 in BooleanParser
                    from b in Char(')')
                    from c in String("||")
                    from bexp2 in BooleanParser
                    select (BooleanExpression)new Or(bexp1, bexp2)
                    ),

                // Option 9 -- True
                Try (String("true")).Select(_ => (BooleanExpression) new True()),

                // Option 10 -- False
                Try (String("false")).Select(_ => (BooleanExpression)new False()),

                // Option 11 -- Brackets
                Try (
                    from a in Char('(')
                    from bexp in BooleanParser
                    from b in Char(')')
                    select bexp
                    )
            );



        // single statement parser
        static readonly Pidgin.Parser<char, Statement> StatementParser =
            OneOf(
                // Option 1 -- Skip
                Try(String("skip")).Select(_ => (Statement)new Skip()),

                // Option 2 -- Assign
                Try(
                    from a in IdentifierParser
                    from op in String(":=")
                    from b in ArithmeticParser
                    select (Statement)new Assign(a, b)
                    ),

                // Option 3 -- Write String
                Try(
                    from k in String("write")
                    from str in StringParser
                    select (Statement)new Write(str)
                    ),

                // Option 4 -- Write Var
                Try (
                    from k in String("write")
                    from id in IdentifierParser
                    select (Statement) new Write(id)
                    ),

                // Option 4 -- Read
                Try (
                    from k in String("read")
                    from id in IdentifierParser
                    select (Statement) new Read(id)
                    ),

                // Option 5 -- If
                Try (
                    from k in String("if")
                    from bexp1 in BooleanParser
                    from k2 in String("then")
                    from b in BlockParser
                    from k3 in String("else")
                    from b2 in BlockParser
                    select (Statement) new If(bexp1, b, b2)
                    ),

                // Option 6 -- While
                Try (
                    from k in String("while")
                    from bexp in BooleanParser
                    from k2 in String("do")
                    from bl in BlockParser
                    select (Statement) new While(bexp, bl)
                    )     
                );

        // compound statement parser (seperated with semi-colons)
        static readonly Pidgin.Parser<char, Block> StatementsParser =
            Try(
                from s in StatementParser
                from c in Char(';')
                from ss in StatementsParser
                select new Block(new List<Statement>() { s }.Concat(ss.statements).ToList())
                )

            // Option 2 -- single statement
            .Or(
                Try(StatementParser).Select(x => new Block(new List<Statement>() { x }))
            );


        // block parser (enclosed in curly parenthesis)
        static readonly Pidgin.Parser<char, Block> BlockParser =
            Try(
                from b in String("{")
                from s in StatementsParser
                from c in String("}")
                select s
                )
            .Or(
                Try(StatementsParser)
                );
    }
}
