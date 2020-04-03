using System;
using System.Collections.Generic;
using System.Linq;

namespace TokensBuilder.Errors
{
    public class TokensLibraryError : TokensError
    {
        public TokensLibraryError(uint _line, string _message) : base(_line, _message)
        {
        }
    }
}
