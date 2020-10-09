using System;
using System.Collections.Generic;
using System.Reflection.Emit;
using TokensAPI;

namespace TokensBuilder.Templates
{
    public class IfTemplate : TokensTemplate
    {
        Type statement;
        TokensReader body = null;

        public bool Parse(TokensReader expression, bool expression_end)
        {
            body = null;
            TokenType token = expression.tokens.Pop();
            if (token == TokenType.IF)
            {
                statement = PartTemplate.ParseStatement(ref expression);
                if (expression_end)
                {
                    body = expression;
                }
                return true;
            }
            else
                return false;
        }

        public List<TokensError> Run()
        {
            List<TokensError> errors = new List<TokensError>();
            TokensBuilder.gen.needLaterCall = false;
            Label endBlock = Context.functionBuilder.generator.DefineLabel();
            LaterCalls.Brfalse(endBlock); // create statement
            TokensBuilder.gen.ParseExpression(body); // parse body
            LaterCalls.BrEndIf(); // break to end of all if-else
            TokensBuilder.gen.needLaterCall = false;
            return errors;
        }
    }
}
