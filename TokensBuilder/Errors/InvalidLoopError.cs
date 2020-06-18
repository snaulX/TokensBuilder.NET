using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TokensBuilder.Errors
{
    public class InvalidLoopError : TokensError
    {
        public InvalidLoopError(uint _line, string _message) : base(_line, _message)
        {
        }
    }
}
