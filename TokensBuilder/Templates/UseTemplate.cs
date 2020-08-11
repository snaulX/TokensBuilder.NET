using System.Collections.Generic;
using TokensAPI;

namespace TokensBuilder.Templates
{
    class UseTemplate : TokensTemplate
    {
        public bool Parse(TokensReader expression, bool expression_end)
        {
            return expression_end && expression.tokens[0] == TokenType.USING_NAMESPACE
                && expression.tokens.Count == 1;
        }

        public List<TokensError> Run(TokensReader expression)
        {
            TokensBuilder.gen.usingNamespaces.Add(expression.string_values.Peek());
            return new List<TokensError>();
        }
    }
}
