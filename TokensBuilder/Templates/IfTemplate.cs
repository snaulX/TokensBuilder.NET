using System;
using System.Collections.Generic;
using TokensAPI;

namespace TokensBuilder.Templates
{
    public class IfTemplate : TokensTemplate
    {
        public bool Parse(TokensReader expression, bool expression_end)
        {
            TokenType token = expression.tokens.Pop();
            if (token == TokenType.IF)
            {
                PartTemplate.ParseStatement(ref expression);
                return true;
            }
            else
                return false;
        }

        public List<TokensError> Run(TokensReader expression)
        {
            throw new NotImplementedException();
        }
    }
}
