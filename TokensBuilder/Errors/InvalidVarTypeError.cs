using System;

namespace TokensBuilder.Errors
{
    class InvalidVarTypeError : TokensError
    {
        public InvalidVarTypeError(uint _line, string _message) : base(_line, _message)
        {
        }
    }
}
