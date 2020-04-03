using System;
using System.Collections.Generic;

namespace TokensBuilder.Errors
{
    public class DirectiveError : TokensError
    {
        public DirectiveError(uint _line, string _message) : base(_line, _message)
        {
        }
    }
}
