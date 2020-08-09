using System;
using System.Collections.Generic;
using TokensAPI;

namespace TokensBuilder.Templates
{
    class UseTemplate : TokensTemplate
    {
        public bool Parse(TokensReader expression, bool expression_end)
        {
            return expression_end && expression.tokens == new List<TokenType> { TokenType.USING_NAMESPACE };
        }

        public List<TokensError> Run(TokensReader expression)
        {
            TokensBuilder.gen.usingNamespaces.Add(expression.string_values.Peek());
            return new List<TokensError>();
        }
    }
}
