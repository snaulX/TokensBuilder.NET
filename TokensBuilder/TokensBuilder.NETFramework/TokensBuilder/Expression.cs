using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TokensAPI;

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

        public Expression(Token token, List<Identifer> args)
        {
            this.token = token;
            this.args = args ?? throw new ArgumentNullException(nameof(args));
        }
    }
}
