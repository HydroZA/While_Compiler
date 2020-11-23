using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using lex;

namespace Parser
{
    public class WhileTokenParser
    {
        Stack<(TokenType, string)> tokens;

        public WhileTokenParser()
        {
            
        }

        // temp booleanExpression to test
        public Block Parse(List<(TokenType, string)> t)
        {
            t.Reverse();
            this.tokens = new Stack<(TokenType, string)>(t);
            return StatementsParser();

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
                if(tok2.Item1 == TokenType.KEYWORD)
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

        private Statement Skip()
        {
            var t = tokens.Pop();
            if (t.Item2 == "skip")
                return new Skip();
            else
            {
                tokens.Push(t);
                throw new NoMatchException($"Encountered unexpected {t.Item2}\nExpected \"skip\"");
            }
        }

        private Statement Assign()
        {
            var t = tokens.Pop();
            if (t.Item1 == TokenType.IDENTIFIER)
            {
                // Store the variable name
                var iden = t.Item2;

                var t2 = tokens.Pop();
                if (t2.Item2 == ":=")
                {
                    // Get the arithmetic expression to assign to the var
                    ArithmeticExpression aexp = ArithmeticParser();
                    
                    // return
                    return new Assign(iden, aexp);
                }
                else
                {
                    // Push the token back on the stack
                    // so it can be passed on to other parsers
                    tokens.Push(t2);
                }
            }
            tokens.Push(t);
            throw new NoMatchException($"Encountered unexpected {t.Item2}\nExpected Identifier");
        }

        private Statement Write()
        {
            var t = tokens.Pop();
            if (t.Item2 == "write")
            {
                // write can either be performed on a string or on a variable
                var t2 = tokens.Pop();
                if (t2.Item1 == TokenType.STRING || t2.Item1 == TokenType.IDENTIFIER)
                {
                    return new Write(t2.Item2);
                }
                else
                {
                    // Push back on the stack
                    tokens.Push(t2);
                }
            }
            tokens.Push(t);
            throw new NoMatchException($"Encountered unexpected {t.Item2}\nExpected \"write\"");
        }

        private Statement Read()
        {
            var t = tokens.Pop();
            if (t.Item2 == "read")
            {
                var t2 = tokens.Pop();
                if (t2.Item1 == TokenType.IDENTIFIER)
                {
                    return new Read(t2.Item2);
                }
                else
                {
                    tokens.Push(t2);
                }
            }
            tokens.Push(t);
            throw new NoMatchException($"Encountered unexpected {t.Item2}\nExpected \"read\"");
        }

        private Statement If()
        {
            try
            {
                var t = tokens.Pop();
                if (t.Item2 == "if")
                {
                    var bexp = BooleanParser();
                    var t2 = tokens.Pop();
                    if (t2.Item2 == "then")
                    {
                        var b = BlockParser();
                        var t3 = tokens.Pop();
                        if (t3.Item2 == "else")
                        {
                            var b2 = BlockParser();
                            return new If(bexp, b, b2);
                        }
                        else
                        {
                            throw new Exception();
                        }
                    }
                    else
                    {
                        throw new Exception();
                    }
                }
                else
                {
                    /* We push the token back here and not in the above parsers because
                     * if we fail to parse on the first keyword, then we are in the wrong parser
                     * and need to allow for it to be passed on to the next parser.
                     * 
                     * If we fail after confirming we are in an If statement, that must mean
                     *  that the If statement was malformed, which should lead to a parsing failure
                     */
                    tokens.Push(t);
                    throw new NoMatchException($"Encountered unexpected {t.Item2}\nExpected \"if\"");
                }
            }
            catch (Exception)
            {
                throw new Exception("Malformed If Statement. Parse Failed");
            }
        }

        private Statement While()
        {
            try
            {
                var t = tokens.Pop();
                if (t.Item2 == "while")
                {
                    var b = BooleanParser();

                    var t2 = tokens.Pop();
                    if (t2.Item2 == "do")
                    {
                        var bl = BlockParser();
                        return new While(b, bl);
                    }
                    else
                    {
                        throw new Exception();
                    }
                }
                else
                {
                    tokens.Push(t);
                    throw new NoMatchException($"Encountered unexpected {t.Item2}\nExpected \"while\"");
                }
            }
            catch (Exception)
            {
                throw new Exception("Malformed While Statement. Parse Failed");
            }
        }

        private Statement StatementParser()
        {
            Statement stmt;
            try
            {
                stmt = Skip();
            }
            catch (NoMatchException)
            {
                try
                {
                    stmt = Assign();
                }
                catch (NoMatchException)
                {
                    try
                    {
                        stmt = Write();
                    }
                    catch (NoMatchException)
                    {
                        try
                        {
                            stmt = Read();
                        }
                        catch (NoMatchException)
                        {
                            try
                            {
                                stmt = If();
                            }
                            catch(NoMatchException)
                            {
                                try
                                {
                                    stmt = While();
                                }
                                catch (NoMatchException)
                                {
                                    throw new Exception("Failed to match statement. Parse Failed");
                                }
                            }
                        }
                    }
                }
            }
            return stmt;
        }

        private Block StatementsParser()
        {
            var s = StatementParser();
            
            // More tokens?
            if (tokens.Count() > 0)
            {
                var t = tokens.Pop();
                if (t.Item1 == TokenType.SEMICOLON)
                {
                    var ss = StatementsParser();
                    return new Block(s.ToList().Concat(ss.statements).ToList());
                }
                else
                {
                    tokens.Push(t);
                    return new Block(s.ToList());
                }
            }
            return new Block(s.ToList()); ;
        }

        private Block BlockParser()
        {
            return new Block(new List<Statement>() { });
        }
    }
}
