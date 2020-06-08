using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using TokensAPI;
using TokensBuilder.Errors;

namespace TokensBuilder
{
    public sealed class Generator
    {
        //lastLiterals - all literals that was readed
        //curLiteralIndex - index of peeked (need) literal
        //funcArgs - level of argments of method
        //lengthLiterals - список количеств литералов, которые идут через сепаратор (точку) подряд

        public uint line = 0;
        public TokensReader reader;
        public string currentNamespace = "";
        public List<string> literals = new List<string>(), usingNamespaces = new List<string>();
        public Dictionary<string, Action> directives = new Dictionary<string, Action>();
        public byte needEndStatement = 0, needEndSequence = 0, needEndBlock = 0;
        public bool? isActual = null; //need three values
        public List<TokensError> errors = new List<TokensError>();
        public List<CustomAttributeBuilder> attributes = new List<CustomAttributeBuilder>();
        public Stack<List<Type>> parameterTypes = new Stack<List<Type>>();
        public Dictionary<string, Label> labels = new Dictionary<string, Label>();

        //flags
        private bool isDirective = false, needEnd = false, extends = false, implements = false, ifDirective = true, 
            needSeparator = false, needReturn = false, needAssign = false, tryDirective = false, initClass = false, 
            isParams = false, isFuncAlias = false, isTypeAlias = false, isCtor = false;
        private int curLiteralIndex = 0, funcArgs = 0;
        private TokenType prev;
        private Stack<int> lengthLiterals = new Stack<int>();
        private OperatorType? needOperator = null;
        private object value1, value2;

        //properties
        private string curLiteral
        {
            get
            {
                if (curLiteralIndex >= lengthLiterals.Peek() && curLiteralIndex > 0) return literals[curLiteralIndex - 1];
                else return literals[curLiteralIndex];
            }
        }
        private bool isFuncBody => !Context.functionBuilder.IsEmpty;
        private bool isFuncArgs => funcArgs > 0;
        private bool dontPop 
        {
            get => needAssign || needReturn || isFuncArgs;
            set
            {
                needAssign = value;
                //if (!value && needReturn) gen.Emit(OpCodes.Ret);
                needReturn = value;
            }
        }
        /// <summary>
        /// Length of all previous literals. Faster then lengthLiterals.Sum() on ~13 ms
        /// </summary>
        private int sumLength
        {
            get 
            {
                int sum = 0;
                foreach (int num in lengthLiterals)
                {
                    sum += num;
                }
                return sum;
            }
        }
        private ILGenerator gen => Context.functionBuilder.generator;

        //methods
        private void RemoveLastLiterals()
        {
            //llen - length of literals, len - last length of literals
            int llen = literals.Count, len = lengthLiterals.Pop(); //remove last length of literals
            curLiteralIndex = llen - len; //ставим индекс на объект перед которым всё должно быть удалено
            literals.RemoveRange(curLiteralIndex, llen - curLiteralIndex); //и всё удаляем перед этим индексом
            if (curLiteralIndex <= 0) curLiteralIndex = 0;
            else curLiteralIndex--;
        }
        private void AddLengthLiteral() => lengthLiterals.Push(literals.Count - sumLength);

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
                {
                    TokenType tt = reader.tokens.Peek();
                    ParseToken(tt);
                    prev = tt;
                }
            }
            CheckOnAllClosed();
            foreach (TokensError error in errors)
                Console.Error.WriteLine(error);
        }

        #region Private methods for parsing of tokens
        private void CheckOnAllClosed()
        {
            if (needEndBlock > 0) errors.Add(new NeedEndError(line, $"Need end of {needEndBlock} blocks"));
            else if (needEndSequence > 0) errors.Add(new NeedEndError(line, $"Need end of {needEndSequence} arrays"));
            else if (needEndStatement > 0) errors.Add(new NeedEndError(line, $"Need end of {needEndStatement} statements"));
        }

        private bool IsEnd(TokenType token)
            => token == TokenType.EXPRESSION_END || (token == TokenType.BLOCK && reader.bool_values[0]);

        private void ParseStatement()
        {
            if (reader.bool_values.Peek())
            {
                needEndStatement++;
                if (prev == TokenType.LITERAL)
                {
                    funcArgs++;
                    AddLengthLiteral();
                    parameterTypes.Push(new List<Type>());
                }
            }
            else
            {
                needEndStatement--;
                if (isFuncArgs)
                {
                    EndParseArgsOfCallingMethod();
                }
            }
        }

        private void ParseOperator()
        {
            switch (needOperator)
            {
                case OperatorType.ADD:
                    break;
                case OperatorType.SUB:
                    break;
                case OperatorType.MUL:
                    break;
                case OperatorType.DIV:
                    break;
                case OperatorType.MOD:
                    break;
                case OperatorType.EQ:
                    break;
                case OperatorType.NOTEQ:
                    break;
                case OperatorType.NOT:
                    break;
                case OperatorType.AND:
                    break;
                case OperatorType.OR:
                    break;
                case OperatorType.XOR:
                    break;
                case OperatorType.GT:
                    break;
                case OperatorType.LT:
                    break;
                case OperatorType.GTQ:
                    break;
                case OperatorType.LTQ:
                    break;
                case OperatorType.ASSIGN:
                    break;
                case OperatorType.ADDASSIGN:
                    break;
                case OperatorType.SUBASSIGN:
                    break;
                case OperatorType.MULASSIGN:
                    break;
                case OperatorType.DIVASSIGN:
                    break;
                case OperatorType.MODASSIGN:
                    break;
                case OperatorType.CONVERTTO:
                    break;
                case OperatorType.INC:
                    break;
                case OperatorType.DEC:
                    break;
                case OperatorType.IN:
                    break;
                case OperatorType.GORE:
                    break;
                case OperatorType.LORE:
                    break;
                case OperatorType.RANGE:
                    break;
                case OperatorType.POW:
                    break;
            }
        }

        private void Include(string path)
        {
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
        }

        private void EndParseArgsOfCallingMethod()
        {
            funcArgs--;
            int lengthCall = lengthLiterals.Peek(), len = literals.Count;
            string methName = literals.Last(), typeName = string.Join(".", 
                literals.GetRange(len - lengthCall, lengthCall - 1));
            Type callingType = Context.GetTypeByName(typeName);
            MethodInfo callingMethod;
            try
            {
                if (callingType == null)
                {
                    errors.Add(new TypeNotFoundError(line, $"Type by name {typeName} not found"));
                }
                else
                {
                    callingMethod = callingType.GetMethod(methName, parameterTypes.Peek().ToArray());
                    gen.Emit(OpCodes.Call, callingMethod);
                    parameterTypes.Pop();
                    if (!dontPop && callingMethod.ReturnType != typeof(void))
                    {
                        gen.Emit(OpCodes.Pop);
                    }
                    else
                    {
                        if (isFuncArgs) parameterTypes.Peek().Add(callingMethod.ReturnType);
                        dontPop = false;
                    }
                }
            }
            catch
            {
                try
                {
                    if (callingType.GetMethod(methName) == null)
                        errors.Add(new InvalidMethodError(line, $"Method by name {methName}" +
                            $" not found of class {typeName}"));
                }
                catch (AmbiguousMatchException)
                {
                    errors.Add(new InvalidMethodError(line, $"Invalid types of parameters " +
                        $"{string.Join(", ", parameterTypes)} in method {typeName}.{methName}"));
                }
                parameterTypes.Pop();
            }
            RemoveLastLiterals();
        }
        #endregion

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
            else if (needReturn)
            {
                if (IsEnd(token))
                {
                    gen.Emit(OpCodes.Ret);
                    needReturn = false;
                }
                ParseToken(token);
            }
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
                        Context.CreateMethod();
                        break;
                    case TokenType.VAR:
                        if (isFuncBody)
                        {
                            //Context.functionBuilder.DeclareLocal(reader.string_values.Peek(), reader.string_values.Peek());
                            Context.CreateField();
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
                            if (isFuncBody)
                            {
                                gen.Emit(OpCodes.Ret);
                                Context.classBuilder.methodBuilder = null;
                            }
                            else if (!Context.classBuilder.IsEmpty)
                            {
                                Context.classBuilder.End();
                            }
                            needEndBlock--;
                        }
                        break;
                    case TokenType.STATEMENT:
                        ParseStatement();
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
                        literals.Add(literal);
                        break;
                    case TokenType.SEPARATOR:
                        bool expression = reader.bool_values.Peek();
                        if (expression)
                        {
                            if (literals.IsEmpty())
                            {
                                errors.Add(new InvalidTokenError(line, "Expression separator cannot use without literals before him"));
                            }
                        }
                        else
                        {
                            literals.Clear();
                        }
                        break;
                    case TokenType.EXPRESSION_END:
                        if (initClass)
                            Context.classBuilder.End();

                        literals.Clear();
                        curLiteralIndex = 0;
                        initClass = false;
                        needEnd = false;
                        needReturn = false;
                        needAssign = false;
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
                        needOperator = reader.operators.Peek();
                        break;
                    case TokenType.VALUE:
                        switch (reader.byte_values.Peek())
                        {
                            case 0:
                                parameterTypes.Peek().Add(typeof(object));
                                gen.Emit(OpCodes.Ldnull);
                                break;
                            case 1:
                                parameterTypes.Peek().Add(typeof(int));
                                int val = (int)reader.values.Peek();
                                if (val <= byte.MaxValue && val >= byte.MinValue) gen.Emit(OpCodes.Ldc_I4_S, val);
                                else gen.Emit(OpCodes.Ldc_I4, val);
                                break;
                            case 2:
                                parameterTypes.Peek().Add(typeof(string));
                                gen.Emit(OpCodes.Ldstr, (string)reader.values.Peek());
                                break;
                            case 3:
                                parameterTypes.Peek().Add(typeof(byte));
                                gen.Emit(OpCodes.Ldc_I4_S, (byte)reader.values.Peek());
                                break;
                            case 4:
                                parameterTypes.Peek().Add(typeof(bool));
                                if ((bool)reader.values.Peek()) gen.Emit(OpCodes.Ldc_I4_1);
                                else gen.Emit(OpCodes.Ldc_I4_0);
                                break;
                            case 5:
                                parameterTypes.Peek().Add(typeof(char));
                                gen.Emit(OpCodes.Ldc_I4, (char)reader.values.Peek());
                                break;
                            case 6:
                                parameterTypes.Peek().Add(typeof(float));
                                gen.Emit(OpCodes.Ldc_R4, (float)reader.values.Peek());
                                break;
                            case 7:
                                parameterTypes.Peek().Add(typeof(short));
                                gen.Emit(OpCodes.Ldc_I4, (short)reader.values.Peek());
                                break;
                            case 8:
                                parameterTypes.Peek().Add(typeof(long));
                                gen.Emit(OpCodes.Ldc_I8, (long)reader.values.Peek());
                                break;
                            case 9:
                                parameterTypes.Peek().Add(typeof(double));
                                gen.Emit(OpCodes.Ldc_R8, (double)reader.values.Peek());
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
                        needReturn = true;
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
                        Include(reader.string_values.Peek());
                        break;
                    case TokenType.BREAKPOINT:
                        gen.Emit(OpCodes.Break);
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
