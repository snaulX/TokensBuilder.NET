using System;

namespace TokensBuilder.Errors
{
    class NotInitClassError : TokensError
    {
        public NotInitClassError(uint _line, string _message) : base(_line, _message)
        {
        }
    }
}
