using System.Collections.Generic;
using System.Reflection.Emit;
using TokensAPI;
using System.Reflection;

namespace TokensBuilder
{
    public class Expression
    {
        public Token token;
        public List<Identifer> args;

        public Expression()
        {
            token = Token.NULL;
            args = new List<Identifer>();
        }
    }

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
    }
}
