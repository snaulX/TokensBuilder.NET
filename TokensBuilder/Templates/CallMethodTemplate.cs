using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
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
        public MethodInfo method = null;

        public bool Parse(TokensReader expression, bool expression_end)
        {
            if (!expression_end) 
                return false;

            LaterCalls.Seek();
            paramTypes = new List<Type>();
            methname = "";
            typename = "";
            TokenType token = expression.tokens.Pop();
            if (token == TokenType.LITERAL)
            {
                bool mustLiteral = true;
                StringBuilder parentName = new StringBuilder();
                string lastLiteral = "";
                while (token == TokenType.LITERAL || token == TokenType.SEPARATOR)
                {
                    if (mustLiteral && token == TokenType.LITERAL)
                    {
                        lastLiteral = expression.string_values.Pop();
                        mustLiteral = false;
                    }
                    else if (!mustLiteral && token == TokenType.SEPARATOR)
                    {
                        mustLiteral = true;
                        if (expression.bool_values.Pop())
                            parentName.Append(lastLiteral + ".");
                        else
                            break;
                    }
                    else
                        break;
                    token = expression.tokens.Pop();
                }
                if (mustLiteral)
                    return false;
                else
                {
                    if (parentName.Length == 0) parentName.Append(lastLiteral);
                    else parentName.Length--; // delete last character - '.'
                    typename = parentName.ToString();
                    methname = lastLiteral;
                    if (token == TokenType.STATEMENT && expression.bool_values.Pop())
                    {
                        if (expression.tokens[0] == TokenType.STATEMENT && !expression.bool_values[0])
                        {
                            if (expression.tokens.Count == 1)
                                return true;
                            else
                                return false;
                        }
                        else
                        {
                            parse_param:
                            Type paramType = PartTemplate.ParseValue(ref expression);
                            if (paramType == null)
                                return false;
                            else
                            {
                                paramTypes.Add(paramType);
                                token = expression.tokens.Pop();
                                if (token == TokenType.STATEMENT)
                                {
                                    if (!expression.bool_values.Pop())
                                    {
                                        if (expression.tokens.Count == 0)
                                            return true;
                                        else
                                            return false;
                                    }
                                    else
                                        expression.bool_values.Insert(0, true);
                                }
                                else if (token == TokenType.SEPARATOR && !expression.bool_values.Pop())
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

        public List<TokensError> Run()
        {
            List<TokensError> errors = new List<TokensError>();
            try
            {
                Type callerType = Context.GetTypeByName(typename);
                if (callerType == null)
                {
                    LocalBuilder local = Context.functionBuilder.GetLocal(typename);
                    if (local == null)
                        errors.Add(
                            new TypeNotFoundError(
                                line, $"Before calling method. Type or local with name {typename} not found"));
                    else
                    {
                        LaterCalls.LoadLocal(local, true);
                        method = local.LocalType.GetMethod(methname, paramTypes.ToArray());
                        if (method == null)
                            errors.Add(new InvalidMethodError(line, $"Method with name {typename + methname} not found"));
                        else if (method.IsStatic)
                            errors.Add(new InvalidMethodError(
                                line, $"Local variable {typename} cannot call static method {methname}"));
                        else
                            LaterCalls.CallMethod(method, dontPop);
                    }
                }
                else
                {
                    method = callerType.GetMethod(methname, paramTypes.ToArray());
                    if (method == null)
                        errors.Add(new InvalidMethodError(line, $"Method with name {typename + methname} not found"));
                    else if (!method.IsStatic)
                        errors.Add(new InvalidMethodError(
                            line, $"Type {typename} cannot call not static method {methname}"));
                    else
                        LaterCalls.CallMethod(method, dontPop);
                }
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
