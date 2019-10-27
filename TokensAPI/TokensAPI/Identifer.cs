using System;
using System.Collections.Generic;
using System.Text;
using TokensAPI.identifers;

namespace TokensAPI
{
    public abstract class Identifer
    {
        public string identifer;
        public abstract void Parse(string input);
        public abstract bool Check(string input);
        public static Identifer GetIdentifer(string input)
        {
            Statement statement = new Statement();
            identifers.Array array = new identifers.Array();
            LongIdentifer longIdentifer = new LongIdentifer();
            SimpleIdentifer simpleIdentifer = new SimpleIdentifer();
            if (statement.Check(input)) return new Statement(input);
            else if (array.Check(input)) return new identifers.Array(input);
            else if (longIdentifer.Check(input)) return new LongIdentifer(input);
            else if (simpleIdentifer.Check(input)) return new SimpleIdentifer(input);
            else throw new ArgumentNullException($"{input} is not identifer");
        }
        public Identifer Null { get => new SimpleIdentifer(); }
        public abstract string GetValue();
        public override string ToString() => GetValue();
    }
}
