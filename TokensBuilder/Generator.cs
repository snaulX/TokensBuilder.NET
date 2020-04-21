using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Reflection.Emit;
using TokensAPI;
using TokensBuilder.Errors;

namespace TokensBuilder
{
    public sealed class Generator
    {
        public uint line = 0;
        public TokensReader reader;
        public string currentNamespace = "";
        public List<string> lastLiterals = new List<string>();
        public List<string> usingNamespaces = new List<string>();
        public bool initClass = false, tryDirective = false;
        public Dictionary<string, Action> directives = new Dictionary<string, Action>();
        public byte needEndStatement = 0, needEndSequence = 0, needEndBlock = 0;
        public List<TokensError> errors = new List<TokensError>();
        public List<CustomAttributeBuilder> attributes = new List<CustomAttributeBuilder>();
        public List<Type> parameterTypes = new List<Type>();
        public Dictionary<string, Label> labels = new Dictionary<string, Label>();
        //flags
        private bool isDirective = false, needEnd = false, extends = false, implements = false, isFuncArgs = false,
            ifDirective = true, needSeparator = false;
        public bool? isActual = null; //need three values
        private bool isFuncBody => Context.functionBuilder.IsEmpty;
        private ILGenerator gen => Context.functionBuilder.generator;

        public Generator()
        {
            directives.Add("extends", () =>
            {
                TokenType curToken = reader.tokens.Peek();
                if (curToken == TokenType.LITERAL)
                {
                    if (Config.header == HeaderType.CLASS)
                    {
                        Context.mainClass.Extends(reader.string_values.Peek());
                    }
                    else
                    {
                        errors.Add(new InvalidHeaderError(line, Config.header, "extends directive can be only with class header"));
                    }
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
                        Context.mainClass.Implements(reader.string_values.Peek());
                    else
                        errors.Add(new InvalidHeaderError(line, Config.header, "implements directive can be only with class header"));
                }
                isDirective = false;
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
                string outType = reader.string_values.Peek();
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
            });
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
                if (tryDirective)
                {
                    int errlen = errors.Count;
                    ParseToken(reader.tokens.Peek());
                    if (errors.Count > errlen)
                        errors.RemoveRange(errlen, errors.Count);
                    //tryDirective = false;
                }
                else
                    ParseToken(reader.tokens.Peek());
            }
            CheckOnAllClosed();
            foreach (TokensError error in errors)
                Console.Error.WriteLine(error);
        }

        private void CheckOnAllClosed()
        {
            if (needEndBlock > 0) errors.Add(new NeedEndError(line, $"Need end of {needEndBlock} blocks"));
            else if (needEndSequence > 0) errors.Add(new NeedEndError(line, $"Need end of {needEndSequence} arrays"));
            else if (needEndStatement > 0) errors.Add(new NeedEndError(line, $"Need end of {needEndStatement} statements"));
        }

        private bool IsEnd(TokenType token)
            => token == TokenType.EXPRESSION_END || (token == TokenType.BLOCK && reader.bool_values[0]);

        public void ParseToken(TokenType token)
        {
            if (needEnd)
            {
                if (!IsEnd(token)) errors.Add(new TokensError(line, "End of expression with breakpoint not found"));
                needEnd = false;
                ParseToken(token);
            }
            else if (extends)
            {
                if (token == TokenType.LITERAL)
                    Context.classBuilder.Extends(reader.string_values.Peek());
                else
                    errors.Add(new InvalidTokenError(line, TokenType.LITERAL));
                extends = false;
                needEnd = true;
            }
            else if (implements)
            {
                if (needSeparator)
                {
                    if (IsEnd(token))
                    {
                        implements = false;
                        needSeparator = false;
                        ParseToken(token);
                    }
                }
                else
                {
                    Context.classBuilder.Implements(reader.string_values.Peek());
                    needSeparator = true;
                }
            }
            /*else if (!lastLiterals.IsEmpty())
            {
                if (IsEnd(token))
                    lastLiterals.Clear();
            }*/
            else
            {
                switch (token)
                {
                    case TokenType.NEWLN:
                        isDirective = false;
                        line++;
                        break;
                    case TokenType.CLASS:
                        Context.classBuilder = 
                            new ClassBuilder(reader.string_values.Peek(), currentNamespace,
                            reader.class_types.Peek(), reader.securities.Peek());
                        initClass = true;
                        break;
                    case TokenType.FUNCTION:
                        break;
                    case TokenType.VAR:
                        if (isFuncBody)
                        {
                            Context.functionBuilder.DeclareLocal(reader.string_values.Peek(), reader.string_values.Peek());
                        }
                        break;
                    case TokenType.BLOCK:
                        if (reader.bool_values.Peek())
                        {
                            implements = false;
                            extends = false;
                            needEndBlock++;
                        }
                        else
                        {
                            if (!Context.classBuilder.IsEmpty) 
                            {
                                Context.classBuilder.End();
                            }
                            needEndBlock--;
                        }
                        break;
                    case TokenType.STATEMENT:
                        if (reader.bool_values.Peek())
                        {
                            needEndStatement++;
                            if (!lastLiterals.IsEmpty())
                            {
                                isFuncArgs = true;
                            }
                        }
                        else
                        {
                            if (isFuncArgs)
                            {
                                Context.classBuilder.methodBuilder.generator.Emit(OpCodes.Call,
                                    Context.GetTypeByName(lastLiterals[0], usingNamespaces)
                                    .GetMethod(lastLiterals[1], parameterTypes.ToArray()));
                                parameterTypes.Clear();
                                isFuncArgs = false;
                            }
                            needEndStatement--;
                        }
                        break;
                    case TokenType.SEQUENCE:
                        if (reader.bool_values.Peek()) needEndSequence++;
                        else needEndSequence--;
                        break;
                    case TokenType.LITERAL:
                        string literal = reader.string_values.Peek();
                        if (isDirective)
                        {
                            try
                            {
                                directives[literal]();
                            }
                            catch (KeyNotFoundException)
                            {
                                errors.Add(new DirectiveError(line, $"Directive by name {literal} not found"));
                            }
                            isDirective = false;
                        }
                        else
                        {
                            //pass
                        }
                        lastLiterals.Add(literal);
                        break;
                    case TokenType.SEPARATOR:
                        bool expression = reader.bool_values.Peek();
                        if (expression)
                        {
                            if (lastLiterals.IsEmpty())
                            {
                                errors.Add(new InvalidTokenError(line, "Expression separator cannot use without literals before him"));
                            }
                        }
                        else
                        {
                            lastLiterals.Clear();
                        }
                        break;
                    case TokenType.EXPRESSION_END:
                        if (initClass)
                            Context.classBuilder.End();

                        initClass = false;
                        needEnd = false;
                        break;
                    case TokenType.LOOP:
                        break;
                    case TokenType.LABEL:
                        break;
                    case TokenType.GOTO:
                        break;
                    case TokenType.LOOP_OPERATOR:
                        break;
                    case TokenType.OPERATOR:
                        break;
                    case TokenType.VALUE:
                        switch (reader.byte_values.Peek())
                        {
                            case 0:
                                parameterTypes.Add(typeof(object));
                                gen.Emit(OpCodes.Ldnull);
                                break;
                            case 1:
                                break;
                            case 2:
                                parameterTypes.Add(typeof(string));
                                gen.Emit(OpCodes.Ldstr, (string) reader.values.Peek());
                                break;
                        }
                        break;
                    case TokenType.NULLABLE:
                        break;
                    case TokenType.SWITCH:
                        break;
                    case TokenType.CASE:
                        break;
                    case TokenType.DIRECTIVE:
                        isDirective = true;
                        break;
                    case TokenType.NEW:
                        break;
                    case TokenType.ANNOTATION:
                        break;
                    case TokenType.THROW:
                        break;
                    case TokenType.TRY:
                        break;
                    case TokenType.CATCH:
                        break;
                    case TokenType.FINALLY:
                        break;
                    case TokenType.IF:
                        break;
                    case TokenType.ELSE:
                        break;
                    case TokenType.RETURN:
                        break;
                    case TokenType.ACTUAL:
                        isActual = reader.bool_values.Peek();
                        break;
                    case TokenType.TYPEOF:
                        break;
                    case TokenType.NAMESPACE:
                        currentNamespace = reader.string_values.Peek();
                        break;
                    case TokenType.IMPORT_LIBRARY:
                        ParseTokensLibrary(reader.string_values.Peek(), ref reader);
                        break;
                    case TokenType.USING_NAMESPACE:
                        usingNamespaces.Add(reader.string_values.Peek());
                        break;
                    case TokenType.INCLUDE:
                        string path = reader.string_values.Peek();
                        try
                        {
                            Assembly.LoadFrom(path);
                        }
                        catch (FileNotFoundException)
                        {
                            errors.Add(new IncludeError(line, $"The {path} was not found, or the module" +
                                " you are trying to load does not indicate a file name extension."));
                        }
                        catch (FileLoadException)
                        {
                            errors.Add(new IncludeError(line, "Failed to load the file that was found." +
                                " or The ability to execute code in remote assemblies is disabled."));
                        }
                        catch (BadImageFormatException)
                        {
                            errors.Add(new IncludeError(line, $"{Path.GetFileName(path)} is not valid assembly"));
                        }
                        catch (ArgumentException)
                        {
                            errors.Add(new IncludeError(line, $"Name of assembly is empty or not valid"));
                        }
                        catch (PathTooLongException)
                        {
                            errors.Add(new IncludeError(line, "The assembly name is longer than the maximum length" +
                                " defined in the system."));
                        }
                        break;
                    case TokenType.BREAKPOINT:
                        Context.functionBuilder.generator.Emit(OpCodes.Break);
                        needEnd = true;
                        break;
                    case TokenType.IMPLEMENTS:
                        CheckOnAllClosed();
                        implements = true;
                        break;
                    case TokenType.EXTENDS:
                        CheckOnAllClosed();
                        extends = true;
                        break;
                    case TokenType.INSTANCEOF:
                        break;
                    case TokenType.WITH:
                        break;
                    case TokenType.YIELD:
                        break;
                    case TokenType.LAMBDA:
                        break;
                    case TokenType.ASYNC:
                        bool async = reader.bool_values.Peek();
                        break;
                    case TokenType.PARAMETER_TYPE:
                        bool type = reader.bool_values.Peek();
                        break;
                    case TokenType.REF:
                        break;
                }
            }
        }

        public void ParseTokensLibrary(string path, ref TokensReader treader)
        {
            TokensReader tokensReader = new TokensReader();
            try
            {
                if (path.StartsWith("<")) tokensReader.SetPath(path.Remove(path.Length - 2) + ".tokens");
                else tokensReader.SetPath(AppDomain.CurrentDomain.BaseDirectory + path + ".tokens");
            }
            catch
            {
                errors.Add(new TokensLibraryError(line, $"Tokens library by path {path} not found"));
                return;
            }
            tokensReader.GetHeaderAndTarget(out byte header, out _);
            if (header != 5) throw new InvalidHeaderException(header);
            reader.ReadTokens();
            reader.EndWork();
            treader.Add(tokensReader);
        }
    }
}
