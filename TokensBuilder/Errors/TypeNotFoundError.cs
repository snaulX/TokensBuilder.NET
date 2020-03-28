using System;

namespace TokensBuilder.Errors
{
    public class TypeNotFoundError : TokensError
    {
        public TypeNotFoundError(uint _line, string _message) : base(_line, _message)
        {
        }
    }
}
