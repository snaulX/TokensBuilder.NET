using System;
using System.Collections.Generic;
using TokensAPI;

namespace TokensBuilder.Templates
{
    class LibTemplate : TokensTemplate
    {
        public TokensReader library = new TokensReader();
        public string path = "";

        public bool Parse(TokensReader expression, bool expression_end)
        {
            if (expression_end && expression.tokens[0] == TokenType.IMPORT_LIBRARY
                && expression.tokens.Count == 1)
            {
                path = expression.string_values.Pop();
                return true;
            }
            else return false;
        }

        public List<TokensError> Run()
        {
            List<TokensError> errors = new List<TokensError>();
            try
            {
                if (path.StartsWith("<")) library.SetPath(path.Remove(path.Length - 2) + ".tokens");
                else library.SetPath(AppDomain.CurrentDomain.BaseDirectory + "lib/" + path + ".tokens");
            }
            catch
            {
                errors.Add(new Errors.TokensLibraryError(
                    TokensBuilder.gen.line, $"Tokens library by path {path} not found"));
                return errors;
            }
            library.GetHeaderAndTarget(out _, out _);
            library.ReadTokens();
            library.EndWork();
            return errors;
        }
    }
}
