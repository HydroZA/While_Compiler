using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using lex;
using System.ComponentModel.Design;
using System;

namespace UnitTests
{
    [TestClass]
    public class TestLexing
    {
        Lexer lexer = new Lexer();
        Rexp rules = WhileLexingRules.rules;

        [TestMethod]
        public void TestRemoveWhitespace()
        { 
            List<(string, string)> input = new List<(string, string)>()
            {
                ("KEYWORD", "read"),
                ("WHITESPACE", " "),
                ("COMMENT", "// this is a comment"),
                ("IDENTIFIER", "n")
            };

            List<(string, string)> expected = new List<(string, string)>()
            {
                ("KEYWORD", "read"),
                ("IDENTIFIER", "n")
            };

            List<(string, string)> actual = lexer.RemoveWhitespace(input);
            CollectionAssert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void TestRemoveWhitespaceFromString()
        {
            string input = "read n; // this line reads n";
            string expected = "read n; ";
            Assert.AreEqual(expected, lexer.RemoveWhitespaceFromString(input));
        }

        [TestMethod]
        public void TestKeywords()
        {
            string input = "read";
            List<(string, string)> expected = new List<(string, string)>()
            {
                ("KEYWORD", input)
            };

            List<(string, string)> actual = lexer.Lex(rules, input);
            CollectionAssert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void TestIdentifiers()
        {
            string input = "n_iden";
            List<(string, string)> expected = new List<(string, string)>()
            {
                ("IDENTIFIER", input)
            };

            List<(string, string)> actual = lexer.Lex(rules, input);
            CollectionAssert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void TestOperators()
        {
            string input = "==";
            List<(string, string)> expected = new List<(string, string)>()
            {
                ("OPERATOR", input)
            };

            List<(string, string)> actual = lexer.Lex(rules, input);
            CollectionAssert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void TestNumbers()
        {
            string input = "169";
            List<(string, string)> expected = new List<(string, string)>()
            {
                ("NUMBER", input)
            };

            List<(string, string)> actual = lexer.Lex(rules, input);
            CollectionAssert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void TestComments()
        {
            string input = "//this is a really long comment and i hope it successfully tests this regex";
            List<(string, string)> expected = new List<(string, string)>()
            {

            };

            List<(string, string)> actual = lexer.Lex(rules, input);
            CollectionAssert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void TestStrings()
        {
            string input = "\"this is a string<1\"";
            List<(string, string)> expected = new List<(string, string)>()
            {
                ("STRING", input)
            };

            List<(string, string)> actual = lexer.Lex(rules, input);
            CollectionAssert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void TestSemicolon()
        {
            string input = ";";
            List<(string, string)> expected = new List<(string, string)>()
            {
                ("SEMI-COLON", input)
            };

            List<(string, string)> actual = lexer.Lex(rules, input);
            CollectionAssert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void TestWhitespace()
        {
            string input = "\n      \t";

            List<(string, string)> expected = new List<(string, string)>()
            {

            };

            List<(string, string)> actual = lexer.Lex(rules, input);
            CollectionAssert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void TestReadN()
        {
            string input = "read n";

            List<(string, string)> expected = new List<(string, string)>()
            {
                ("KEYWORD", "read"),
                ("IDENTIFIER", "n")
            };

            List<(string, string)> actual = lexer.Lex(rules, input);
            CollectionAssert.AreEqual(expected, actual);
        }



        [TestMethod]
        public void TestFibonnaci()
        {
            string input = @"write ""Fib""
                read n;
                minus1:= 0;
                (minus2) := 1;
                while n > 0 do
                {
                    temp:= minus2;
                    minus2:= minus1 + minus2;
                    minus1:= temp;
                    n:= n - 1
                };
                write ""Result"";
                write minus2;
            ";


            List<(string, string)> expected = new List<(string, string)>()
            {
                ("KEYWORD", "write"),
                ("STRING", "\"Fib\""),
                ("KEYWORD", "read"),
                ("IDENTIFIER", "n"),
                ("SEMI-COLON", ";"),
                ("IDENTIFIER", "minus1"),
                ("OPERATOR", ":="),
                ("NUMBER", "0"),
                ("SEMI-COLON", ";"),
                ("LPAR", "("),
                ("IDENTIFIER", "minus2"),
                ("RPAR", ")"),
                ("OPERATOR", ":="),
                ("NUMBER", "1"),
                ("SEMI-COLON", ";"),
                ("KEYWORD", "while"),
                ("IDENTIFIER", "n"),
                ("OPERATOR", ">"),
                ("NUMBER", "0"),
                ("KEYWORD", "do"),
                ("LPAR", "{"),
                ("IDENTIFIER", "temp"),
                ("OPERATOR", ":="),
                ("IDENTIFIER", "minus2"),
                ("SEMI-COLON", ";"),
                ("IDENTIFIER", "minus2"),
                ("OPERATOR", ":="),
                ("IDENTIFIER", "minus1"),
                ("OPERATOR", "+"),
                ("IDENTIFIER", "minus2"),
                ("SEMI-COLON", ";"),
                ("IDENTIFIER", "minus1"),
                ("OPERATOR", ":="),
                ("IDENTIFIER", "temp"),
                ("SEMI-COLON", ";"),
                ("IDENTIFIER", "n"),
                ("OPERATOR", ":="),
                ("IDENTIFIER", "n"),
                ("OPERATOR", "-"),
                ("NUMBER", "1"),
                ("RPAR", "}"),
                ("SEMI-COLON", ";"),
                ("KEYWORD", "write"),
                ("STRING", "\"Result\""),
                ("SEMI-COLON", ";"),
                ("KEYWORD", "write"),
                ("IDENTIFIER", "minus2"),
                ("SEMI-COLON", ";")
            };

            List<(string, string)> actual = lexer.Lex(rules, input);
            CollectionAssert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void TestLoops()
        {
            string input = @"start := 1000;
                x := start;
                y := start;
                z := start;
                while 0 < x do {
                 while 0 < y do {
                  while 0 < z do {
                    z := z - 1
                  };
                  z := start;
                  y := y - 1
                 };     
                 y := start;
                 x := x - 1
                }
            ";


            List<(string, string)> expected = new List<(string, string)>()
            {
                ("IDENTIFIER", "start"),
                ("OPERATOR", ":="),
                ("NUMBER", "1000"),
                ("SEMI-COLON", ";"),
                ("IDENTIFIER", "x"),
                ("OPERATOR", ":="),
                ("IDENTIFIER", "start"),
                ("SEMI-COLON", ";"),
                ("IDENTIFIER", "y"),
                ("OPERATOR", ":="),
                ("IDENTIFIER", "start"),
                ("SEMI-COLON", ";"),
                ("IDENTIFIER", "z"),
                ("OPERATOR", ":="),
                ("IDENTIFIER", "start"),
                ("SEMI-COLON", ";"),
                ("KEYWORD", "while"),
                ("NUMBER", "0"),
                ("OPERATOR", "<"),
                ("IDENTIFIER", "x"),
                ("KEYWORD", "do"),
                ("LPAR", "{"),
                ("KEYWORD", "while"),
                ("NUMBER", "0"),
                ("OPERATOR", "<"),
                ("IDENTIFIER", "y"),
                ("KEYWORD", "do"),
                ("LPAR", "{"),
                ("KEYWORD", "while"),
                ("NUMBER", "0"),
                ("OPERATOR", "<"),
                ("IDENTIFIER", "z"),
                ("KEYWORD", "do"),
                ("LPAR", "{"),
                ("IDENTIFIER", "z"),
                ("OPERATOR", ":="),
                ("IDENTIFIER", "z"),
                ("OPERATOR", "-"),
                ("NUMBER", "1"),
                ("RPAR", "}"),
                ("SEMI-COLON", ";"),
                ("IDENTIFIER", "z"),
                ("OPERATOR", ":="),
                ("IDENTIFIER", "start"),
                ("SEMI-COLON", ";"),
                ("IDENTIFIER", "y"),
                ("OPERATOR", ":="),
                ("IDENTIFIER", "y"),
                ("OPERATOR", "-"),
                ("NUMBER", "1"),
                ("RPAR", "}"),
                ("SEMI-COLON", ";"),
                ("IDENTIFIER", "y"),
                ("OPERATOR", ":="),
                ("IDENTIFIER", "start"),
                ("SEMI-COLON", ";"),
                ("IDENTIFIER", "x"),
                ("OPERATOR", ":="),
                ("IDENTIFIER", "x"),
                ("OPERATOR", "-"),
                ("NUMBER", "1"),
                ("RPAR", "}")
            };

            List<(string, string)> actual = lexer.Lex(rules, input);
            CollectionAssert.AreEqual(expected, actual);
        }
    }
}
