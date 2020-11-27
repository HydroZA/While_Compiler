using System;
using System.Collections.Generic;
using Parser;

namespace Compiler
{
    public class CodeGenerator
    {
        private static readonly string JVM_PROLOGUE = @"
            .class public XXX.XXX
            .super java/lang/Object

            .method public static write(I)V 
                .limit locals 1 
                .limit stack 2 
                getstatic java/lang/System/out Ljava/io/PrintStream; 
                iload 0
                invokevirtual java/io/PrintStream/println(I)V 
                return 
            .end method

            .method public static main([Ljava/lang/String;)V
               .limit locals 200
               .limit stack 200

            ; COMPILED CODE STARTS";

        private static readonly string JVM_EPILOGUE = @"
            ; COMPILED CODE ENDS
               return

            .end method";

        private int labelCount;
        private Dictionary<string, int> Environment;

        public CodeGenerator()
        {
            labelCount = 0;
            Environment = new Dictionary<string, int>();
        }

        // Writes a string containing compiled code to caller
        public string Compile (Block code)
        {
            
        }

        // Returns a new unique label
        private string GetLabel(string s) => s + labelCount++;


        private string CompileArithmeticExpression(ArithmeticExpression aexp) => aexp switch
        {
            Number => 
        };

    }
}
