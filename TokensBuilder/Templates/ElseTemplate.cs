using System;
using System.Collections.Generic;
using TokensAPI;

namespace TokensBuilder.Templates
{
    class ElseTemplate : TokensTemplate
    {
        TokensReader expr = null;
        IfTemplate ift = new IfTemplate();
        bool haveIf = false;

        public bool Parse(TokensReader expression, bool expression_end)
        {
            expr = null;
            haveIf = false;
            if (expression.tokens.Pop() == TokenType.ELSE)
            {
                TokensReader backup = new TokensReader();
                backup.Add(expression);
                try
                {
                    if (ift.Parse(expression, expression_end))
                    {
                        haveIf = true;
                        return true;
                    }
                }
                finally { }
                if (expression_end) expr = backup;
                return true;
            }
            else return false;
        }

        public List<TokensError> Run()
        {
            if (haveIf)
                return ift.Run();
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
