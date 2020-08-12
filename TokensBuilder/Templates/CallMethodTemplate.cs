using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using TokensAPI;
using TokensBuilder.Errors;

namespace TokensBuilder.Templates
{
    class CallMethodTemplate : TokensTemplate
    {
        private uint line => TokensBuilder.gen.line;
        public bool dontPop = false;
        public string typename = "", methname = "";
        public List<Type> paramTypes = new List<Type>();
        public List<object> parameters = new List<object>();
        public MethodInfo method = null;

        public bool Parse(TokensReader expression, bool expression_end)
        {
            if (!expression_end) 
                return false;

            parameters = new List<object>();
            paramTypes = new List<Type>();
            methname = "";
            typename = "";
            TokenType token = expression.tokens.Peek();
            if (token == TokenType.LITERAL)
            {
                bool mustLiteral = true;
                StringBuilder parentName = new StringBuilder();
                string lastLiteral = "";
                while (token == TokenType.LITERAL || token == TokenType.SEPARATOR)
                {
                    if (mustLiteral && token == TokenType.LITERAL)
                    {
                        lastLiteral = expression.string_values.Peek();
                        mustLiteral = false;
                    }
                    else if (!mustLiteral && token == TokenType.SEPARATOR)
                    {
                        mustLiteral = true;
                        if (expression.bool_values.Peek())
                            parentName.Append(lastLiteral + ".");
                        else
                            break;
                    }
                    else
                        break;
                    token = expression.tokens.Peek();
                }
                if (mustLiteral)
                    return false;
                else
                {
                    parentName.Length--; // delete last character - '.'
                    typename = parentName.ToString();
                    methname = lastLiteral;
                    if (token == TokenType.STATEMENT && expression.bool_values.Peek())
                    {
                        token = expression.tokens.Peek();
                        if (token == TokenType.STATEMENT && !expression.bool_values.Peek())
                            return true;
                        else
                        {
                            parse_param:
                            expression.tokens.Insert(0, token);
                            Type paramType = PartTemplate.ParseValue(ref expression, out object val);
                            if (paramType == null)
                                return false;
                            else
                            {
                                paramTypes.Add(paramType);
                                parameters.Add(val);
                                token = expression.tokens.Peek();
                                if (token == TokenType.STATEMENT)
                                {
                                    if (!expression.bool_values.Peek())
                                        return true;
                                    else
                                        expression.bool_values.Insert(0, true);
                                }
                                else if (token == TokenType.SEPARATOR && expression.bool_values.Peek())
                                    goto parse_param;
                                else
                                    return false;
                            }
                            return false;
                        }
                    }
                    else
                        return false;
                }
            }
            else 
                return false;
        }

        public List<TokensError> Run(TokensReader expression)
        {
            List<TokensError> errors = new List<TokensError>();
            try
            {
                method = Context.GetTypeByName(typename).GetMethod(methname, paramTypes.ToArray());
                if (method == null)
                    errors.Add(new InvalidMethodError(line, $"Method with name {typename + methname} not found"));
                else
                {
                    Context.CallMethod(method, parameters, dontPop);
                }
            }
            catch (NullReferenceException)
            {
                errors.Add(
                    new TypeNotFoundError(line, $"Before calling method. Type with name {typename} not found"));
            }
            catch (AmbiguousMatchException)
            {
                errors.Add(new InvalidMethodError(line,
                    $"Method with name {typename+methname} havent parameters with types: {string.Join(", ", paramTypes)}"));
            }
            return errors;
        }
    }
}
