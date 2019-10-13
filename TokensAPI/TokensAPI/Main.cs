using System;
using System.Collections.Generic;
using System.Text;
using TokensAPI.identifers;

namespace TokensAPI
{
    public static class Main
    {
        public static Dictionary<string, Token> tokens
        {
            get => new Dictionary<string, Token>();
        }

        public static Token GetToken(string name)
        {
            try
            {
                return tokens[name];
            }
            catch (KeyNotFoundException)
            {
                return null; //it`s a pass
            }
        }

        public static Identifer GetIdentifer(string name)
        {
            return new SimpleIdentifer(name);
        }
    }
}
