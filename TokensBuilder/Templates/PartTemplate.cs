using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using TokensAPI;
using TokensBuilder.Errors;

namespace TokensBuilder.Templates
{
    public static class PartTemplate
    {
        static uint line => TokensBuilder.gen.line;
        static byte lvlStatement = 0;
        public static List<TokensError> errors = new List<TokensError>();

        public static FieldInfo ParseField(ref TokensReader expression)
        {
            errors = new List<TokensError>();
            string varName = ParseLiterals(ref expression);
            if (varName != null)
            {
                FieldInfo field = Context.GetVarByName(varName);
                if (field == null)
                {
                    errors.Add(new VarNotFoundError(line, $"Field with name {varName} not found"));
                }
                return field;
            }
            else
                return null;
        }

        public static LocalBuilder ParseLocal(ref TokensReader expression)
        {
            if (expression.tokens.Pop() == TokenType.LITERAL)
                return Context.functionBuilder.GetLocal(expression.string_values.Pop());
            else 
                return null;
        }

        public static MethodInfo ParseCallMethod(ref TokensReader expression)
        {
            errors = new List<TokensError>();
            List<Type> paramTypes = new List<Type>();
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
                    return null;
                else
                {
                    if (parentName.Length == 0) parentName.Append(lastLiteral);
                    else parentName.Length--; // delete last character - '.'
                    string typename = parentName.ToString();
                    string methname = lastLiteral;
                    if (token == TokenType.STATEMENT && expression.bool_values.Pop()) // open arguments
                    {
                        token = expression.tokens.Pop();
                        if (token == TokenType.STATEMENT && !expression.bool_values.Pop()) // empty arguments
                        {
                            CallMethodTemplate callMethod = new CallMethodTemplate();
                            callMethod.methname = methname;
                            callMethod.paramTypes = paramTypes;
                            callMethod.typename = typename;
                            callMethod.dontPop = true;
                            callMethod.Run(expression);
                            return callMethod.method;
                        }
                        else
                        {
                            expression.tokens.Insert(0, token);
                            parse_param:
                            Type paramType = ParseValue(ref expression);
                            if (paramType == null)
                                return null;
                            else
                            {
                                paramTypes.Add(paramType);
                                token = expression.tokens.Pop();
                                if (token == TokenType.STATEMENT)
                                {
                                    if (!expression.bool_values.Pop())
                                    {
                                        CallMethodTemplate callMethod = new CallMethodTemplate();
                                        callMethod.methname = methname;
                                        callMethod.paramTypes = paramTypes;
                                        callMethod.typename = typename;
                                        callMethod.dontPop = true;
                                        callMethod.Run(expression);
                                        return callMethod.method;
                                    }
                                    else
                                        expression.bool_values.Insert(0, true);
                                }
                                else if (token == TokenType.SEPARATOR && !expression.bool_values.Pop())
                                    goto parse_param;
                                else
                                    return null;
                            }
                            return null;
                        }
                    }
                    else
                        return null;
                }
            }
            else
                return null;
        }

        public static string ParseLiterals(ref TokensReader expression)
        {
            TokenType token = expression.tokens.Pop();
            if (token == TokenType.LITERAL)
            {
                bool mustLiteral = true;
                StringBuilder literals = new StringBuilder();
                while (token == TokenType.LITERAL || token == TokenType.SEPARATOR)
                {
                    if (mustLiteral && token == TokenType.LITERAL)
                    {
                        literals.Append(expression.string_values.Pop());
                        mustLiteral = false;
                    }
                    else if (!mustLiteral && token == TokenType.SEPARATOR)
                    {
                        mustLiteral = true;
                        if (expression.bool_values.Pop())
                            literals.Append(".");
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
                    token = expression.tokens.Pop();
                }
                if (mustLiteral)
                {
                    errors.Add(new InvalidTokenError(line, "After separator must be literal"));
                }
                else
                {
                    expression.tokens.Insert(0, token);
                    return literals.ToString();
                }
            }
            else
            {
                expression.tokens.Insert(0, token);
            }
            return null;
        }

        public static Type ParseStatement(ref TokensReader expression)
        {
            TokenType token = expression.tokens.Pop();
            if (token == TokenType.STATEMENT)
            {
                if (expression.bool_values.Pop())
                {
                    return ParseSmallStatement(ref expression);
                }
                else
                {
                    errors.Add(new InvalidTokenError(line, $"There are wrong 'closing statement' token"));
                }
            }
            else
            {
                expression.tokens.Insert(0, token);
                errors.Add(new InvalidTokenError(line, $"Statement cannot start with token {token}"));
            }
            return null;
        }

        public static Type ParseSmallStatement(ref TokensReader expression)
        {
            int thisStatement = lvlStatement;
            lvlStatement++;
            List<Type> valuetypes = new List<Type>();
            while (thisStatement < lvlStatement)
            {
                valuetypes.Add(ParseValue(ref expression));
            }
            valuetypes.RemoveLast(); // remove type which was getted of close statement
            if (valuetypes.IsEmpty())
                return typeof(void);
            else if (valuetypes.Count == 1)
                return valuetypes[0];
            else
                return valuetypes[valuetypes.Count / 2];
        }

        public static Type ParseValue(ref TokensReader expression)
        {
            Type type = null;
            TokenType token = expression.tokens.Pop();
            if (token == TokenType.VALUE)
            {
                byte valtype = expression.byte_values.Pop();
                if (valtype == 0) type = typeof(object);
                else if (valtype == 1) type = typeof(int);
                else if (valtype == 2) type = typeof(string);
                else if (valtype == 3) type = typeof(sbyte);
                else if (valtype == 4) type = typeof(bool);
                else if (valtype == 5) type = typeof(char);
                else if (valtype == 6) type = typeof(float);
                else if (valtype == 7) type = typeof(short);
                else if (valtype == 8) type = typeof(long);
                else if (valtype == 9) type = typeof(double);
                LaterCalls.LoadObject(expression.values.Pop());
            }
            else if (token == TokenType.LITERAL) // is method
            {
                expression.tokens.Insert(0, TokenType.LITERAL);
                TokensReader backup = new TokensReader();
                backup.Add(expression);
                MethodInfo method = ParseCallMethod(ref expression);
                if (method == null)
                {
                    expression = backup;
                    LocalBuilder local = ParseLocal(ref expression);
                    if (local != null)
                    {
                        LaterCalls.LoadLocal(local);
                        type = local.LocalType;
                    }
                    else // is field
                    {
                        expression = backup;
                        FieldInfo value = ParseField(ref expression);
                        LaterCalls.LoadField(value);
                        type = value.FieldType;
                    }
                }
                else
                    type = method.ReturnType;
            }
            else if (token == TokenType.NEW)
            {
                string ctorName = ParseLiterals(ref expression);
                if (ctorName != null)
                {
                    type = Context.GetTypeByName(ctorName);
                    token = expression.tokens.Pop();
                    if (token == TokenType.STATEMENT)
                    {
                        if (expression.bool_values.Pop())
                        {
                            token = expression.tokens.Pop();
                            if (token == TokenType.STATEMENT && !expression.bool_values.Pop()) // empty arguments
                            {
                                LaterCalls.NewObject(type.GetConstructor(Type.EmptyTypes));
                                return type;
                            }
                            else
                            {
                                expression.tokens.Insert(0, token);
                                List<Type> paramTypes = new List<Type>();
                                parse_param:
                                Type paramType = ParseValue(ref expression);
                                if (paramType == null)
                                    return null;
                                else
                                {
                                    paramTypes.Add(paramType);
                                    token = expression.tokens.Pop();
                                    if (token == TokenType.STATEMENT)
                                    {
                                        if (!expression.bool_values.Pop())
                                        {
                                            LaterCalls.NewObject(type.GetConstructor(paramTypes.ToArray()));
                                            return type;
                                        }
                                        else
                                            expression.bool_values.Insert(0, true);
                                    }
                                    else if (token == TokenType.SEPARATOR && !expression.bool_values.Pop())
                                        goto parse_param;
                                    else
                                        return null;
                                }
                                return null;
                            }
                        }
                        else
                            errors.Add(new InvalidTokenError(
                                line, $"After constructor name must stay open statement (not close)"));
                    }
                    else
                        errors.Add(new InvalidTokenError(
                            line, $"After constructor name must stay open statement (not {token})"));
                }
            }
            else if (token == TokenType.STATEMENT)
            {
                if (expression.bool_values.Pop())
                {
                    type = ParseSmallStatement(ref expression);
                }
                else
                {
                    if (lvlStatement == 0)
                        errors.Add(new InvalidTokenError(line, $"There are wrong 'closing statement' token"));
                    else
                        lvlStatement--;
                }
            }
            if (!expression.tokens.IsEmpty() && expression.tokens[0] == TokenType.OPERATOR)
            {
                expression.tokens.RemoveAt(0);
                OperatorType op = expression.operators.Pop();
                if (type != null)
                {
                    if (type == ParseValue(ref expression))
                        LaterCalls.LoadOperator(type, op);
                    else
                        errors.Add(new InvalidTypeError(
                            line, $"Type {type} before operator {op} not equals type of value after operator"));
                }
                switch (op)
                {
                    case OperatorType.EQ:
                    case OperatorType.NOTEQ:
                    case OperatorType.NOT:
                    case OperatorType.AND:
                    case OperatorType.OR:
                    case OperatorType.GT:
                    case OperatorType.LT:
                    case OperatorType.GORE:
                    case OperatorType.LORE:
                    case OperatorType.IN:
                        type = typeof(bool);
                        break;
                    case OperatorType.GTQ:
                        break;
                    case OperatorType.LTQ:
                        break;
                    case OperatorType.CONVERTTO:
                        break;
                    case OperatorType.RANGE:
                        break;
                    case OperatorType.POW:
                        type = typeof(float);
                        break;
                }
            }
            TokensBuilder.gen.errors.AddRange(errors);
            errors.Clear();
            return type;
        }
    }
}
