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
            Label block = Context.functionBuilder.generator.DefineLabel();
            LaterCalls.Call();
            Context.functionBuilder.generator.Emit(OpCodes.Brfalse, block);
            TokensBuilder.gen.ParseExpression(expr);
            LaterCalls.BrEndIf();
            Context.functionBuilder.generator.MarkLabel(block);
            return errors;
        }
    }
}
