using System;
using System.Collections.Generic;
using System.Linq;
using TokensAPI;
using TokensBuilder.Errors;
using TokensBuilder.Templates;

namespace TokensBuilder
{
    public sealed class Generator
    {
        #region Public fields
        public uint line = 0;
        public TokensReader reader, expression;
        public string currentNamespace = "";
        public List<string> usingNamespaces = new List<string>();
        public Dictionary<string, Action> directives = new Dictionary<string, Action>();
        public byte needEndBlock = 0;
        public List<TokensError> errors = new List<TokensError>();
        public bool tryDirective = false;
        public List<TokenType> tryTokens = new List<TokenType>();
        public Stack<int> tryPositions = new Stack<int>(), tryCounts = new Stack<int>(); // positions of 'try' tokens
        #endregion

        #region Flags
        private int count = 0;
        private Dictionary<TokenType, TokensTemplate> strongTemplates = new Dictionary<TokenType, TokensTemplate>();
        private List<TokensTemplate> flexTemplates = new List<TokensTemplate>();
        #endregion

        #region Properties
        private bool isFuncBody => Context.functionBuilder != null && !Context.functionBuilder.IsEmpty;
        #endregion

        public Generator()
        {
            directives.Add("extends", () =>
            {
                TokenType curToken = reader.tokens.Pop();
                if (curToken == TokenType.LITERAL)
                {
                    if (Config.header == HeaderType.CLASS)
                        Context.mainClass.Extends(reader.string_values.Pop());
                    else
                        errors.Add(new InvalidHeaderError(line, Config.header, "extends directive can be only with class header"));
                }
                else
                {
                    errors.Add(new InvalidTokenError(line, curToken));
                }
            });
            directives.Add("implements", () =>
            {
                TokenType tokenType = TokenType.LITERAL;
                while (tokenType != TokenType.NEWLN)
                {
                    if (Config.header == HeaderType.CLASS)
                        Context.mainClass.Implements(reader.string_values.Pop());
                    else
                        errors.Add(new InvalidHeaderError(line, Config.header, "implements directive can be only with class header"));
                }
            });
            directives.Add("if", () =>
            {
                //something
            });
            directives.Add("endif", () =>
            {
                //something
            });
            directives.Add("outtype", () =>
            {
                string outType = reader.string_values.Pop();
                if (!Enum.TryParse(outType, out Config.outputType))
                {
                    errors.Add(new InvalidOutTypeError(line, outType));
                }
            });
            directives.Add("try", () =>
            {
                tryDirective = true;
            });
            directives.Add("endtry", () =>
            {
                tryDirective = false;
                tryPositions.Push(expression.tokens.Count);
                tryCounts.Push(tryTokens.Count - tryCounts.Sum());
                expression.tokens.Add(tryTokens.Last());
            });
            strongTemplates.Add(TokenType.INCLUDE, new IncludeTemplate());
            strongTemplates.Add(TokenType.USING_NAMESPACE, new UseTemplate());
            strongTemplates.Add(TokenType.IMPORT_LIBRARY, new LibTemplate());
            strongTemplates.Add(TokenType.NAMESPACE, new NamespaceTemplate());
            strongTemplates.Add(TokenType.BREAKPOINT, new BreakpointTemplate());
            strongTemplates.Add(TokenType.VAR, new VarTemplate());
            strongTemplates.Add(TokenType.IF, new IfTemplate());
            flexTemplates.Add(new CallMethodTemplate());
            flexTemplates.Add(new AssignTemplate());
            reader = new TokensReader();
        }

        public void Generate()
        {
            reader.GetHeaderAndTarget(out byte h, out Config.platform);
            Config.header = (HeaderType)h;
            reader.ReadTokens();
            reader.EndWork();
            while (reader.tokens.Count > 0)
            {
                ParseExpression();
            }
            foreach (TokensError error in errors)
                Console.Error.WriteLine(error);
        }

        /// <summary>
        /// Get expression and parse it
        /// </summary>
        public void ParseExpression()
        {
            expression = new TokensReader();
            bool exprend = true;
            int pos = 0;
            while (pos < reader.tokens.Count)
            {
                pos++;
                TokenType token;
                try
                {
                    token = reader.tokens[pos];
                }
                catch (ArgumentOutOfRangeException)
                {
                    break;
                }
                if (tryDirective)
                {
                    if (token != TokenType.DIRECTIVE)
                    {
                        tryTokens.Add(token);
                    }
                }
                if (token == TokenType.EXPRESSION_END || token == TokenType.BLOCK)
                {
                    if (token == TokenType.EXPRESSION_END) exprend = true;
                    else
                    {
                        if (reader.bool_values.Pop())
                        {
                            exprend = false;
                            needEndBlock++;
                        }
                        else
                        {
                            if (expression.tokens.IsEmpty()) needEndBlock--;
                            else errors.Add(new NeedEndError(line, "Doesnt close expression before closing block"));
                        }
                    }
                    break;
                }
                else if (token == TokenType.NEWLN) line++;
                else if (token == TokenType.DIRECTIVE)
                {
                    pos = ParseDirective(++pos);
                }
                else
                {
                    if (!tryDirective) expression.tokens.Add(token);
                    if (token == TokenType.CLASS)
                    {
                        expression.string_values.Add(reader.string_values.Pop());
                        expression.class_types.Add(reader.class_types.Pop());
                        expression.securities.Add(reader.securities.Pop());
                    }
                    else if (token == TokenType.FUNCTION)
                    {
                        expression.string_values.Add(reader.string_values.Pop());
                        expression.string_values.Add(reader.string_values.Pop());
                        expression.function_types.Add(reader.function_types.Pop());
                        expression.securities.Add(reader.securities.Pop());
                    }
                    else if (token == TokenType.VAR)
                    {
                        expression.var_types.Add(reader.var_types.Pop());
                        expression.securities.Add(reader.securities.Pop());
                    }
                    else if (token == TokenType.STATEMENT || token == TokenType.SEQUENCE || token == TokenType.SEPARATOR
                        || token == TokenType.RETURN || token == TokenType.LAMBDA || token == TokenType.ASYNC
                        || token == TokenType.PARAMETER_TYPE || token == TokenType.GENERIC || token == TokenType.ACTUAL)
                    {
                        expression.bool_values.Add(reader.bool_values.Pop());
                    }
                    else if (token == TokenType.LITERAL || token == TokenType.TYPEOF || token == TokenType.NAMESPACE
                        || token == TokenType.IMPORT_LIBRARY || token == TokenType.INCLUDE || token == TokenType.USING_NAMESPACE
                        || token == TokenType.INSTANCEOF || token == TokenType.GOTO || token == TokenType.LABEL)
                    {
                        expression.string_values.Add(reader.string_values.Pop());
                    }
                    else if (token == TokenType.LOOP)
                    {
                        expression.loops.Add(reader.loops.Pop());
                    }
                    else if (token == TokenType.LOOP_OPERATOR)
                    {
                        expression.bool_values.Add(reader.bool_values.Pop());
                        expression.string_values.Add(reader.string_values.Pop());
                    }
                    else if (token == TokenType.OPERATOR)
                    {
                        expression.operators.Add(reader.operators.Pop());
                    }
                    else if (token == TokenType.VALUE)
                    {
                        expression.byte_values.Add(reader.byte_values.Pop());
                        expression.values.Add(reader.values.Pop());
                    }
                }
            }
            reader.tokens.RemoveRange(0, pos);
            bool error = false;
            int trypos = -1;
            try
            {
                if (expression.tokens.IsEmpty()) return;
                reparse:
                TokensTemplate template = strongTemplates[expression.tokens[0]];
                try
                {
                    if (template.Parse(expression, exprend))
                    {
                        List<TokensError> errs = template.Run(expression);
                        if (!errs.IsEmpty())
                        {
                            errors.AddRange(errs);
                            error = true;
                        }
                    }
                    else
                        error = true;
                }
                catch
                {
                    error = true;
                }
                if (error)
                {
                    if (!tryTokens.IsEmpty())
                    {
                        LaterCalls.RemoveLast();
                        if (trypos < 0) trypos = tryPositions.Pop();
                        expression.tokens.RemoveAt(trypos);
                        if (count == 0) count = tryCounts.Pop();
                        if (count > 0)
                        {
                            count--;
                            expression.tokens.Insert(trypos, tryTokens.Pop());
                            goto reparse;
                        }
                    }
                    else
                    {
                        errors.Add(new InvalidTokensTemplateError(line, $"Invalid template of token {expression.tokens[0]}"));
                    }
                }
            }
            catch (KeyNotFoundException)
            {
                reparse:
                foreach (TokensTemplate template in flexTemplates)
                {
                    TokensReader backup = new TokensReader();
                    backup.Add(expression);
                    try
                    {
                        if (template.Parse(expression, exprend))
                        {
                            errors.AddRange(template.Run(expression));
                            return;
                        }
                        else
                            expression = backup;
                    }
                    catch { expression = backup; }
                }
                if (!tryTokens.IsEmpty())
                {
                    LaterCalls.RemoveLast();
                    if (trypos < 0) trypos = tryPositions.Pop();
                    expression.tokens.RemoveAt(trypos);
                    if (count == 0) count = tryCounts.Pop();
                    if (count > 0)
                    {
                        count--;
                        expression.tokens.Insert(trypos, tryTokens.Pop());
                        goto reparse;
                    }
                }
                else
                {
                    errors.Add(new InvalidTokensTemplateError(line, $"Unknown tokens template {string.Join(" ", expression.tokens)}"));
                }
            }
            finally
            {
                if (isFuncBody) LaterCalls.Call();
            }
        }

        public int ParseDirective(int pos)
        {
            if (reader.tokens[pos] == TokenType.LITERAL)
            {
                //pos++;
                string dname = reader.string_values.Pop();
                try
                {
                    directives[dname]();
                }
                catch (KeyNotFoundException)
                {
                    errors.Add(new DirectiveError(line, $"Directive with name {dname} not found"));
                }
            }
            else
                errors.Add(new InvalidTokenError(line, $"Invalid token {reader.tokens[pos]} after directive"));
            return pos;
        }
    }
}
