using System;
using System.Collections.Generic;
using System.Reflection.Emit;
using TokensAPI;

namespace TokensBuilder.Templates
{
    public class IfTemplate : TokensTemplate
    {
        Type statement;

        public bool Parse(TokensReader expression, bool expression_end)
        {
            TokenType token = expression.tokens.Pop();
            if (token == TokenType.IF)
            {
                statement = PartTemplate.ParseStatement(ref expression);
                return true;
            }
            else
                return false;
        }

        public List<TokensError> Run()
        {
            List<TokensError> errors = new List<TokensError>();
            Label block = Context.functionBuilder.generator.DefineLabel();

            return errors;
        }
    }
}
