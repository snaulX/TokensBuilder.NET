using System.Collections.Generic;
using System.Reflection.Emit;
using TokensAPI;

namespace TokensBuilder.Templates
{
    class BreakpointTemplate : TokensTemplate
    {
        public bool Parse(TokensReader expression, bool expression_end)
        {
            return expression_end && expression.tokens[0] == TokenType.BREAKPOINT
                && expression.tokens.Count == 1;
        }

        public List<TokensError> Run(TokensReader expression)
        {
            Context.functionBuilder.generator.Emit(OpCodes.Break);
            return new List<TokensError>();
        }
    }
}
