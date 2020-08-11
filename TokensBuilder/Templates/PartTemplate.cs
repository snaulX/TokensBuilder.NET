using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using TokensAPI;
using TokensBuilder.Errors;

namespace TokensBuilder.Templates
{
    public static class PartTemplate
    {
        static uint line => TokensBuilder.gen.line;

        public static FieldInfo ParseVar(ref TokensReader expression, out List<TokensError> errors)
        {
            errors = new List<TokensError>();
            TokenType token = expression.tokens.Peek();
            if (token == TokenType.LITERAL)
            {
                bool mustLiteral = true;
                StringBuilder varName = new StringBuilder();
                while (token == TokenType.LITERAL || token == TokenType.SEPARATOR)
                {
                    if (mustLiteral && token == TokenType.LITERAL)
                    {
                        varName.Append(expression.string_values.Peek());
                    }
                    else if (!mustLiteral && token == TokenType.SEPARATOR)
                    {
                        if (expression.bool_values.Peek())
                            varName.Append(".");
                        else
                        {
                            expression.bool_values.Insert(0, false);
                            break;
                        }
                    }
                    else
                    {
                        break;
                    }
                    token = expression.tokens.Peek();
                }
                if (mustLiteral)
                {
                    errors.Add(new InvalidTokenError(line, "After separator must be literal"));
                    return null;
                }
                else
                {
                    FieldInfo field = Context.GetVarByName(varName.ToString());
                    if (field == null)
                    {
                        errors.Add(new VarNotFoundError(line, $"Field with name {varName} not found"));
                    }
                    return field;
                }
            }
            else
            {
                expression.tokens.Insert(0, token);
            }
            return null;
        }
    }
}
