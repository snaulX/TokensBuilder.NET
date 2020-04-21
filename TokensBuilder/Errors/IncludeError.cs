using System;

namespace TokensBuilder.Errors
{
    public class IncludeError : TokensError
    {
        public IncludeError(uint _line, string _message) : base(_line, _message)
        {
        }
    }
}
