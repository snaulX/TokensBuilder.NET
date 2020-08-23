using System.Collections.Generic;
using TokensAPI;

namespace TokensBuilder.Templates
{
    class NamespaceTemplate : TokensTemplate
    {
        public string ns = "";
        public bool Parse(TokensReader expression, bool expression_end)
        {
            if (expression_end && expression.tokens[0] == TokenType.NAMESPACE
                && expression.tokens.Count == 1)
            {
                ns = expression.string_values.Pop();
                return true;
            }
            else return false;
        }

        public List<TokensError> Run()
        {
            TokensBuilder.gen.currentNamespace = ns;
            return new List<TokensError>();
        }
    }
}
