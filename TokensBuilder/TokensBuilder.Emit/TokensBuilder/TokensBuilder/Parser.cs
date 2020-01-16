using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace TokensBuilder
{
    public static class Parser
    {
        public static object ParseLine(string line, ContextInfo context)
        {
            object obj = "";
            line = line.Trim();
            StringBuilder buffer = new StringBuilder();
            char current = line[0];
            while (char.IsWhiteSpace(current))
            {
                //pass
            }
            return obj;
        }
    }
}
