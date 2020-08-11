using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TokensAPI;

namespace TokensBuilder.Templates
{
    class CallMethodTemplate : TokensTemplate
    {
        string methname = "";
        List<Type> paramTypes = new List<Type>();
        List<object> parameters = new List<object>();

        public bool Parse(TokensReader expression, bool expression_end)
        {
            parameters = new List<object>();
            paramTypes = new List<Type>();
            methname = "";
            TokenType token = expression.tokens.Peek();
            if (token == TokenType.LITERAL)
            {
                bool mustLiteral = true;
                StringBuilder callMethName = new StringBuilder();
                while (token == TokenType.LITERAL || token == TokenType.SEPARATOR)
                {
                    if (mustLiteral && token == TokenType.LITERAL)
                        callMethName.Append(expression.string_values.Peek());
                    else if (!mustLiteral && token == TokenType.SEPARATOR)
                    {
                        if (expression.bool_values.Peek())
                            callMethName.Append(".");
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
                    methname = callMethName.ToString();
                    if (token == TokenType.STATEMENT && expression.bool_values.Peek())
                    {
                        token = expression.tokens.Peek();
                        if (token == TokenType.STATEMENT && !expression.bool_values.Peek())
                            return true;
                        else
                        {
                            parse_param:
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
            throw new NotImplementedException();
        }
    }
}
