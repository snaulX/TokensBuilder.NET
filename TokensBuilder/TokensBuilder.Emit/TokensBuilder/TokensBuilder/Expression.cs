using System.Collections.Generic;
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
    }
}
