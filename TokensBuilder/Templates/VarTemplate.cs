using System;
using System.Collections.Generic;
using System.Reflection;
using TokensAPI;

namespace TokensBuilder.Templates
{
    class VarTemplate : TokensTemplate
    {
        private uint line => TokensBuilder.gen.line;
        public string typename = "";
        public List<string> varnames = new List<string>();
        public VarType type;
        public SecurityDegree security;

        public bool Parse(TokensReader expression, bool expression_end)
        {
            typename = "";
            varnames = new List<string>();
            type = VarType.DEFAULT;
            security = SecurityDegree.PUBLIC;
            if (expression_end && expression.tokens.Pop() == TokenType.VAR)
            {
                type = expression.var_types.Pop();
                security = expression.securities.Pop();
                if (expression.tokens.Pop() == TokenType.LITERAL)
                {
                    typename = expression.string_values.Pop();
                    parse_var:
                    if (expression.tokens.Pop() == TokenType.LITERAL)
                    {
                        varnames.Add(expression.string_values.Pop());
                        TokenType token;
                        try
                        {
                            token = expression.tokens.Pop();
                        }
                        catch (ArgumentOutOfRangeException) // if tokens is ended
                        {
                            return true;
                        }
                        if (token == TokenType.SEPARATOR)
                        {
                            if (expression.bool_values.Pop())
                                return false;
                            else
                                goto parse_var;
                        }
                        else
                        {
                            return false;
                        }
                    }
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
            if (Context.isFuncBody)
            {
                foreach (string varname in varnames)
                    Context.functionBuilder.localVariables.Add(
                        varname, Context.functionBuilder.DeclareLocal(typename));
            }
            else
            {
                foreach (string varname in varnames)
                {
                    FieldAttributes fieldAttributes;
                    if (security == SecurityDegree.PUBLIC)
                        fieldAttributes = FieldAttributes.Public;
                    else if (security == SecurityDegree.PRIVATE)
                        fieldAttributes = FieldAttributes.Private;
                    else if (security == SecurityDegree.PROTECTED)
                        fieldAttributes = FieldAttributes.Family;
                    else // if security == INTERNAL
                        fieldAttributes = FieldAttributes.Assembly;
                    if (type == VarType.STATIC)
                        fieldAttributes |= FieldAttributes.Static;
                    else if (type == VarType.CONST)
                        fieldAttributes |= FieldAttributes.Literal;
                    Context.classBuilder.DefineField(varname, typename, fieldAttributes);
                    if (type == VarType.FINAL)
                        Context.classBuilder.finalFields.Add(varname, Context.classBuilder.fieldBuilder);
                }
            }
            return errors;
        }
    }
}
