using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Parser
{
    public abstract class ArithmeticExpression { }
    public enum ArithOperationType
    {
        PLUS,
        MINUS,
        TIMES,
        DIVIDE,
        MODULO
    }
    public class Var : ArithmeticExpression
    {
        public string s;
        
        public Var(string s)
        {
            this.s = s;
        }
    }

    public class Number : ArithmeticExpression
    {
        public int num;

        public Number(int num)
        {
            this.num = num;
        }
    }

    public class ArithmeticOperation : ArithmeticExpression
    {
        public ArithmeticExpression a1, a2;
        public ArithOperationType type;

        public ArithmeticOperation(ArithOperationType type, ArithmeticExpression a1, ArithmeticExpression a2)
        {
            this.type = type;
            this.a1 = a1;
            this.a2 = a2;
        }
    }

    // This code is from before i realized it would be much easier to just make an enum
/*
    public class Number : ArithmeticExpression
    {
        public int x;

        public Number(int x)
        {
            this.x = x;
        }
    }

    public class Plus : ArithmeticExpression
    {
        public ArithmeticExpression a1, a2;

        public Plus(ArithmeticExpression a1, ArithmeticExpression a2)
        {
            this.a1 = a1;
            this.a2 = a2;
        }
    }

    public class Minus : ArithmeticExpression
    {
        public ArithmeticExpression a1, a2;

        public Minus(ArithmeticExpression a1, ArithmeticExpression a2)
        {
            this.a1 = a1;
            this.a2 = a2;
        }
    }

    public class Times : ArithmeticExpression
    {
        public ArithmeticExpression a1, a2;

        public Times(ArithmeticExpression a1, ArithmeticExpression a2)
        {
            this.a1 = a1;
            this.a2 = a2;
        }
    }

    public class Divide : ArithmeticExpression
    {
        public ArithmeticExpression a1, a2;

        public Divide(ArithmeticExpression a1, ArithmeticExpression a2)
        {
            this.a1 = a1;
            this.a2 = a2;
        }
    }

    public */
      
}
