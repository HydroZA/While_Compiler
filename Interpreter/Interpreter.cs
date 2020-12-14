using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using Parser;

namespace Interpreter
{
    public class Interpreter
    {
        public Dictionary<string, int> Environment;

        public Interpreter()
        {
            Environment = new Dictionary<string, int>();
        }

        public Dictionary<string, int> Interpret(Block code)
        {
            EvaluateBlock(code);
            return Environment;
        }

        private void EvaluateBlock(Block code)
        {
            for (int i = 0; i < code.statements.Count; i++)
            {
                EvaluateStatement(code.statements[i]);
            }
        }

        private void EvaluateStatement(Statement s)
        {
            switch (s)
            {
                case Skip:
                    break;
                case Assign aexp:
                    {
                        Environment.AddOrUpdate(aexp.s, EvaluateArithmeticExpression(aexp.a));
                        break;
                    }
                case If i:
                    {
                        if (EvaluateBooleanExpression(i.a))
                        {
                            EvaluateBlock(i.bl1);
                        }
                        else
                        {
                            EvaluateBlock(i.bl2);
                        }
                        break;
                    }
                case While w:
                    {
                        while (EvaluateBooleanExpression(w.b))
                        {
                            EvaluateBlock(w.bl);
                        }
                        break;
                    }
                case Write wr:
                    {
                        // write could either be for a string or variable
                        if (wr.s[0] == '\"')
                        {
                            // Use a regex to strip the " from the string
                            Console.WriteLine(Regex.Replace(wr.s, @"""", ""));
                        }
                        else
                        {
                            // write the variable contents to stdOut
                            Console.WriteLine(Environment.GetValueOrDefault(wr.s));
                        }
                        break;
                    }
                case Read r:
                    {
                        try
                        {
                            Environment.AddOrUpdate(r.id, Int32.Parse(Console.ReadLine()));
                        }
                        catch (Exception)
                        {
                            Console.WriteLine("Read only accepts integer input, exiting");
                        }
                        break;
                    }
                case For f:
                    {
                        EvaluateStatement(f.var);
                        for (int i = Environment.GetValueOrDefault(f.var.s); i <= EvaluateArithmeticExpression(f.upto); i++)
                        {
                            EvaluateBlock(f.bl);
                            Environment.AddOrUpdate(f.var.s, Environment.GetValueOrDefault(f.var.s) + 1);
                        }
                        break;
                    }
            };
        }

        

        private int EvaluateArithmeticExpression(ArithmeticExpression aexp) =>
            aexp switch
            {
                Number num => num.num,
                Var var => Environment.GetValueOrDefault(var.s),
                ArithmeticOperation aop => aop.type switch
                {
                    ArithOperationType.PLUS => EvaluateArithmeticExpression(aop.a1) + EvaluateArithmeticExpression(aop.a2),
                    ArithOperationType.MINUS => EvaluateArithmeticExpression(aop.a1) - EvaluateArithmeticExpression(aop.a2),
                    ArithOperationType.TIMES => EvaluateArithmeticExpression(aop.a1) * EvaluateArithmeticExpression(aop.a2),
                    ArithOperationType.DIVIDE => EvaluateArithmeticExpression(aop.a1) / EvaluateArithmeticExpression(aop.a2),
                    ArithOperationType.MODULO => EvaluateArithmeticExpression(aop.a1) % EvaluateArithmeticExpression(aop.a2)
                },
                
           };

        private bool EvaluateBooleanExpression(BooleanExpression bexp) =>
            bexp switch
            {
                True => true,
                False => false,
                BooleanOperation bop => bop.type switch
                {
                    BoolOperationType.EQUAL => EvaluateArithmeticExpression(bop.a1) == EvaluateArithmeticExpression(bop.a2),
                    BoolOperationType.NOT_EQUAL => EvaluateArithmeticExpression(bop.a1) != EvaluateArithmeticExpression(bop.a2),
                    BoolOperationType.GREATER_THAN => EvaluateArithmeticExpression(bop.a1) > EvaluateArithmeticExpression(bop.a2),
                    BoolOperationType.LESS_THAN => EvaluateArithmeticExpression(bop.a1) < EvaluateArithmeticExpression(bop.a2),
                    BoolOperationType.GREATER_THAN_OR_EQUAL => EvaluateArithmeticExpression(bop.a1) >= EvaluateArithmeticExpression(bop.a2),
                    BoolOperationType.LESS_THAN_OR_EQUAL => EvaluateArithmeticExpression(bop.a1) <= EvaluateArithmeticExpression(bop.a2),

                },
                And and => EvaluateBooleanExpression(and.b1) && EvaluateBooleanExpression(and.b2),
                Or or => EvaluateBooleanExpression(or.b1) || EvaluateBooleanExpression(or.b2)
            };

    }
}
