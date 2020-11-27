using System;
using System.Collections.Generic;
using System.Text;
using Parser;

namespace CodeGen
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
            
            .method public static writes(Ljava/lang/String ;)V
                .limit stack 2
                .limit locals 1
                getstatic java/lang/System/out Ljava/io/PrintStream;
                aload 0
                invokevirtual java/io/PrintStream/println(Ljava/lang/String ;)V
                return
            .end method

            .method public static main([Ljava/lang/String;)V
               .limit locals 200
               .limit stack 200

            ; COMPILED CODE STARTS

";

        private static readonly string JVM_EPILOGUE = @"
            
            ; COMPILED CODE ENDS
               return

            .end method";


        // Code for the read function
        /*
         *      getstatic java/lang/System/in Ljava/io/InputStream;
         *      invokevirtual java/io/InputStream/read()I 
        */
        private static int labelCount;
        private static int varIndex;
        private Dictionary<string, int> Environment;

        // Creates and stores unique labels
        private struct Label
        {
            public readonly string l;

            public Label(string s)
            {
                l = s + labelCount++;
            }
        }

        public CodeGenerator()
        {
            labelCount = 0;
            varIndex = -1;
            Environment = new Dictionary<string, int>();
        }

        // Writes a string containing compiled code to caller
        public string Compile (Block code, string className)
        {
            return
                (JVM_PROLOGUE +
                CompileBlock(code) +
                JVM_EPILOGUE)
                .Replace("XXX", className);
        }


        private string I(string s) => "\t" + s + "\n";
        private string L(string s) => s + ":\n";

        // Converts Arithmetic Operators to their Java Byte Code equivalent
        private string DefineArithOperator(ArithOperationType aop) => aop switch
        {
            ArithOperationType.PLUS => I("iadd"),
            ArithOperationType.MINUS => I("isub"),
            ArithOperationType.TIMES => I("imul"),
            ArithOperationType.DIVIDE => I("idiv"),
            ArithOperationType.MODULO => I("irem"),
            _ => throw new Exception("Got invalid arithmetic operator")
        };

        // Converts Boolean Operators to their Java Byte Code equivalent
        private string DefineBoolOperator(BoolOperationType bop) => bop switch
        {
            BoolOperationType.EQUAL => "\tif_icmpne ",
            BoolOperationType.NOT_EQUAL => "\tif_icmpeq ",
            BoolOperationType.LESS_THAN => "\tif_icmpge ",
            BoolOperationType.GREATER_THAN => "\tif_icmple ",
            BoolOperationType.LESS_THAN_OR_EQUAL => "\tif_icmpgt ",
            BoolOperationType.GREATER_THAN_OR_EQUAL => "\tif_icmplt ",
            _ => throw new Exception("Got invalid boolean operator")
        };


        private string CompileArithmeticExpression(ArithmeticExpression aexp) => aexp switch
        {
            Number n => I($"ldc {n.num}"),
            Var v => I($"iload {Environment[v.s]} \t\t; {v.s}"),
            ArithmeticOperation aop => CompileArithmeticExpression(aop.a1) + CompileArithmeticExpression(aop.a2) + DefineArithOperator(aop.type),
            _ => throw new Exception("Got invalid Arithmetic Expression")
        };

        private string CompileBooleanExpression(BooleanExpression bexp, Label jmp) => bexp switch
        {
            True => "",
            False => I($"goto {jmp.l}"),
            BooleanOperation bop => CompileArithmeticExpression(bop.a1) + CompileArithmeticExpression(bop.a2) + DefineBoolOperator(bop.type) + jmp.l + "\n",
            _ => throw new Exception("Got invalid Boolean Expression")
        };

        private string CompileStatement(Statement stmt) 
        {
            switch (stmt)
            {
                case Skip:
                    {
                        return "";
                    }
                case Assign ass:
                    {
                        return CompileArithmeticExpression(ass.a) + I($"istore {Environment.GetOrAdd(ass.s, ref varIndex)}\t\t; {ass.s}");
                    }
                case If i:
                    {
                        Label loopStart = new Label("If_else");
                        Label loopEnd = new Label("If_end");
                        string trueBranch = CompileBlock(i.bl1);
                        string elseBranch = CompileBlock(i.bl2);

                        StringBuilder ifBuilder = new StringBuilder();

                        ifBuilder.Append(CompileBooleanExpression(i.a, loopStart));
                        ifBuilder.Append(trueBranch);
                        ifBuilder.Append(I($"goto {loopEnd.l}"));
                        ifBuilder.Append(L(loopStart.l));
                        ifBuilder.Append(elseBranch);
                        ifBuilder.Append(L(loopEnd.l));

                        return ifBuilder.ToString();
                    }
                case While w:
                    {
                        Label loopStart = new Label("Loop_start");
                        Label loopEnd = new Label("Loop_end");
                        string block = CompileBlock(w.bl);

                        StringBuilder whileBuilder = new StringBuilder();

                        whileBuilder.Append(L(loopStart.l));
                        whileBuilder.Append(CompileBooleanExpression(w.b, loopEnd));
                        whileBuilder.Append(block);
                        whileBuilder.Append(I($"goto {loopStart.l}"));
                        whileBuilder.Append(L(loopEnd.l));

                        return whileBuilder.ToString();
                    }
                case Write wr:
                    {
                        if (wr.s.StartsWith("\""))
                        {
                            return
                                I($"ldc {wr.s}\t\t; {wr.s}\n" +
                                $"invokestatic XXX/XXX/writes(Ljava/lang/String;)V");
                        }
                        else
                        {
                            return 
                                I($"iload {Environment[wr.s]}\t\t; {wr.s}\n" +
                                $"invokestatic XXX/XXX/write(I)V");
                        }
                    }
                case For f:
                    {
                        throw new NotImplementedException();
                    }
                case Read r:
                    {
                        throw new NotImplementedException();
                    }
                default:
                    throw new Exception("Got invalid Statement");
            }

        }

        private string CompileBlock (Block bl)
        {
            StringBuilder blBuilder = new StringBuilder();

            for (int i = 0; i < bl.statements.Count; i++)
                blBuilder.Append(CompileStatement(bl.statements[i]));

            return blBuilder.ToString();
        }
    }
}
