using System;
using System.Collections.Generic;
using System.Text;

namespace TokensAPI.identifers
{
    public class Array : Identifer
    {
        public List<Identifer> elements;

        public Array()
        {
            identifer = "";
            elements = new List<Identifer>();
        }

        public override bool Check(string input) => input.StartsWith('[') && input.EndsWith(']');

        public override void Parse(string input)
        {
            if (Check(input))
            {
                string[] ids = input.Remove(0, 1).Remove(input.Length - 2).Split(',');
            }
        }
    }
}
