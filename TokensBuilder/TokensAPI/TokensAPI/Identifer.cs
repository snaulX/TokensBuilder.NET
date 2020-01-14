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
            LongIdentifer longIdentifer = new LongIdentifer();
            SimpleIdentifer simpleIdentifer = new SimpleIdentifer();
            if (longIdentifer.Check(input)) return new LongIdentifer(input);
            else if (simpleIdentifer.Check(input)) return new SimpleIdentifer(input);
            else throw new ArgumentNullException($"{input} is not identifer");
        }
        public Identifer Null { get => new SimpleIdentifer(); }
        public virtual string GetValue() => identifer;
        public override string ToString() => GetValue();
    }
}
