using System;
using System.Collections.Generic;

namespace TokensAPI.identifers
{
    public class Array : Identifer
    {
        public List<Identifer> elements;
        private string input;

        public Array()
        {
            identifer = "";
            elements = new List<Identifer>();
        }

        public Array(string input)
        {
            this.input = input;
            elements = new List<Identifer>();
        }

        public override bool Check(string input) => input.StartsWith('[') && input.EndsWith(']');

        public override void Parse(string input)
        {
            if (Check(input))
            {
                string[] ids = input.Remove(0, 1).Remove(input.Length - 2).Split(',');
                for (int i = 0; i < ids.Length; i++)
                {
                    elements.Add(GetIdentifer(ids[i]));
                }
            }
        }
    }
}
