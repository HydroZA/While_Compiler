﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Parser
{
    class Program
    {
        static void Main(string[] args)
        {
            var wp = new WhileParser();
            Block bl = wp.Parse("if(a<b)thenskipelsea:=(a*b)+1");
            Console.WriteLine("done");
        }
    }
}
