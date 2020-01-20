using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace TokensBuilder
{
    public class Parser
    {
        public ContextInfo context;

        public Parser(ContextInfo _context)
        {
            context = _context;
        }

        public object ParseLine(string line)
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
