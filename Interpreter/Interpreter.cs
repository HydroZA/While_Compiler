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
            // Check if the block is empty
            if (code.statements.Count == 0)
            {
                return;
            }
            else
            {
                Statement current = code.statements[0];
                code.statements.RemoveAt(0);

            }
        }

        private void EvaluateStatement(Statement s)
        {

        }

    }
}
