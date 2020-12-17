using System;
using System.Collections.Generic;
using System.Text;
using Parser;

namespace CodeGen
{
    public class CodeGenerator
    {
        private const string JVM_PROLOGUE = @"
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
            
.method public static writes(Ljava/lang/String;)V
    .limit stack 2
    .limit locals 1
    getstatic java/lang/System/out Ljava/io/PrintStream;
    aload 0
    invokevirtual java/io/PrintStream/println(Ljava/lang/String;)V
    return
.end method

.method public static read()I
	.limit locals 10
	.limit stack 10
	ldc 0
	istore 1                ; this will hold our final integer
Label1:
	getstatic java/lang/System/in Ljava/io/InputStream;
	invokevirtual java/io/InputStream/read()I
	istore 2
	iload 2
	ldc 10                  ; the newline delimiter for (Unix 10) (Windows 13)
	isub
	ifeq Label2
	iload 2
	ldc 32                  ; the space delimiter
	isub
	ifeq Label2
	iload 2
	ldc 48 		            ; we have our digit in ASCII, have to subtract it from 48
	isub
	ldc 10
	iload 1
	imul
	iadd
	istore 1
	goto Label1
Label2:
	;when we come here we have our integer computed in Local Variable 1
	iload 1
	ireturn
.end method
            
.method public static main([Ljava/lang/String;)V
    .limit locals 200
    .limit stack 200

; COMPILED CODE STARTS

";

        private const string JVM_EPILOGUE = @"
; COMPILED CODE ENDS
    return

.end method";

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

        // returns a string containing compiled code to caller
        public string Compile (Block code, string className)
        {
            return
                (JVM_PROLOGUE +
                CompileBlock(code) +
                JVM_EPILOGUE)
                .Replace("XXX", className);
        }

        private static string I(string s) => "\t" + s + "\n";
        private static string L(string s) => s + ":\n";

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

        // Converts Boolean Operators to their (inverted) Java Byte Code equivalent 
        private string DefineBoolOperator(BoolOperationType bop, bool invert=false) => bop switch
        {
            BoolOperationType.EQUAL => invert ? "\tif_icmpeq " : "\tif_icmpne ",
            BoolOperationType.NOT_EQUAL => invert ? "\tif_icmpne " : "\tif_icmpeq ",
            BoolOperationType.LESS_THAN => invert ? "\tif_icmplt " : "\tif_icmpge ",
            BoolOperationType.GREATER_THAN => invert ? "\tif_icmpgt " : "\tif_icmple ",
            BoolOperationType.LESS_THAN_OR_EQUAL => invert ? "\tif_icmple " : "\tif_icmpgt ",
            BoolOperationType.GREATER_THAN_OR_EQUAL => invert ? "\tif_icmpge " : "\tif_icmplt ",
            _ => throw new Exception("Got invalid boolean operator")
        };

        private string CompileArithmeticExpression(ArithmeticExpression aexp) => aexp switch
        {
            Number n => I($"ldc {n.num}"),
            Var v => I($"iload {Environment[v.s]} \t\t; {v.s}"),
            ArithmeticOperation aop =>  CompileArithmeticExpression(aop.a1) +
                                        CompileArithmeticExpression(aop.a2) +
                                        DefineArithOperator(aop.type),
            _ => throw new Exception("Got invalid Arithmetic Expression")
        };

        private string CompileBooleanExpression(BooleanExpression bexp, Label jmp, bool invert=false) => bexp switch
        {
            True => "",
            False => I($"goto {jmp.l}"),
            BooleanOperation bop => CompileArithmeticExpression(bop.a1) +
                                    CompileArithmeticExpression(bop.a2) +
                                    DefineBoolOperator(bop.type, invert) + jmp.l + "\n",
            And a => CompileBooleanExpression(a.b1, jmp) + CompileBooleanExpression(a.b2, jmp),
            Or o => throw new NotImplementedException(),
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
                        return CompileArithmeticExpression(ass.a) + I($"istore {Environment.IndexGetOrAdd(ass.s, ref varIndex)}\t\t; {ass.s}");
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
                                I($"ldc {wr.s}\t\t; {wr.s}") +
                                I($"invokestatic XXX/XXX/writes(Ljava/lang/String;)V");
                        }
                        else
                        {
                            return 
                                I($"iload {Environment[wr.s]}\t\t; {wr.s}") +
                                I($"invokestatic XXX/XXX/write(I)V");
                        }
                    }
                case For f:
                    {
                        Label loopStart = new Label("Loop_start");
                        Label loopEnd = new Label("Loop_end");

                        StringBuilder forBuilder = new StringBuilder();

                        //Compile the "incrementer" variable and place on heap
                        forBuilder.Append(CompileStatement(f.var));
                        
                        // label the start of the loop
                        forBuilder.Append(L(loopStart.l));

                        // load the incrementer variable to the stack
                        forBuilder.Append(I($"iload {Environment[f.var.s]}"));

                        // load the upto variable to the stack
                        forBuilder.Append(CompileArithmeticExpression(f.upto));

                        // check if the incrementer variable is less than or equal the upto value
                        forBuilder.Append(I($"if_icmpgt {loopEnd.l}"));

                        // true branch
                        forBuilder.Append(CompileBlock(f.bl));
                        // increment the incrementer variable by 1
                        forBuilder.Append(I($"iinc {Environment[f.var.s]} 1"));
                        forBuilder.Append(I($"goto {loopStart.l}"));

                        // false branch
                        forBuilder.Append(L(loopEnd.l));

                        return forBuilder.ToString();
                    }
                case Read r:
                    {
                        return
                            I("invokestatic XXX/XXX/read()I") +
                            I($"istore {Environment.IndexGetOrAdd(r.id, ref varIndex)}\t\t;{r.id}");
                    }
                default:
                    throw new Exception("Got invalid Statement");
            }

        }

        private string CompileBlock (Block bl)
        {
            StringBuilder blockBuilder = new StringBuilder();

            for (int i = 0; i < bl.statements.Count; i++)
                blockBuilder.Append(CompileStatement(bl.statements[i]));

            return blockBuilder.ToString();
        }
    }
}
