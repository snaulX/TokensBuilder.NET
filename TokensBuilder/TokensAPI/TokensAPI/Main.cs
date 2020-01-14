using System;
using System.Collections.Generic;

namespace TokensAPI
{
    public static class Main
    {
        public static Token GetToken(string name)
        {
            try
            {
                return (Token) Enum.Parse(typeof(Token), name);
            }
            catch (KeyNotFoundException)
            {
                throw new KeyNotFoundException($"Token by name {name} not found"); //it`s really genial
            }
        }
    }
}
