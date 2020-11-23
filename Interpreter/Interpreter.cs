using System;
using System.Collections.Generic;
using System.Text;
using Parser;

namespace Interpreter
{
    public class Interpreter
    {
        private Dictionary<string, int> Environment;

        public Interpreter()
        {
            Environment = new Dictionary<string, int>();
        }

        public string Interpret(Block code)
        {
            EvaluateBlock(code);
        }

        private void EvaluateBlock(Block code)
        {
            int i = 0;
            while (code.statements.Count > 0)
            {
                EvaluateStatement(code.statements[i]);
            }
        }

        private void EvaluateStatement(Statement s)
        {
            s switch
            {
                // C# does not have a "do nothing" statement, so we just return true into the void
                Skip => true,
                Assign aexp => 


            }
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
