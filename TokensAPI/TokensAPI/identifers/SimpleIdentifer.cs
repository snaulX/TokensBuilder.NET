using System;
using System.Collections.Generic;
using System.Text;

namespace TokensAPI.identifers
{
    public class SimpleIdentifer : Identifer
    {
        public string identifer = "";

        public SimpleIdentifer(): base()
        {
        }

        public SimpleIdentifer(string identifer) : this()
        {
            this.identifer = identifer;
        }

        public static bool Check(string input)
        {
            foreach (char c in input)
            {
                if (!char.IsLetterOrDigit(c)) return false;
            }
            return true;
        }
    }
}
