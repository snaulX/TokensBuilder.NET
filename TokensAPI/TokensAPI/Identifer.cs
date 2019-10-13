using System;
using System.Collections.Generic;
using System.Text;

namespace TokensAPI
{
    public abstract class Identifer
    {
        public string identifer;
        public abstract void Parse(string input);
        public abstract bool Check(string input);
    }
}
