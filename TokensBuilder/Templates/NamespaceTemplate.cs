using System.Collections.Generic;
using TokensAPI;

namespace TokensBuilder.Templates
{
    class NamespaceTemplate : TokensTemplate
    {
        public bool Parse(TokensReader expression, bool expression_end)
        {
            return expression_end && expression.tokens == new List<TokenType> { TokenType.NAMESPACE };
        }

        public List<TokensError> Run(TokensReader expression)
        {
            TokensBuilder.gen.currentNamespace = expression.string_values.Peek();
            return new List<TokensError>();
        }
    }
}
