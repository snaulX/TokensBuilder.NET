using System;
using System.Collections.Generic;
using System.Reflection.Emit;
using System.Reflection;
using TokensAPI;
using System.Text;
using System.Linq;

namespace TokensBuilder
{
    enum BlockType
    {
        DEFAULT,
        IF,
        ELIF,
        ELSE,
        WHILE,
        FOR,
        FOREACH,
        SWITCH,
        MODULE,
        CLASS,
        INTERFACE,
        STRUCT,
        ENUM,
        METHOD,
        CONSTRUCTOR,
        GET,
        SET,
        TRY,
        CATCH,
        FINALLY,
        ENTRYPOINT,
        ASYNC,
        EVENT,
        WITH
    }

    public class Generator
    {
        public ContextInfo context;
        public List<Expression> expressions;

        public Generator()
        {
            expressions = new List<Expression>();
            context = new ContextInfo();
        }

        public void GenerateIL(string assembly_name, string code, bool haveScript = true)
        {
            //parse code to expressions
            string[] lines = code.Split('\n', '\r');
            try
            {
                for (int i = 0; i < lines.Length; i++)
                {
                    string line = lines[i].Trim();
                    int j = 0;
                    StringBuilder buffer = new StringBuilder();
                    try
                    {
                        for (j = 0; !char.IsWhiteSpace(line[j]); j++)
                        {
                            try {
                                Console.WriteLine("LINE " + line + ' ' + j.ToString());
                                buffer.Append(line[j]); 
                            }
                            catch { break; }
                        }
                        Console.WriteLine("LINE " + line);
                        Token token = (Token)Enum.Parse(typeof(Token), buffer.ToString());
                        Console.WriteLine("TOKEN " + buffer.ToString());
                        buffer.Clear();
                        if (j == line.Length)
                        {
                            while (char.IsWhiteSpace(line[j]))
                            {
                                j++;
                            } //skip whitespaces
                            List<Identifer> args = new List<Identifer>();
                            byte priority = 0;
                            for (j = j; j < line.Length; j++)
                            {
                                char cur = line[j];
                                if (cur == '(')
                                {
                                    buffer.Append(cur);
                                    priority++;
                                }
                                else if (cur == ')' && priority > 0)
                                {
                                    buffer.Append(cur);
                                    priority--;
                                }
                                else if (char.IsWhiteSpace(cur) && priority == 0)
                                {
                                    args.Add(Identifer.GetIdentifer(buffer.ToString()));
                                    buffer.Clear();
                                }
                                else
                                {
                                    buffer.Append(cur);
                                }
                            }
                            args.Add(Identifer.GetIdentifer(buffer.ToString()));
                            expressions.Add(new Expression { token = token, args = args });
                        }
                        else
                        {
                            expressions.Add(new Expression { token = token });
                        }
                    }
                    catch { } //just skip
                }
            }
            catch { }
            Build(assembly_name, haveScript);
        }

        public void Build(string assembly_name, bool haveScript)
        {
            //create context
            context.appName = assembly_name;
            context.CreateName();
            context.CreateAssembly();
            context.haveScript = haveScript;
            if (haveScript) context.InitilizateScript();

            //variables for building
            List<BlockType> blockType = new List<BlockType> { BlockType.DEFAULT };
            string namespace_name = "";
            Dictionary<string, Label> labels = new Dictionary<string, Label>();
            bool tryBlock = false, haveReturn = false, ifDirective = true;
            Statement statement;

            //parse expressions
            for (int i = 0; i < expressions.Count; i++)
            {
                Expression e = expressions[i];
                string name = e.args[0].GetValue();
                if (!ifDirective)
                {
                    if (e.token == Token.DIRECTIVA)
                    {
                        if (name == "endif")
                            ifDirective = true;
                    }
                }
                Console.WriteLine(e.token + " " + string.Join(" ", e.args) + "\t" + string.Join(" ", blockType));
                switch (e.token)
                {
                    case Token.NULL:
                        context.ILGenerator.Emit(OpCodes.Nop);
                        break;
                    case Token.USE:
                        context.ILGenerator.UsingNamespace(name);
                        break;
                    case Token.INCLUDE:
                        context.references.Add(Assembly.LoadFrom(name));
                        break;
                    case Token.WRITEVAR:
                        context.LoadValue(name);
                        context.ILGenerator.Emit(OpCodes.Stfld);
                        break;
                    case Token.CLASS:
                        if (namespace_name != string.Empty)
                            name = namespace_name + '.' + name;
                        context.type = context.module.DefineType(name, TypeAttributes.Class);
                        blockType.Add(BlockType.CLASS);
                        break;
                    case Token.FIELD:
                        break;
                    case Token.METHOD:
                        if (blockType.Count == 1)
                        {
                            Type type = Type.GetType(name);
                            if (type != null)
                            {
                                //extension function
                                DynamicMethod method = new DynamicMethod(e.args[1].GetValue(), Type.GetType(e.args[2].GetValue()),
                                    Type.EmptyTypes, type);
                            }
                            else if (haveScript)
                            {
                                //script function
                                List<Identifer> argums = e.args.GetRange(1, e.args.Count);
                                for (int j = 0; j < argums.Count; j++)
                                {
                                    string attr = argums[j].GetValue();
                                }
                                context.method = context.mainType.DefineMethod(name, MethodAttributes.Public);
                            }
                            else
                            {
                                throw new Exception("Cannot initiliaze method without class in non-script program");
                            }
                        }
                        else
                        {
                            MethodBuilder method = context.type.DefineMethod(name, MethodAttributes.HideBySig);
                            context.method = method;
                        }
                        blockType.Add(BlockType.METHOD);
                        break;
                    case Token.END:
                        BlockType last = blockType.Last(); //get current block
                        if (last == BlockType.DEFAULT) context.ILGenerator.Emit(OpCodes.Br);
                        else if (last == BlockType.TRY) tryBlock = true;
                        else if (last == BlockType.CATCH) context.ILGenerator.EndExceptionBlock();
                        else if (last == BlockType.METHOD)
                        {
                            if (context.method.ReturnType == typeof(void))
                            {
                                if (!haveReturn) context.ILGenerator.Emit(OpCodes.Ret);
                            }
                            else
                            {
                                if (!haveReturn) throw new Exception("Method by name '" + context.method.Name + "' haven`t return");
                            }
                            context.EndMethod();
                            haveReturn = false;
                        }
                        else if (last == BlockType.GET)
                        {
                            if (!haveReturn) throw new Exception("Getter of property by name '" + context.property.Name + "' haven`t return");
                            context.property.SetGetMethod(context.method);
                            context.EndMethod();
                            haveReturn = false;
                        }
                        else if (last == BlockType.SET)
                        {
                            if (haveReturn) throw new Exception("Setter of property by name '" + context.property.Name + "' have return");
                            context.property.SetSetMethod(context.method);
                            context.EndMethod();
                        }
                        else if (last == BlockType.WHILE)
                        {
                            //pass
                        }
                        else if (last == BlockType.FOR)
                        {
                            //pass
                        }
                        else if (last == BlockType.FOREACH)
                        {
                            //pass
                        }
                        else if (last == BlockType.WITH) context.withVariable = null;
                        else if (last == BlockType.EVENT) context.eventBuilder = null;
                        else if (last == BlockType.CLASS) context.type.CreateType();
                        else if (last == BlockType.ENUM) context.enumBuilder.CreateType();

                        blockType.RemoveAt(blockType.Count - 1); //end (remove) current block
                        break;
                    case Token.RUNFUNC:
                        try { for (int j = 2; j < e.args.Count; j++) context.LoadValue(e.args[i].GetValue()); }
                        catch { }
                        context.ILGenerator.Emit(OpCodes.Call, Type.GetType(name).GetMethod(e.args[1].GetValue())); 
                        break;
                    case Token.WHILE:
                        blockType.Add(BlockType.WHILE);
                        break;
                    case Token.FOR:
                        blockType.Add(BlockType.FOR);
                        break;
                    case Token.FOREACH:
                        blockType.Add(BlockType.FOREACH);
                        break;
                    case Token.BREAK:
                        context.ILGenerator.Emit(OpCodes.Br_S);
                        break;
                    case Token.CONTINUE:
                        context.ILGenerator.Emit(OpCodes.Br_S);
                        break;
                    case Token.RETURN:
                        if (blockType.Contains(BlockType.METHOD) || blockType.Contains(BlockType.GET))
                        {
                            context.LoadValue(name);
                            context.ILGenerator.Emit(OpCodes.Ret);
                            haveReturn = true;
                        }
                        else
                        {
                            throw new Exception("Operator 'return' called not in function");
                        }
                        break;
                    case Token.IF:
                        blockType.Add(BlockType.IF);
                        break;
                    case Token.ELSE:
                        blockType.Add(BlockType.ELSE);
                        break;
                    case Token.ELIF:
                        blockType.Add(BlockType.ELIF);
                        break;
                    case Token.GOTO:
                        context.ILGenerator.Emit(OpCodes.Br, labels[name]);
                        break;
                    case Token.LABEL:
                        Label label = context.ILGenerator.DefineLabel();
                        context.ILGenerator.MarkLabel(label);
                        labels.Add(name, label);
                        break;
                    case Token.YIELD:
                        if ((blockType.Contains(BlockType.METHOD) || blockType.Contains(BlockType.GET)) && context.method.ReturnType is IEnumerable<object>)
                        {
                            //pass
                        }
                        else
                        {
                            throw new InvalidOperationException("Yield can be used in method or getter only with return type IEnumerable");
                        }
                        break;
                    case Token.STRUCT:
                        if (namespace_name != string.Empty)
                            name = namespace_name + '.' + name;
                        context.type = context.module.DefineType(name);
                        blockType.Add(BlockType.STRUCT);
                        break;
                    case Token.INTERFACE:
                        if (namespace_name != string.Empty)
                            name = namespace_name + '.' + name;
                        context.type = context.module.DefineType(name, TypeAttributes.Interface);
                        blockType.Add(BlockType.INTERFACE);
                        break;
                    case Token.ENUM:
                        if (namespace_name != string.Empty)
                            name = namespace_name + '.' + name;
                        context.enumBuilder = context.module.DefineEnum(name, TypeAttributes.NotPublic, typeof(int));
                        blockType.Add(BlockType.ENUM);
                        break;
                    case Token.MODULE:
                        blockType.Add(BlockType.MODULE);
                        break;
                    case Token.CONSTRUCTOR:
                        blockType.Add(BlockType.CONSTRUCTOR);
                        break;
                    case Token.ATTRIBUTE:
                        if (name == "Entrypoint")
                        {
                            blockType.Add(BlockType.ENTRYPOINT);
                        }
                        break;
                    case Token.OPCODEADD:
                        context.ILGenerator.Emit((OpCode) typeof(OpCodes).GetField(name).GetValue(null));
                        break;
                    case Token.EVENT:
                        context.eventBuilder = context.type.DefineEvent(name, EventAttributes.None, Type.GetType(e.args[1].GetValue()));
                        blockType.Add(BlockType.EVENT);
                        break;
                    case Token.GETEVENT:
                        break;
                    case Token.TRY:
                        context.ILGenerator.BeginExceptionBlock();
                        blockType.Add(BlockType.TRY);
                        break;
                    case Token.CATCH:
                        if (!tryBlock) throw new Exception("Catch-block cannot be without try-block");
                        try { context.ILGenerator.BeginCatchBlock(Type.GetType(name)); }
                        catch { context.ILGenerator.BeginCatchBlock(typeof(Exception)); }
                        blockType.Add(BlockType.CATCH);
                        break;
                    case Token.IMPLEMENTS:
                        context.type.AddInterfaceImplementation(Type.GetType(name));
                        break;
                    case Token.THROW:
                        context.ILGenerator.ThrowException(Type.GetType(name));
                        break;
                    case Token.OVERRIDE:
                        context.type.SetParent(Type.GetType(name));
                        break;
                    case Token.GET:
                        MethodBuilder getter = context.type.DefineMethod("get_" + name, MethodAttributes.Final);
                        context.method = getter;
                        blockType.Add(BlockType.GET);
                        break;
                    case Token.SET:
                        MethodBuilder setter = context.type.DefineMethod("set_" + name, MethodAttributes.Final);
                        context.method = setter;
                        blockType.Add(BlockType.SET);
                        break;
                    case Token.TYPEOF:
                        context.ILGenerator.Emit(OpCodes.Ldobj, Type.GetType(name));
                        break;
                    case Token.CONST:
                        break;
                    case Token.ASYNC:
                        blockType.Add(BlockType.ASYNC);
                        break;
                    case Token.AWAIT:
                        break;
                    case Token.SWITCH:
                        context.ILGenerator.Emit(OpCodes.Switch);
                        break;
                    case Token.CASE:
                        break;
                    case Token.DEFAULT:
                        break;
                    case Token.STARTBLOCK:
                        label = context.ILGenerator.DefineLabel();
                        context.ILGenerator.MarkLabel(label);
                        blockType.Add(BlockType.DEFAULT);
                        break;
                    case Token.DIRECTIVA:
                        string arg = e.args[1].GetValue();
                        if (name == "enable")
                        {
                            if (arg == "script") context.haveScript = true;
                            else if (arg == "entrypoint") context.haveScript = false;
                        }
                        else if (name == "company")
                        {
                            context.assembly.SetCustomAttribute(
                                new CustomAttributeBuilder(
                                    typeof(AssemblyCompanyAttribute).GetConstructor(new Type[] { typeof(string) }
                                    ), new object[] { arg }));
                        }
                        else if (name == "version")
                        {
                            context.assembly.SetCustomAttribute(
                                new CustomAttributeBuilder(
                                    typeof(AssemblyVersionAttribute).GetConstructor(new Type[] { typeof(string) }
                                    ), new object[] { arg }));
                        }
                        else if (name == "file_version")
                        {
                            context.assembly.SetCustomAttribute(
                                new CustomAttributeBuilder(
                                    typeof(AssemblyFileVersionAttribute).GetConstructor(new Type[] { typeof(string) }
                                    ), new object[] { arg }));
                        }
                        else if (name == "copyright")
                        {
                            context.assembly.SetCustomAttribute(
                                new CustomAttributeBuilder(
                                    typeof(AssemblyCopyrightAttribute).GetConstructor(new Type[] { typeof(string) }
                                    ), new object[] { arg }));
                        }
                        else if (name == "title")
                        {
                            context.assembly.SetCustomAttribute(
                                new CustomAttributeBuilder(
                                    typeof(AssemblyTitleAttribute).GetConstructor(new Type[] { typeof(string) }
                                    ), new object[] { arg }));
                        }
                        else if (name == "description")
                        {
                            context.assembly.SetCustomAttribute(
                                new CustomAttributeBuilder(
                                    typeof(AssemblyDescriptionAttribute).GetConstructor(new Type[] { typeof(string) }
                                    ), new object[] { arg }));
                        }
                        else if (name == "product_name")
                        {
                            context.assembly.SetCustomAttribute(
                                new CustomAttributeBuilder(
                                    typeof(AssemblyProductAttribute).GetConstructor(new Type[] { typeof(string) }
                                    ), new object[] { arg }));
                        }
                        else if (name == "if")
                        {
                            if (arg == "DOTNET")
                                ifDirective = true;
                            else if (arg == "JVM" || arg == "LLVM")
                                ifDirective = false;
                            else
                                throw new KeyNotFoundException($"Statement by name {arg} not found in TokensBuilder (TokensError in line {i})");
                        }
                        else
                        {
                            throw new NotSupportedException($"Directiva by name {name} not found (TokensError in line {i})");
                        }
                        break;
                    case Token.NAMESPACE:
                        namespace_name = e.args[0].GetValue();
                        break;
                    case Token.BREAKPOINT:
                        context.ILGenerator.Emit(OpCodes.Break);
                        break;
                    case Token.WITH:
                        blockType.Add(BlockType.WITH);
                        context.withVariable = context.type.GetField(name);
                        break;
                    case Token.SIZEOF:
                        context.LoadValue(name);
                        context.ILGenerator.Emit(OpCodes.Sizeof);
                        break;
                    case Token.ADD:
                        foreach (Identifer identifer in e.args)
                        {
                            context.LoadValue(identifer.GetValue());
                        }
                        context.ILGenerator.Emit(OpCodes.Add);
                        break;
                    case Token.SUB:
                        foreach (Identifer identifer in e.args)
                        {
                            context.LoadValue(identifer.GetValue());
                        }
                        context.ILGenerator.Emit(OpCodes.Sub);
                        break;
                    case Token.DIV:
                        foreach (Identifer identifer in e.args)
                        {
                            context.LoadValue(identifer.GetValue());
                        }
                        context.ILGenerator.Emit(OpCodes.Div);
                        break;
                    case Token.MUL:
                        foreach (Identifer identifer in e.args)
                        {
                            context.LoadValue(identifer.GetValue());
                        }
                        context.ILGenerator.Emit(OpCodes.Mul);
                        break;
                    case Token.MOD:
                        foreach (Identifer identifer in e.args)
                        {
                            context.LoadValue(identifer.GetValue());
                        }
                        context.ILGenerator.Emit(OpCodes.Rem);
                        break;
                    case Token.AND:
                        foreach (Identifer identifer in e.args)
                        {
                            context.LoadValue(identifer.GetValue());
                        }
                        context.ILGenerator.Emit(OpCodes.And);
                        break;
                    case Token.OR:
                        foreach (Identifer identifer in e.args)
                        {
                            context.LoadValue(identifer.GetValue());
                        }
                        context.ILGenerator.Emit(OpCodes.Or);
                        break;
                    case Token.XOR:
                        foreach (Identifer identifer in e.args)
                        {
                            context.LoadValue(identifer.GetValue());
                        }
                        context.ILGenerator.Emit(OpCodes.Xor);
                        break;
                    case Token.NOT:
                        foreach (Identifer identifer in e.args)
                        {
                            context.LoadValue(identifer.GetValue());
                        }
                        context.ILGenerator.Emit(OpCodes.Not);
                        break;
                    case Token.CEQ:
                        foreach (Identifer identifer in e.args)
                        {
                            context.LoadValue(identifer.GetValue());
                        }
                        context.ILGenerator.Emit(OpCodes.Ceq);
                        break;
                    case Token.CLT:
                        foreach (Identifer identifer in e.args)
                        {
                            context.LoadValue(identifer.GetValue());
                        }
                        context.ILGenerator.Emit(OpCodes.Clt);
                        break;
                    case Token.CGT:
                        foreach (Identifer identifer in e.args)
                        {
                            context.LoadValue(identifer.GetValue());
                        }
                        context.ILGenerator.Emit(OpCodes.Cgt);
                        break;
                }
            }

            //close context
            if (blockType.Count == 1)
            {
                if (context.haveScript) context.EndScript();
                context.EndWrite();
            }
            else
            {
                throw new Exception("Block(s) not closed. TokensError in the end of compilation");
            }
        }

        public void CreatePE(string full_name) => context.assembly.Save(full_name);
    }

    public class ContextInfo
    {
        public Parser parser;
        public bool haveScript;
        public string appName;
        public PEFileKinds outputType;
        public AssemblyBuilder assembly;
        public AssemblyName assemblyName;
        public ModuleBuilder module;
        public TypeBuilder type, mainType;
        public EnumBuilder enumBuilder;
        public MethodBuilder method, script;
        public FieldBuilder field;
        public PropertyBuilder property;
        public LocalBuilder local;
        public ConstructorBuilder constructor;
        public EventBuilder eventBuilder;
        public ILGenerator ILGenerator => method.GetILGenerator();
        public FieldInfo withVariable;
        public List<Assembly> references; 

        public ContextInfo()
        {
            appName = "";
            outputType = PEFileKinds.ConsoleApplication;
            assemblyName = new AssemblyName();
            method = null;
            script = null;
            haveScript = true;
            parser = new Parser(this);
        }

        public void EndWrite()
        {
            if (haveScript) mainType.CreateType();
        }

        public void EndMethod()
        {
            method = null;
        }

        public void CreateName()
        {
            int index = appName.LastIndexOf('.');
            if (index <= 0) assemblyName.Name = appName;
            else assemblyName.Name = appName.Substring(0, index);
        }

        public void CreateAssembly()
        {
            assembly = AppDomain.CurrentDomain.DefineDynamicAssembly(assemblyName, AssemblyBuilderAccess.RunAndSave);
            module = assembly.DefineDynamicModule(assemblyName.Name);
            if (haveScript) mainType = module.DefineType(appName + "Main", TypeAttributes.Class);
        }

        public void InitilizateScript()
        {
            if (haveScript)
            {
                script = mainType.DefineMethod("Main",
                    MethodAttributes.Private | MethodAttributes.Static,
                    CallingConventions.Any, typeof(void), new Type[] { typeof(string[]) });
                method = script;
            }
            else
            {
                throw new InvalidOperationException("Cannot initilizate script in none-script program (TokensError)");
            }
        }

        public void EndScript()
        {
            if (haveScript)
            {
                ILGenerator.Emit(OpCodes.Ret);
                script = method;
                method = null;
                assembly.SetEntryPoint(script.GetBaseDefinition(), outputType);
            }
            else
            {
                throw new InvalidOperationException("Cannot end script in none-script program (TokensError)");
            }
        }

        public void LoadValue(string value)
        {
            if (value[0] == '\"' && value[value.Length - 1] == '\"')
            {
                ILGenerator.Emit(OpCodes.Ldstr, value.Substring(1, value.Length - 2));
            }
            else if (int.TryParse(value, out int a))
            {
                ILGenerator.Emit(OpCodes.Ldc_I4, a);
            }
            else if (long.TryParse(value, out long b))
            {
                ILGenerator.Emit(OpCodes.Ldc_I8, b);
            }
            else if (float.TryParse(value, out float c))
            {
                ILGenerator.Emit(OpCodes.Ldc_R4, c);
            }
            else if (double.TryParse(value, out double d))
            {
                ILGenerator.Emit(OpCodes.Ldc_R8, d);
            }
            else if (bool.TryParse(value, out bool e))
            {
                if (e) ILGenerator.Emit(OpCodes.Ldc_I4_1);
                else ILGenerator.Emit(OpCodes.Ldc_I4_0);
            }
            else if (value == "null")
            {
                ILGenerator.Emit(OpCodes.Ldnull);
            }
            else if (value[0] == '[' && value[value.Length - 1] == ']')
            {
                value = value.Substring(1, value.Length - 2);
                string typename = (string) value.TakeWhile((ch) => !char.IsWhiteSpace(ch));
                value = value.Remove(0, typename.Length);
                ILGenerator.Emit(OpCodes.Newarr, Type.GetType(typename));
            }
            else if (value.StartsWith("new "))
            {
                value = value.Remove(0, 4);
                ILGenerator.Emit(OpCodes.Newobj);
            }
            else
            {
                //ILGenerator.Emit(OpCodes.Newobj, Parser.ParseLine(value, this));
            }
        }

        public object GetValue(string value)
        {
            if (value[0] == '\"' && value[value.Length - 1] == '\"')
            {
                return value.Substring(1, value.Length - 2);
            }
            else if (value[0] == '\'' && value[value.Length - 1] == '\'')
            {
                return char.Parse(value.Substring(1, value.Length - 2));
            }
            else if (int.TryParse(value, out int a))
            {
                return a;
            }
            else if (long.TryParse(value, out long b))
            {
                return b;
            }
            else if (float.TryParse(value, out float c))
            {
                return c;
            }
            else if (double.TryParse(value, out double d))
            {
                return d;
            }
            else if (bool.TryParse(value, out bool e))
            {
                return e;
            }
            else if (value == "null")
            {
                return null;
            }
            else if (value[0] == '[' && value[value.Length - 1] == ']')
            {
                value = value.Substring(1, value.Length - 2);
                string typename = (string)value.TakeWhile((ch) => !char.IsWhiteSpace(ch));
                value = value.Remove(0, typename.Length);
                return Type.GetType(typename);
            }
            else if (value.StartsWith("new "))
            {
                value = value.Remove(0, 4);
                return value;
            }
            else
            {
                return parser.ParseLine(value);
            }
        }

        public Type FindStaticClass(string name)
        {
            if (haveScript)
                if (mainType.Name == name)
                    return mainType;
            Type returnType = assembly.GetType(name);
            if (returnType == null)
            {
                for (int i = 0; i < references.Count; i++)
                {
                    returnType = references[i].GetType(name);
                    if (returnType == null)
                    {
                        continue;
                    }
                    else
                    {
                        return returnType;
                    }
                }
            }
            else
            {
                return returnType;
            }
            throw new TypeLoadException($"Static class by name '{name}' not found (TokensError)");
        }
    }
}
