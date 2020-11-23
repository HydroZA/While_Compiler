using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Pidgin;
using static Pidgin.Parser;
using lex;
using static Pidgin.ParserExtensions;
namespace Parser
{
    public class WhileTokenParser
    {
        Stack<(TokenType, string)> tokens;

        public WhileTokenParser()
        {
            
        }

        // temp booleanExpression to test
        public ArithmeticExpression Parse(List<(TokenType, string)> t)
        {
            t.Reverse();
            this.tokens = new Stack<(TokenType, string)>(t);
            return ArithmeticParser();

        }

        /*
         * Pattern:
         * 1. Pop
         * 2. Check
         * 3. Match? return else Push
         */
        private ArithOperationType DefineArithmeticOperator((TokenType, string) token)
        {
            if (token.Item1 != TokenType.ARITH_OP)
            {
                throw new Exception("Call to define Arithmetic Operator with invalid TokenType");
            }
            return token.Item2 switch
            {
                "*" => ArithOperationType.TIMES,
                "/" => ArithOperationType.DIVIDE,
                "%" => ArithOperationType.MODULO,
                "+" => ArithOperationType.PLUS,
                "-" => ArithOperationType.MINUS,
                _ => throw new NoMatchException("Token was not a valid arithmetic operator"),
            };
        }

        private BoolOperationType DefineBooleanOperator((TokenType, string) token)
        {
            if (token.Item1 != TokenType.OPERATOR)
            {
                throw new Exception ("Call to define Boolean Operator with invalid TokenType");
            }
            return token.Item2 switch
            {
                "==" => BoolOperationType.EQUAL,
                "!=" => BoolOperationType.NOT_EQUAL,
                "<" => BoolOperationType.LESS_THAN,
                ">" => BoolOperationType.GREATER_THAN,
                "<=" => BoolOperationType.LESS_THAN_OR_EQUAL,
                ">=" => BoolOperationType.GREATER_THAN_OR_EQUAL,
                _ => throw new NoMatchException("Token was not a valid Boolean operator"),
            };
        }

        private ArithmeticExpression Fa()
        {
            
            var firstToken = tokens.Pop();

            switch (firstToken.Item1)
            {
                // Option 1 -- In Brackets
                case TokenType.LPAREN:
                {
                        //store the next token in case we need to backtrack later
                    var temp = tokens.Peek();
                    ArithmeticExpression aexp = ArithmeticParser();

                    var tok2 = tokens.Pop();
                    if (tok2.Item1 == TokenType.RPAREN)
                        return aexp;
                    else
                    {
                        // not a valid brackets arithmetic expression, push the token back on the stack
                        tokens.Push(tok2);
                        tokens.Push(temp);
                        break;
                    }
                }

                // Option 2 -- Number
                case TokenType.NUMBER:
                    return new Number(Int32.Parse(firstToken.Item2));

                // Option 3 -- Identifier
                case TokenType.IDENTIFIER:
                    return new Var(firstToken.Item2);
            }
            
            // If we get here it means we found no matches, push the token back on the stack
            tokens.Push(firstToken);
            throw new NoMatchException();
        }

        // x*2/4+1
        public ArithmeticExpression ArithmeticParser()
        {
            ArithmeticExpression x = Fa();

            // Are there any more tokens to parse?
            if (tokens.Count() > 0)
            {
                var token = tokens.Peek();

                try
                {
                    // Check if the next tokens represent an arithmetic operation

                    if (token.Item1 == TokenType.ARITH_OP)
                    {
                        var op = DefineArithmeticOperator(token);
                        
                        // need to call pop here because we have confirmed that it is a arithmetic operation
                        tokens.Pop();
                        
                        ArithmeticExpression y = ArithmeticParser();
                        return new ArithmeticOperation(op, x, y);
                    }
                    else
                    {
                        return x;
                    }
                }
                catch (NoMatchException)
                {
                    // The next token is not an operator, meaning the arithmetic expression
                    // is just something that was matched in Fa()
                    // Push the token back on the stack and return x
                    return x;
                }
            }
            else
            {
                return x;
            }

        }

        public BooleanExpression BooleanParser()
        {
            // Check if the next sequence of tokens is an Arithmetic Expression
            try
            {
                var temp = tokens.Peek();
                var x = ArithmeticParser();


                var firstToken = tokens.Peek();
                if (firstToken.Item1 == TokenType.OPERATOR)
                {
                    tokens.Pop();
                    // check what the boolean operation is
                    var op = DefineBooleanOperator(firstToken);

                    var y = ArithmeticParser();
                    return new BooleanOperation(op, x, y);
                }
                else
                {
                    // push everything back on the stack and throw nomatcherror
                    tokens.Push(temp);

                    throw new NoMatchException($"Encountered unexpected {firstToken.Item1}\nExpected {TokenType.OPERATOR}");
                }
            }

            catch (NoMatchException)
            {
                // It could still be a boolean expression in brackets
                // or an && or || expression
                // or simply a True or False

                
                var tok2 = tokens.Pop();

                // Option 2 -- And, Or
                if (tok2.Item1 == TokenType.LPAREN)
                {
                    var tok3 = tokens.Peek();
                    BooleanExpression bexp1 = BooleanParser();

                    var tok4 = tokens.Pop();
                    
                    if (tok4.Item1 == TokenType.RPAREN)
                    {
                        // Any more tokens?
                        if (tokens.Count() > 0)
                        {
                            var tok5 = tokens.Pop();
                            
                            if (tok5.Item2 == "&&")
                            {
                                var bexp2 = BooleanParser();
                                return new And(bexp1, bexp2);
                            }
                            else if (tok5.Item2 == "||")
                            {
                                var bexp2 = BooleanParser();
                                return new Or(bexp1, bexp2);
                            }
                            else
                            {
                                tokens.Push(tok5);
                                return bexp1;
                            }
                        }
                        else
                        {
                            return bexp1;
                        }

                    }
                    else
                    {
                        tokens.Push(tok4);
                        tokens.Push(tok3);
                        tokens.Push(tok2);
                        throw new NoMatchException();
                    }
                }
                else if(tok2.Item1 == TokenType.KEYWORD)
                {
                    if (tok2.Item2 == "true")
                    {
                        return new True();
                    }
                    else if (tok2.Item2 == "false")
                    {
                        return new False();
                    }
                    else
                    {
                        tokens.Push(tok2);
                        throw new NoMatchException();
                    }
                }
                // if no match then throw error and push everything back on stack
                else
                {
                    tokens.Push(tok2);
                    throw new NoMatchException();
                }
            }
        }

        private string Keyword (string wantedKeyword)
        {
            var token = tokens.Pop();
            if (token.Item1 == TokenType.KEYWORD)
            {
                if (token.Item2 == wantedKeyword)
                {
                    return token.Item2;
                }
            }

            // not a match -- Put it back on the stack
            tokens.Push(token);
            throw new Exception("Unexpected Token");
        }
    }
}
