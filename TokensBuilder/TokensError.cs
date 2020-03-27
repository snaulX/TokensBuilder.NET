using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TokensBuilder
{
    public class TokensError
    {
        public uint line;
        public string message;

        public TokensError(uint _line, string _message)
        {
            line = _line;
            message = _message;
        }

        public override string ToString() => $"{GetType().Name} in line {line}. {message}";
    }
}
