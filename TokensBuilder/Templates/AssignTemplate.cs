using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using TokensAPI;
using TokensBuilder.Errors;

namespace TokensBuilder.Templates
{
    class AssignTemplate : TokensTemplate
    {
        public FieldInfo field;
        public LocalBuilder local;
        public Type valuetype;

        public bool Parse(TokensReader expression, bool expression_end)
        {
            field = null;
            local = null;
            valuetype = null;
            if (expression.tokens.Pop() == TokenType.LITERAL && expression_end)
            {
                expression.tokens.Insert(0, TokenType.LITERAL);
                TokensReader backup = new TokensReader();
                backup.Add(expression);
                local = PartTemplate.ParseLocal(ref expression);
                if (local == null) // is field
                {
                    expression = backup;
                    field = PartTemplate.ParseField(ref expression);
                }

                if (expression.tokens.Pop() == TokenType.OPERATOR && expression.operators.Pop() == OperatorType.ASSIGN)
                {
                    valuetype = PartTemplate.ParseValue(ref expression);
                    if (expression.tokens.Count == 0)
                        return true;
                    else
                        return false;
                }
                else
                    return false;
            }
            else
                return false;
        }

        public List<TokensError> Run()
        {
            List<TokensError> errors = new List<TokensError>();
            if (local == null) // is field
            {
                if (field.FieldType.IsAssignableFrom(valuetype))
                    LaterCalls.SetField(field);
                else
                    errors.Add(new InvalidTypeError(TokensBuilder.gen.line,
                        $"Type of value {valuetype} not equals type of getted field {field.FieldType} for assign"));
            }
            else // is local
            {
                if (local.LocalType.IsAssignableFrom(valuetype))
                    LaterCalls.SetLocal(local);
                else
                    errors.Add(new InvalidTypeError(TokensBuilder.gen.line,
                        $"Type of value {valuetype} not equals type of getted local {local.LocalType} for assign"));
            }
            return errors;
        }
    }
}
