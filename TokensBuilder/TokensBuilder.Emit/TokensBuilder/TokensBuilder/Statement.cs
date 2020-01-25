using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;

namespace TokensBuilder
{
    public class Statement
    {
        string left, right;
        OpCode statement_operator;
        MethodInfo statement_method;

        public Statement()
        {
            left = "";
            right = "";
        }

        public Statement(string statement)
        {
            string[] parts = statement.Split(' ');
        }
    }
}
