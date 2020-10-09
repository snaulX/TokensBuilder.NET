using System;
using System.Collections.Generic;
using System.Reflection.Emit;
using TokensAPI;

namespace TokensBuilder.Templates
{
    public class WhileTemplate : TokensTemplate
    {
        Type statement;
        TokensReader body = null;
        ILGenerator g => Context.functionBuilder.generator;

        public bool Parse(TokensReader expression, bool expression_end)
        {
            body = null;
            TokenType token = expression.tokens.Pop();
            LoopType loopType = expression.loops.Pop();
            if (token == TokenType.LOOP && loopType == LoopType.WHILE)
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
            Label endBlock = g.DefineLabel();
            LaterCalls.StartLoop(); // initilizate start of loop
            LaterCalls.Brfalse(endBlock); // make statement that points to end of loop |
            TokensBuilder.gen.ParseExpression(body); // parse body                     |
            LaterCalls.BrEndIf(); // make break to start of loop and mark end of loop <-
            LaterCalls.Call(); // call all that we write here
            LaterCalls.isLoop = false; // mark that we heven`t loop now
            return errors;
        }
    }
}
