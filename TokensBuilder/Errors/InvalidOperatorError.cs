using System;

namespace TokensBuilder.Errors
{
    public class InvalidOperatorError : TokensError
    {
        public InvalidOperatorError(uint _line, string _message) : base(_line, _message)
        {
        }
    }
}
