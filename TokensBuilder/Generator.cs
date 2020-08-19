using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using TokensAPI;
using TokensBuilder.Errors;
using TokensBuilder.Templates;

namespace TokensBuilder
{
    public sealed class Generator
    {
        //lastLiterals - all literals that was readed
        //curLiteralIndex - index of peeked (need) literal
        //funcArgs - level of argments of method
        //lengthLiterals - список количеств литералов, которые идут через сепаратор (точку) подряд
        //putLoopStatement - идёт ли добавление выражения цикла? (например блок цикла закончился и после него мы пушим выражение)

        public uint line = 0, loopStatement = 0;
        public TokensReader reader, expression;
        public string currentNamespace = "";
        public List<string> literals = new List<string>(), usingNamespaces = new List<string>();
        public Dictionary<string, Action> directives = new Dictionary<string, Action>();
        public byte needEndStatement = 0, needEndSequence = 0, needEndBlock = 0;
        public bool? isActual = null; //need three values
        public List<TokensError> errors = new List<TokensError>();
        public List<CustomAttributeBuilder> attributes = new List<CustomAttributeBuilder>();
        public Stack<List<Type>> parameterTypes = new Stack<List<Type>>();
        public Dictionary<string, Label> labels = new Dictionary<string, Label>();
        public Stack<List<OpCode>> blocks = new Stack<List<OpCode>>();
        public bool tryDirective = false, putLoopStatement = false;
        public TokenType prev;
        public List<TokenType> tryTokens = new List<TokenType>();
        public Stack<int> tryPositions = new Stack<int>(), tryCounts = new Stack<int>(); // positions of 'try' tokens

        #region Flags
        private bool isDirective = false, needEnd = false, extends = false, implements = false, ifDirective = true, 
            needSeparator = false, needReturn = false, needAssign = false, initClass = false, isParams = false,
            isFuncAlias = false, isTypeAlias = false, isCtor = false, needBlock = false;
        private int curLiteralIndex = 0, funcArgs = 0, classDefinition = 0, count = 0;
        private Stack<int> lengthLiterals = new Stack<int>();
        private OperatorType? needOperator = null;
        private Stack<Loop> loops = new Stack<Loop>();
        private byte insertOp = 2;
        private Dictionary<TokenType, TokensTemplate> strongTemplates = new Dictionary<TokenType, TokensTemplate>();
        private List<TokensTemplate> flexTemplates = new List<TokensTemplate>();
        #endregion

        #region Properties
        private string curLiteral
        {
            get
            {
                if (curLiteralIndex >= lengthLiterals.Peek() && curLiteralIndex > 0) return literals[curLiteralIndex - 1];
                else return literals[curLiteralIndex];
            }
        }
        private List<string> lastLiterals
        {
            get
            {
                int ll = lengthLiterals.Peek();
                return literals.GetRange(literals.Count - ll, ll);
            }
        }
        private bool isFuncBody => Context.functionBuilder != null && !Context.functionBuilder.IsEmpty;
        private bool isFuncArgs => funcArgs > 0;
        private bool isClassDefinition => classDefinition > 0;
        private bool dontPop 
        {
            get => needAssign || needReturn || isFuncArgs || putLoopStatement;
            set
            {
                needAssign = value;
                //if (!value && needReturn) gen.Emit(OpCodes.Ret);
                needReturn = value;
            }
        }
        private bool isLoopStatement
        {
            get => loopStatement > 0;
            set
            {
                if (value)
                    loopStatement = needEndStatement;
                else
                    needEndStatement = 0;
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
        #endregion

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
                /*if (tryDirective)
                {
                    //int errlen = errors.Count;
                    Generator backup = this;
                    //TokensReader tokensBackup = reader;
                    backup.ParseToken(backup.reader.tokens.Peek());
                    if (backup.errors.Count == errors.Count)
                    {
                        this.blocks = backup.blocks;
                        this.attributes = backup.attributes;
                        this.curLiteralIndex = backup.curLiteralIndex;
                        this.currentNamespace = backup.currentNamespace;
                        this.directives = backup.directives;
                        this.extends = backup.extends;
                        this.funcArgs = backup.funcArgs;
                        this.ifDirective = backup.ifDirective;
                        this.implements = backup.implements;
                        this.initClass = backup.initClass;
                        this.insertOp = backup.insertOp;
                        this.isActual = backup.isActual;
                        this.isCtor = backup.isCtor;
                        this.isDirective = backup.isDirective;
                        this.isFuncAlias = backup.isFuncAlias;
                        this.isParams = backup.isParams;
                        this.isTypeAlias = backup.isTypeAlias;
                        this.labels = backup.labels;
                        this.lengthLiterals = backup.lengthLiterals;
                        this.line = backup.line;
                        this.literals = backup.literals;
                        this.loops = backup.loops;
                        this.loopStatement = backup.loopStatement;
                        this.needAssign = backup.needAssign;
                        this.needBlock = backup.needBlock;
                        this.needEnd = backup.needEnd;
                        this.needEndBlock = backup.needEndBlock;
                        this.needEndSequence = backup.needEndSequence;
                        this.needEndStatement = backup.needEndStatement;
                        this.needOperator = backup.needOperator;
                        this.needReturn = backup.needReturn;
                        this.needSeparator = backup.needSeparator;
                        this.parameterTypes = backup.parameterTypes;
                        this.prev = backup.prev;
                        this.putLoopStatement = backup.putLoopStatement;
                        this.reader = backup.reader;
                        this.usingNamespaces = backup.usingNamespaces;
                        tryDirective = false;
                    }
                        //errors.RemoveRange(errlen, errors.Count);
                }
                else
                {
                    TokenType tt = reader.tokens.Peek();
                    ParseToken(tt);
                    prev = tt;
                }*/
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
            if (reader.bool_values.Pop()) // open statement
            {
                needEndStatement++;
                if (prev == TokenType.LITERAL)
                {
                    funcArgs++;
                    AddLengthLiteral();
                    parameterTypes.Push(new List<Type>());
                }
                else if (prev == TokenType.LOOP)
                {
                    isLoopStatement = true;
                }
            }
            else // close statement
            {
                needEndStatement--;
                if (prev == TokenType.LITERAL)
                {
                    AddLengthLiteral();
                    LoadVar();
                }
                if (isFuncArgs)
                {
                    EndParseArgsOfCallingMethod();
                }
            }
        }

        private void ParseOperator()
        {
            #region First values in stack
            Type f;
            try
            {
                f = parameterTypes.Peek()[0];
            }
            catch
            {
                f = null;
            }
            #endregion

            #region Check variables on use in operator
            string varname = "";
            if (literals.Count > 0) // before operator was literal (variable name)
            {
                AddLengthLiteral();
                if (lastLiterals.Count == 1)
                {
                    varname = literals[0];
                    f = Context.functionBuilder.GetLocal(varname).LocalType;
                }
                else
                {
                    varname = string.Join(".", lastLiterals);
                }
            }
            #endregion

            switch (needOperator)
            {
                case OperatorType.ADD:
                    gen.Emit(OpCodes.Add);
                    break;
                case OperatorType.SUB:
                    gen.Emit(OpCodes.Sub);
                    break;
                case OperatorType.MUL:
                    gen.Emit(OpCodes.Mul);
                    break;
                case OperatorType.DIV:
                    gen.Emit(OpCodes.Div);
                    break;
                case OperatorType.MOD:
                    gen.Emit(OpCodes.Rem);
                    break;
                case OperatorType.EQ:
                    gen.Emit(OpCodes.Ceq);
                    break;
                case OperatorType.NOTEQ:
                    if (f.IsNumber() || f == typeof(bool))
                    {
                        gen.Emit(OpCodes.Ceq);
                        gen.Emit(OpCodes.Not);
                    }
                    else gen.Emit(OpCodes.Call, f.GetMethod("op_Inequality", parameterTypes.Peek().ToArray()));
                    break;
                case OperatorType.NOT:
                    gen.Emit(OpCodes.Not);
                    break;
                case OperatorType.AND:
                    gen.Emit(OpCodes.And);
                    break;
                case OperatorType.OR:
                    gen.Emit(OpCodes.Or);
                    break;
                case OperatorType.XOR:
                    gen.Emit(OpCodes.Xor);
                    break;
                case OperatorType.GT:
                    gen.Emit(OpCodes.Cgt);
                    break;
                case OperatorType.LT:
                    gen.Emit(OpCodes.Clt);
                    break;
                case OperatorType.GTQ:
                    break;
                case OperatorType.LTQ:
                    break;
                case OperatorType.ASSIGN:
                    if (lastLiterals.Count == 1)
                    {
                        try
                        {
                            gen.Emit(OpCodes.Stloc, Context.functionBuilder.localVariables[varname]);
                        }
                        catch (KeyNotFoundException)
                        {
                            errors.Add(new VarNotFoundError(line, $"Local changable variable by name {varname} not found"));
                        }
                    }
                    else
                    {
                        try
                        {
                            gen.Emit(OpCodes.Stfld, Context.GetVarByName(varname));
                        }
                        catch
                        {
                            errors.Add(new VarNotFoundError(line, $"Global variable by name {varname} not found"));
                        }
                    }
                    RemoveLastLiterals();
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
                    gen.Emit(OpCodes.Ldc_I4_1);
                    gen.Emit(OpCodes.Add);
                    break;
                case OperatorType.DEC:
                    gen.Emit(OpCodes.Ldc_I4_1);
                    gen.Emit(OpCodes.Sub);
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
                    gen.Emit(OpCodes.Call, typeof(Math).GetMethod("Pow"));
                    break;
            }
            needOperator = null;
            insertOp = 2;
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
                        if (isFuncArgs || putLoopStatement) parameterTypes.Peek().Add(callingMethod.ReturnType);
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
                        $"{string.Join(", ", parameterTypes.Peek())} in method {typeName}.{methName}"));
                }
                parameterTypes.Pop();
            }
            RemoveLastLiterals();
        }

        private void ParseValue()
        {
            bool needAdd = insertOp == 2 || putLoopStatement;
            switch (reader.byte_values.Pop())
            {
                case 0:
                    if (needAdd) parameterTypes.Peek().Add(typeof(object));
                    gen.Emit(OpCodes.Ldnull);
                    break;
                case 1:
                    if (needAdd) parameterTypes.Peek().Add(typeof(int));
                    int val = (int)reader.values.Pop();
                    if (val <= sbyte.MaxValue && val >= sbyte.MinValue) gen.Emit(OpCodes.Ldc_I4_S, val);
                    else gen.Emit(OpCodes.Ldc_I4, val);
                    break;
                case 2:
                    if (needAdd) parameterTypes.Peek().Add(typeof(string));
                    gen.Emit(OpCodes.Ldstr, (string)reader.values.Pop());
                    break;
                case 3:
                    if (needAdd) parameterTypes.Peek().Add(typeof(sbyte));
                    gen.Emit(OpCodes.Ldc_I4_S, (sbyte)reader.values.Pop());
                    break;
                case 4:
                    if (needAdd) parameterTypes.Peek().Add(typeof(bool));
                    if ((bool)reader.values.Pop()) gen.Emit(OpCodes.Ldc_I4_1);
                    else gen.Emit(OpCodes.Ldc_I4_0);
                    break;
                case 5:
                    if (needAdd) parameterTypes.Peek().Add(typeof(char));
                    gen.Emit(OpCodes.Ldc_I4, (char)reader.values.Pop());
                    break;
                case 6:
                    if (needAdd) parameterTypes.Peek().Add(typeof(float));
                    gen.Emit(OpCodes.Ldc_R4, (float)reader.values.Pop());
                    break;
                case 7:
                    if (needAdd) parameterTypes.Peek().Add(typeof(short));
                    gen.Emit(OpCodes.Ldc_I4, (short)reader.values.Pop());
                    break;
                case 8:
                    if (needAdd) parameterTypes.Peek().Add(typeof(long));
                    gen.Emit(OpCodes.Ldc_I8, (long)reader.values.Pop());
                    break;
                case 9:
                    if (needAdd) parameterTypes.Peek().Add(typeof(double));
                    gen.Emit(OpCodes.Ldc_R8, (double)reader.values.Pop());
                    break;
            }
        }

        private void LoadVar()
        {
            List<string> _var = lastLiterals;
            string name;
            if (_var.Count == 1)
            {
                // it`s local
                name = _var[0];
                LocalBuilder local = Context.functionBuilder.GetLocal(name);
                if (local == null)
                {
                    errors.Add(new VarNotFoundError(line, $"Local variable with name '{name}' not found"));
                }
                else
                {
                    parameterTypes.Peek().Add(local.LocalType);
                    gen.Emit(OpCodes.Ldloc, local);
                }
            }
            else
            {
                // it`s field of some class or something else
                name = string.Join(".", _var);
                FieldInfo field = Context.GetVarByName(name);
                if (field == null)
                {
                    errors.Add(new VarNotFoundError(line, $"Global variable with name '{name}' not found"));
                }
                else
                {
                    parameterTypes.Peek().Add(field.FieldType);
                    gen.Emit(OpCodes.Ldfld, field);
                }
            }
            RemoveLastLiterals();
        }
        #endregion

        public void ParseToken(TokenType token)
        {
            #region Parse operator
            if (insertOp == 0) insertOp = 1;
            else if (insertOp == 1) ParseOperator();
            #endregion

            if (needEnd)
            {
                if (!IsEnd(token)) errors.Add(new TokensError(line, "End of expression with breakpoint not found"));
                needEnd = false;
                ParseToken(token);
            }
            else if (extends)
            {
                if (token == TokenType.LITERAL)
                    Context.classBuilder.Extends(reader.string_values.Pop());
                else
                    errors.Add(new InvalidTokenError(line, TokenType.LITERAL));
                extends = false;
                //needEnd = true;
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
                    Context.classBuilder.Implements(reader.string_values.Pop());
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
            else if (isLoopStatement)
            {
                if (ParseLoopStatement(token)) loopStatement = 0;
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
                            new ClassBuilder(reader.string_values.Pop(), currentNamespace,
                            reader.class_types.Pop(), reader.securities.Pop());
                        initClass = true;
                        if (isFuncBody)
                            Context.functionBuilder.End();
                        break;
                    case TokenType.FUNCTION:
                        Context.CreateMethod();
                        break;
                    case TokenType.VAR:
                        if (isFuncBody)
                        {
                            Context.CreateLocal();
                        }
                        else
                        {
                            Context.CreateField();
                        }
                        break;
                    case TokenType.BLOCK:
                        if (reader.bool_values.Pop())
                        {
                            needEndBlock++;
                            if (initClass)
                            {
                                initClass = false;
                                classDefinition = needEndBlock;
                                implements = false;
                                extends = false;
                            }
                        }
                        else
                        {
                            if (!loops.IsEmpty())
                            {
                                if (loops.Peek() is DoWhileLoop)
                                {
                                    loops.Peek().EndLoop();
                                    loops.Pop();
                                }
                                else
                                {
                                    loops.Pop().EndLoop();
                                }
                            }
                            else if (isFuncBody)
                            {
                                gen.Emit(OpCodes.Ret);
                                Context.classBuilder.methodBuilder = null;
                            }
                            else if (classDefinition == needEndBlock)
                            {
                                Context.classBuilder.End();
                                classDefinition = 0;
                            }
                            needEndBlock--;
                        }
                        break;
                    case TokenType.STATEMENT:
                        ParseStatement();
                        break;
                    case TokenType.SEQUENCE:
                        if (reader.bool_values.Pop()) needEndSequence++;
                        else needEndSequence--;
                        break;
                    case TokenType.LITERAL:
                        string literal = reader.string_values.Pop();
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
                        bool expression = reader.bool_values.Pop();
                        if (expression) // literal separator - .
                        {
                            if (literals.IsEmpty())
                            {
                                errors.Add(new InvalidTokenError(line, "Expression separator cannot use without literals before him"));
                            }
                        }
                        else // expression separator - ,
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
                        LoopType ltype = reader.loops.Pop();
                        if (ltype == LoopType.WHILE) loops.Push(new WhileLoop());
                        else if (ltype == LoopType.DO) loops.Push(new DoWhileLoop());
                        break;
                    case TokenType.LABEL:
                        break;
                    case TokenType.GOTO:
                        break;
                    case TokenType.LOOP_OPERATOR:
                        break;
                    case TokenType.OPERATOR:
                        needOperator = reader.operators.Pop();
                        insertOp = 0;
                        break;
                    case TokenType.VALUE:
                        ParseValue();
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
                        gen.Emit(OpCodes.Newobj,
                            Context.GetTypeByName(string.Join(".", lastLiterals)).GetConstructor(Type.EmptyTypes));
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
                        isActual = reader.bool_values.Pop();
                        break;
                    case TokenType.TYPEOF:
                        break;
                    case TokenType.NAMESPACE:
                        currentNamespace = reader.string_values.Pop();
                        break;
                    case TokenType.IMPORT_LIBRARY:
                        ParseTokensLibrary(reader.string_values.Pop());
                        break;
                    case TokenType.USING_NAMESPACE:
                        usingNamespaces.Add(reader.string_values.Pop());
                        break;
                    case TokenType.INCLUDE:
                        Include(reader.string_values.Pop());
                        break;
                    case TokenType.BREAKPOINT:
                        gen.Emit(OpCodes.Break);
                        needEnd = true;
                        break;
                    case TokenType.IMPLEMENTS:
                        if (initClass)
                            implements = true;
                        else
                            errors.Add(new NotInitClassError(line, "IMPLEMENTS token cannot be realize without class initilization"));
                        break;
                    case TokenType.EXTENDS:
                        if (initClass)
                            extends = true;
                        else
                            errors.Add(new NotInitClassError(line, "EXTENDS token cannot be realize without class initilization"));
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
                        bool async = reader.bool_values.Pop();
                        break;
                    case TokenType.PARAMETER_TYPE:
                        bool type = reader.bool_values.Pop();
                        break;
                    case TokenType.REF:
                        break;
                    case TokenType.GENERIC:
                        errors.Add(new TokensError(line, "Generics not support in this version of compiler"));
                        break;
                }
            }
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
                            template.Run(expression);
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

        public void ParseTokensLibrary(string path)
        {
            TokensReader tokensReader = new TokensReader();
            try
            {
                if (path.StartsWith("<")) tokensReader.SetPath(path.Remove(path.Length - 2) + ".tokens");
                else tokensReader.SetPath(AppDomain.CurrentDomain.BaseDirectory + "lib/" + path + ".tokens");
            }
            catch
            {
                errors.Add(new TokensLibraryError(line, $"Tokens library by path {path} not found"));
                return;
            }
            tokensReader.GetHeaderAndTarget(out _, out _);
            reader.ReadTokens();
            reader.EndWork();
            reader.Add(tokensReader);
        }

        public bool ParseLoopStatement(TokenType token)
        {
            loops.Peek().statementCode.tokens.Add(token);
            if (token == TokenType.STATEMENT)
            {
                bool open = reader.bool_values.Pop();
                if (needEndStatement == loopStatement && !open)
                {
                    loops.Peek().statementCode.bool_values.Add(false);
                    needEndStatement--;
                    return true;
                }
                else
                {
                    if (open)
                    {
                        loops.Peek().statementCode.bool_values.Add(true);
                        needEndStatement++;
                    }
                    else
                    {
                        loops.Peek().statementCode.bool_values.Add(false);
                        needEndStatement--;
                    }
                }
            }
            else if (token == TokenType.SEQUENCE || token == TokenType.SEPARATOR)
            {
                loops.Peek().statementCode.bool_values.Add(reader.bool_values.Pop());
            }
            else if (token == TokenType.LITERAL || token == TokenType.TYPEOF || token == TokenType.INSTANCEOF)
            {
                loops.Peek().statementCode.string_values.Add(reader.string_values.Pop());
            }
            else if (token == TokenType.VALUE)
            {
                loops.Peek().statementCode.byte_values.Add(reader.byte_values.Pop());
                loops.Peek().statementCode.values.Add(reader.values.Pop());
            }
            else if (token == TokenType.OPERATOR)
            {
                loops.Peek().statementCode.operators.Add(reader.operators.Pop());
            }
            return false;
        }
    }
}
