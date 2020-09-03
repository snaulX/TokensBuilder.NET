using System;
using System.Collections.Generic;
using TokensAPI;

namespace TokensBuilder.Templates
{
    class ElseTemplate : TokensTemplate
    {
        TokensReader expr = null;

        public bool Parse(TokensReader expression, bool expression_end)
        {
            expr = null;
            if (expression.tokens.Pop() == TokenType.ELSE)
            {
                if (expression_end) expr = expression;
                return true;
            }
            else return false;
        }

        public List<TokensError> Run()
        {
            List<TokensError> errors = new List<TokensError>();
            TokensBuilder.gen.needLaterCall = false;
            TokensBuilder.gen.ParseExpression(expr);
            LaterCalls.CreateEndIfLabel();
            LaterCalls.Call();
            LaterCalls.brEndIf = false;
            return errors;
        }
    }
}
