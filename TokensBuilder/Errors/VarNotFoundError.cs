using System;

namespace TokensBuilder.Errors
{
    public class VarNotFoundError : TokensError
    {
        public VarNotFoundError(uint _line, string _message) : base(_line, _message)
        {
        }
    }
}
