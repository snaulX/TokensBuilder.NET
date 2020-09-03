using System;
using System.Collections.Generic;
using System.Reflection.Emit;
using TokensAPI;

namespace TokensBuilder.Templates
{
    public class IfTemplate : TokensTemplate
    {
        Type statement;
        TokensReader expr = null;
        private ILGenerator g => Context.functionBuilder.generator;

        public bool Parse(TokensReader expression, bool expression_end)
        {
            expr = null;
            TokenType token = expression.tokens.Pop();
            if (token == TokenType.IF)
            {
                statement = PartTemplate.ParseStatement(ref expression);
                if (expression_end)
                {
                    expr = expression;
                }
                return true;
            }
            else
                return false;
        }

        public List<TokensError> Run()
        {
            List<TokensError> errors = new List<TokensError>();
            Label endBlock = Context.functionBuilder.generator.DefineLabel();
            LaterCalls.Call(); // call statement
            g.Emit(OpCodes.Brfalse, endBlock);
            TokensBuilder.gen.ParseExpression(expr);
            LaterCalls.BrEndIf(endBlock);
            TokensBuilder.gen.needLaterCall = false;
            return errors;
        }
    }
}
