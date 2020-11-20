using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Parser
{
    public abstract class Statement { }


    public class Block
    {
        public List<Statement> statements;
        public Block(List<Statement> statements)
        {
            this.statements = statements;
        }
    }

    public class Skip : Statement { }

    public class If : Statement
    {
        public BooleanExpression a;
        public Block bl1, bl2;
        
        public If(BooleanExpression a, Block bl1, Block bl2)
        {
            this.a = a;
            this.bl1 = bl1;
            this.bl2 = bl2;
        }
    }

    public class While : Statement
    {
        public BooleanExpression b;
        public Block bl;

        public While(BooleanExpression b, Block bl)
        {
            this.b = b;
            this.bl = bl;
        }
    }

    public class Assign : Statement
    {
        public string s;
        public ArithmeticExpression a;

        public Assign(string s, ArithmeticExpression a)
        {
            this.s = s;
            this.a = a;
        }
    }

    public class Write : Statement
    {
        public string s;

        public Write(string s)
        {
            this.s = s;
        }
    }

    public class Read : Statement
    {
        public string id;

        public Read(string id)
        {
            this.id = id;
        }
    }
}
