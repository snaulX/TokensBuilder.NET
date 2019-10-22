using System;
using System.Collections.Generic;

namespace TokensAPI.identifers
{
    public class LongIdentifer : Identifer
    {
        public LongIdentifer()
        {
            identifer = "";
        }

        public LongIdentifer(string identifer): this()
        {
            if (Check(identifer)) this.identifer = identifer;
        }

        public override bool Check(string input) => input.StartsWith('"') && input.EndsWith('"');

        public override void Parse(string input)
        {
            if (Check(input)) identifer = input;
        }

        public string GetValue() => identifer.Remove(0, 1).Remove(identifer.Length - 2);
    }
}
