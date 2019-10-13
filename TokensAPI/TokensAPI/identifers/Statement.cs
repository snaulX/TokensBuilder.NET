using System;
using System.Collections.Generic;
using System.Text;

namespace TokensAPI.identifers
{
    public class Statement : Identifer
    {
        public Token token;
        public List<Identifer> args;

        public Statement()
        {
            identifer = "";
            args = new List<Identifer>();
        }

        public Statement(string identifer): this()
        {
            if (Check(identifer)) this.identifer = identifer;
        }

        public override bool Check(string input) => input.StartsWith('(') && input.EndsWith(')');

        public override void Parse(string input)
        {
            throw new NotImplementedException();
        }
    }
}
