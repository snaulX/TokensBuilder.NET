using System;
using System.Collections.Generic;
using TokensAPI;

namespace TokensBuilder.Templates
{
    class LibTemplate : TokensTemplate
    {
        public TokensReader library = new TokensReader();

        public bool Parse(TokensReader expression, bool expression_end)
        {
            return expression_end && expression.tokens == new List<TokenType> { TokenType.IMPORT_LIBRARY };
        }

        public List<TokensError> Run(TokensReader expression)
        {
            List<TokensError> errors = new List<TokensError>();
            string path = expression.string_values.Peek();
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
