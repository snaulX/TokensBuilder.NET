using System;
using System.Collections.Generic;

namespace TokensAPI
{
    public abstract class Token
    {
        public abstract Identifer Parse(params Identifer[] identifers);

        public static Token GetToken(string name)
        {
            try
            {
                return Main.tokens[name];
            }
            catch (KeyNotFoundException)
            {
                throw new KeyNotFoundException($"Token by name {name} not found"); //it`s really genial
            }
        }
    }
}
