using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TokensBuilder.Errors
{
    public class InvalidHeaderError : TokensError
    {
        public InvalidHeaderError(uint _line, string _message) : base(_line, _message)
        {
        }

        public InvalidHeaderError(uint _line, HeaderType header, string why)
            : base(_line, $"Invalid header {header} becouse {why}")
        {
        }
    }
}
