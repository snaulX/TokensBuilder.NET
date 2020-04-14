using System;
using System.Collections.Generic;

namespace TokensBuilder.Errors
{
    /// <summary>
    /// Error with actual/expect problems or platform problems
    /// </summary>
    public class PlatformImplementationError : TokensError
    {
        public PlatformImplementationError(uint _line, string _message) : base(_line, _message)
        {
        }
    }
}
