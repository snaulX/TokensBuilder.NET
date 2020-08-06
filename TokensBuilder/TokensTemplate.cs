using System;
using System.Collections.Generic;
using System.Linq;

namespace TokensBuilder
{
    interface TokensTemplate
    {
        bool Parse(ref Generator gen);
    }
}
