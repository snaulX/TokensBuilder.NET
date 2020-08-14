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
            if (expression.tokens.Peek() == TokenType.LITERAL)
            {
                TokensReader backup = new TokensReader();
                backup.Add(expression);
                field = PartTemplate.ParseVar(ref expression);
                if (field == null) // is local
                {
                    expression = backup;
                    local = PartTemplate.ParseLocal(ref expression);
                }

                if (expression.tokens.Peek() == TokenType.OPERATOR && expression.operators.Peek() == OperatorType.ASSIGN)
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

        public List<TokensError> Run(TokensReader expression)
        {
            List<TokensError> errors = new List<TokensError>();
            if (local == null) // is field
            {
                if (field.FieldType == valuetype)
                    Context.SetField(field);
                else
                    errors.Add(new InvalidTypeError(TokensBuilder.gen.line,
                        $"Type of value {valuetype} not equals type of getted field {field.FieldType} for assign"));
            }
            else // is local
            {
                if (local.LocalType == valuetype)
                {
                    Context.SetLocal(local);
                }
                else
                    errors.Add(new InvalidTypeError(TokensBuilder.gen.line,
                        $"Type of value {valuetype} not equals type of getted local {local.LocalType} for assign"));
            }
            return errors;
        }
    }
}
