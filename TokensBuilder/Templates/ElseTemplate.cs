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
            if (expression.tokens.Pop() == TokenType.ELSE && expression.tokens.IsEmpty())
            {
                if (expression_end) expr = expression;
                return true;
            }
            else return false;
        }

        public List<TokensError> Run()
        {
            List<TokensError> errors = new List<TokensError>();
            LaterCalls.brEndIf = true;
            LaterCalls.CreateEndIfLabel();
            TokensBuilder.gen.ParseExpression(expr);
            Context.functionBuilder.generator.MarkLabel(LaterCalls.endIfLabels.Pop());
            LaterCalls.Call();
            return errors;
        }
    }
}
