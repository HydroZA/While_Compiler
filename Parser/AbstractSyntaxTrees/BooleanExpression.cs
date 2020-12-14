using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Parser
{
    public abstract class BooleanExpression { }

    public enum BoolOperationType
    {
        LESS_THAN,
        GREATER_THAN,
        LESS_THAN_OR_EQUAL,
        GREATER_THAN_OR_EQUAL,
        NOT_EQUAL,
        EQUAL
    }
    public class True : BooleanExpression { }

    public class False : BooleanExpression { }

    public class And : BooleanExpression
    {
        public BooleanExpression b1, b2;

        public And(BooleanExpression b1, BooleanExpression b2)
        {
            this.b1 = b1;
            this.b2 = b2;
        }
    }
    public class Or : BooleanExpression
    {
        public BooleanExpression b1, b2;

        public Or(BooleanExpression b1, BooleanExpression b2)
        {
            this.b1 = b1;
            this.b2 = b2;
        }
    }

    public class BooleanOperation : BooleanExpression
    {
        public ArithmeticExpression a1, a2;
        public BoolOperationType type;

        public BooleanOperation(BoolOperationType type, ArithmeticExpression a1, ArithmeticExpression a2)
        {
            this.type = type;
            this.a1 = a1;
            this.a2 = a2;
        }
    }
}
