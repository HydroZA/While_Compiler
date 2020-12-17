using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Lexer;

namespace Parser
{
    public class WhileTokenParser
    {
        Stack<(TokenType, string)> tokens;

        public WhileTokenParser()
        {

        }

        public Block Parse(List<(TokenType, string)> t)
        {
            t.Reverse();
            this.tokens = new Stack<(TokenType, string)>(t);
            return BlockParser();

        }

        private ArithOperationType DefineAEArithmeticOperator((TokenType, string) token)
        {
            if (token.Item1 != TokenType.ARITH_OP)
            {
                throw new NoMatchException("Call to define Arithmetic Operator with invalid TokenType");
            }
            return token.Item2 switch
            {
                "+" => ArithOperationType.PLUS,
                "-" => ArithOperationType.MINUS,
                _ => throw new NoMatchException("Token was not a valid arithmetic operator")
            };
        }


        private ArithOperationType DefineTeArithmeticOperator((TokenType, string) token)
        {
            if (token.Item1 != TokenType.ARITH_OP)
            {
                throw new NoMatchException("Call to define Arithmetic Operator with invalid TokenType");
            }
            return token.Item2 switch
            {
                "*" => ArithOperationType.TIMES,
                "/" => ArithOperationType.DIVIDE,
                "%" => ArithOperationType.MODULO,
                _ => throw new NoMatchException("Token was not a valid arithmetic operator")
            };
        }

        private BoolOperationType DefineBooleanOperator((TokenType, string) token)
        {
            if (token.Item1 != TokenType.OPERATOR)
            {
                throw new NoMatchException("Call to define Boolean Operator with invalid TokenType");
            }
            return token.Item2 switch
            {
                "==" => BoolOperationType.EQUAL,
                "!=" => BoolOperationType.NOT_EQUAL,
                "<" => BoolOperationType.LESS_THAN,
                ">" => BoolOperationType.GREATER_THAN,
                "<=" => BoolOperationType.LESS_THAN_OR_EQUAL,
                ">=" => BoolOperationType.GREATER_THAN_OR_EQUAL,
                _ => throw new NoMatchException("Token was not a valid Boolean operator")
            };
        }

        // ================
        // Arithmetic Parsers
        // ================
        private ArithmeticExpression ArithInBrackets()
        {
            if (tokens.Peek().Item1 == TokenType.LPAREN)
            {
                // Pop open bracket
                tokens.Pop();

                // Get the arithmetic Expression
                var aexp = ArithmeticParser();

                // Pop close bracket
                tokens.Pop();

                // Return the Arithmetic Expression
                return aexp;
            }
            else
            {
                // No match found, try another parser
                throw new NoMatchException();
            }
        }

        private ArithmeticExpression Number()
        {
            if (tokens.Peek().Item1 == TokenType.NUMBER)
            {
                return new Number(Int32.Parse(tokens.Pop().Item2));
            }
            else
            {
                throw new NoMatchException();
            }
        }

        private ArithmeticExpression Identifier()
        {
            if (tokens.Peek().Item1 == TokenType.IDENTIFIER)
            {
                return new Var(tokens.Pop().Item2);
            }
            else
            {
                throw new NoMatchException();
            }
        }



        private ArithmeticExpression Fa()
        {
            ArithmeticExpression aexp;
            try
            {
                aexp = ArithInBrackets();
            }
            catch (NoMatchException)
            {
                try
                {
                    aexp = Number();
                }
                catch (NoMatchException)
                {
                    try
                    {
                        aexp = Identifier();
                    }
                    catch (NoMatchException)
                    {
                        throw;
                    }
                }
            }
            return aexp;
        }

        private ArithmeticExpression Te()
        {
            var x = Fa();
            try
            {
                // Check if there's an arithmetic operator
                var op = DefineTeArithmeticOperator(tokens.Peek());

                // Pop the arithmetic operator now that we know there is one
                tokens.Pop();

                var y = Te();
                return new ArithmeticOperation(op, x, y);
            }
            catch (NoMatchException)
            {
                return x;
            }
        }

        private ArithmeticExpression ArithmeticParser()
        {
            var x = Te();
            try
            {
                var op = DefineAEArithmeticOperator(tokens.Peek());
                tokens.Pop();
                var y = ArithmeticParser();

                return new ArithmeticOperation(op, x, y);
            }
            catch (NoMatchException)
            {
                return x;
            }
        }

        // ================
        // Boolean Parsers
        // ================

        // For simple boolean expressions such as "x < y"
        private BooleanExpression SimpleBool()
        {
            ArithmeticExpression x;
            try
            {
                x = ArithmeticParser();
            }
            catch (NoMatchException)
            {
                // No match in this parser
                throw;
            }
            var op = DefineBooleanOperator(tokens.Peek());

            // Pop the Boolean Operator now that we know it's a match
            tokens.Pop();

            var y = ArithmeticParser();

            return new BooleanOperation(op, x, y);
        }

        // For boolean expressions in brackets or Compound Boolean Expressions joined with AND or OR. Eg (x < y) && (z == 4)
        private BooleanExpression BoolBrackets()
        {
            if (tokens.Peek().Item1 == TokenType.LPAREN)
            {
                /*
                 * there is a possibility that the brackets we have entered are part of an
                 * arithmetic expression, and not part of a boolean expression
                 */




                // Pop the open bracket
                tokens.Pop();

                // Get the boolean expression
                var bexp = BooleanParser();

                // Pop the close bracket
                tokens.Pop();

                // Check if there is a compound operator next
                if (tokens.Peek().Item1 == TokenType.COMP_BOOL_OP)
                {
                    var op = tokens.Pop().Item2;
                    var bexp2 = BooleanParser();

                    // Return either an AND or an OR depending on the operation
                    if (op == "&&")
                        return new And(bexp, bexp2);
                    else if (op == "||")
                        return new Or(bexp, bexp2);
                    else
                        throw new Exception(); // Fail the parse if we get here
                }
                else
                {
                    return bexp;
                }
            }
            else
            {
                throw new NoMatchException();
            }
        }

        // For boolean operations that are literally True or False
        private BooleanExpression TrueOrFalse()
        {
            if (tokens.Peek().Item2 == "true")
            {
                tokens.Pop();
                return new True();
            }
            else if (tokens.Peek().Item2 == "false")
            {
                tokens.Pop();
                return new False();
            }
            else
            {
                // No match here
                throw new NoMatchException();
            }
        }

        private BooleanExpression BooleanParser()
        {
            BooleanExpression bexp;
            try
            {
                bexp = BoolBrackets();
            }
            catch (NoMatchException)
            {
                try
                {
                    
                    bexp = SimpleBool();
                }
                catch (NoMatchException)
                {
                    try
                    {
                        bexp = TrueOrFalse();
                    }
                    catch (NoMatchException)
                    {
                        // The expression we got was not a boolean expression
                        // but we expected one if this parser was called
                        // meaning the whole parsing should fail
                        throw new Exception();
                    }
                }
            }
            return bexp;
        }

        // ================
        // Statement Parsers
        // ================

        private Statement Skip()
        {
            var t = tokens.Pop();
            if (t.Item2 == "skip")
                return new Skip();
            else
            {
                tokens.Push(t);
                throw new NoMatchException();
            }
        }

        private Statement Assign()
        {
            if (tokens.Peek().Item1 == TokenType.IDENTIFIER)
            {


                // Store the variable name
                var iden = tokens.Pop().Item2;

                if (tokens.Peek().Item2 == ":=")
                {
                    tokens.Pop();

                    // Get the arithmetic expression to assign to the var
                    ArithmeticExpression aexp = ArithmeticParser();
                    
                    // return
                    return new Assign(iden, aexp);
                }
                else
                {
                    throw new NoMatchException($"Encountered unexpected token\nExpected Identifier");
                }
            }
            throw new NoMatchException($"Encountered unexpected token\nExpected Identifier");
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
            if (tokens.Peek().Item2 == "if")
            {
                tokens.Pop();

                // The if statement appears to always have brackets in it, pop those off
            //    if (tokens.Peek().Item1 == TokenType.LPAREN)
            //        tokens.Pop();

                var bexp = BooleanParser();

           //     if (tokens.Peek().Item1 == TokenType.RPAREN)
             //       tokens.Pop();

                try
                {
                    if (tokens.Pop().Item2 == "then")
                    {
                        var b = BlockParser();
                        if (tokens.Pop().Item2 == "else")
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
                catch (Exception)
                {
                    throw new Exception("Malformed If Statement. Parse Failed");
                }
            }
            else
            {
                throw new NoMatchException();
            }
        }

        private Statement While()
        {
            if (tokens.Peek().Item2 == "while")
            {
                tokens.Pop();
                try
                {
                    var b = BooleanParser();

                    if (tokens.Pop().Item2 == "do")
                    {
                        var bl = BlockParser();
                        return new While(b, bl);
                    }
                    else
                    {
                        throw new Exception();
                    }
                }
                catch (Exception)
                {
                    throw new Exception("Malformed While Statement. Parse Failed");
                }
            }
            else
            {
                throw new NoMatchException();
            }

        }

        private Statement For()
        {
            if (tokens.Peek().Item2 == "for")
            {
                tokens.Pop();

                Assign ass = (Assign) Assign();

                if (tokens.Peek().Item2 == "upto")
                {
                    tokens.Pop();

                    var upto = ArithmeticParser();

                    if (tokens.Peek().Item2 == "do")
                    {
                        tokens.Pop();

                        Block bl = BlockParser();

                        return new For(ass, upto, bl);
                    }
                    else
                    {
                        throw new NoMatchException();
                    }
                }
                else
                {
                    throw new NoMatchException();
                }
            }
            else
            {
                throw new NoMatchException();
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
                                    try
                                    {
                                        stmt = For();
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
            }
            return stmt;
        }

        private Block StatementsParser()
        {
            var s = StatementParser();
            
            // More tokens?
            if (tokens.Count > 0)
            {
                if (tokens.Peek().Item1 == TokenType.SEMICOLON)
                {
                    tokens.Pop();
                    var ss = StatementsParser();
                    return new Block(s.ToList().Concat(ss.statements).ToList());
                }
                else
                {
                    return new Block(s.ToList());
                }
            }
            return new Block(s.ToList()); ;
        }

        private Block BlockParser()
        {
            try
            {
                if (tokens.Peek().Item2 == "{")
                {
                    tokens.Pop();
                    Block stmts = StatementsParser();

                    // pop the closing bracket
                    tokens.Pop();

                    return stmts;
                }
                else
                {
                    throw new NoMatchException();
                }
            }
            catch (NoMatchException)
            {
                return StatementsParser();
            }
        }
    }
}
