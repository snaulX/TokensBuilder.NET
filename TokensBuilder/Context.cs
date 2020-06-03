using System;
using System.Collections.Generic;
using System.Reflection.Emit;
using System.Reflection;
using TokensAPI;
using TokensBuilder.Errors;
using TokensStandart;

namespace TokensBuilder
{
    public static class Context
    {
        public static AssemblyBuilder assemblyBuilder = null;
        public static AssemblyName assemblyName = new AssemblyName();
        public static ModuleBuilder moduleBuilder = null;
        private static ClassBuilder _classBuilder = null;
        public static ClassBuilder mainClass = null;
        public static ClassBuilder classBuilder
        {
            get => _classBuilder ?? mainClass;
            set => _classBuilder = value;
        }
        public static FunctionBuilder functionBuilder => classBuilder.methodBuilder;
        private static Generator gen => TokensBuilder.gen;
        public static readonly CustomAttributeBuilder entrypointAttr = new CustomAttributeBuilder(
                        typeof(EntrypointAttribute).GetConstructor(Type.EmptyTypes), new object[] { }),
            scriptAttr = new CustomAttributeBuilder(
                        typeof(ScriptAttribute).GetConstructor(Type.EmptyTypes), new object[] { });
        public static Dictionary<string, object> constants = new Dictionary<string, object>();
        public static List<MethodInfo> scriptFunctions = new List<MethodInfo>();

        public static MethodInfo FindScriptFunction(string name)
        {
            foreach (MethodInfo func in scriptFunctions)
            {
                if (func.Name == name)
                    return func;
            }
            return null;
        }

        public static Type GetTypeByName(string name) => GetTypeByName(name, gen.usingNamespaces);

        public static Type GetTypeByName(string name, IEnumerable<string> namespaces)
        {
            Type type = null;
            type = Type.GetType(name);
            if (type != null) return type;
            foreach (string nameSpace in namespaces)
            {
                type = Type.GetType(nameSpace + '.' + name);
                if (type != null) return type;
            }
            return null;
        }

        public static Type GetInterfaceByName(string name, IEnumerable<string> namespaces)
        {
            Type iface = null;
            foreach (string nameSpace in namespaces)
            {
                iface = Assembly.GetExecutingAssembly().GetType(nameSpace + name);
                if (!iface.IsInterface) iface = null;
                if (iface != null) return iface;
            }
            return null;
        }


        public static void CreateAssembly(bool autoAssemblyName = false)
        {
            if (Config.header == HeaderType.LIBRARY) Config.outputType = PEFileKinds.Dll;
            else if (Config.header == HeaderType.CONSOLE) Config.outputType = PEFileKinds.ConsoleApplication;
            else if (Config.header == HeaderType.GUI) Config.outputType = PEFileKinds.WindowApplication;
            //else don't change output type in config
            if (autoAssemblyName)
            {
                assemblyName = new AssemblyName
                {
                    Version = Config.version,
                    Name = Config.appName
                };
            }
            assemblyBuilder = AppDomain.CurrentDomain.DefineDynamicAssembly(assemblyName, AssemblyBuilderAccess.RunAndSave);
            moduleBuilder = assemblyBuilder.DefineDynamicModule(Config.FileName);
            if (Config.header == HeaderType.CLASS || Config.header == HeaderType.SCRIPT)
            {
                mainClass = new ClassBuilder(Config.MainClassName, "", ClassType.STATIC, SecurityDegree.PRIVATE);
                if (Config.header == HeaderType.SCRIPT)
                {
                    mainClass.SetAttribute(scriptAttr);
                    mainClass.CreateMethod("Main", "void", FuncType.STATIC, SecurityDegree.PRIVATE);
                    functionBuilder.SetAttribute(scriptAttr);
                    functionBuilder.SetAttribute(entrypointAttr);
                    assemblyBuilder.SetEntryPoint(functionBuilder.methodBuilder.GetBaseDefinition());
                }
            }
        }

        public static void CreateField()
        {
            string typeName = gen.reader.string_values.Peek(), name = gen.reader.string_values.Peek();
            Console.WriteLine(typeName);
            Console.WriteLine(name);
            FieldAttributes fieldAttributes;
            SecurityDegree security = gen.reader.securities.Peek();
            if (security == SecurityDegree.PUBLIC) fieldAttributes = FieldAttributes.Public;
            else if (security == SecurityDegree.PRIVATE) fieldAttributes = FieldAttributes.Private;
            else if (security == SecurityDegree.INTERNAL) fieldAttributes = FieldAttributes.Assembly;
            else fieldAttributes = FieldAttributes.Family;
            VarType varType = gen.reader.var_types.Peek();
            if (varType == VarType.CONST) fieldAttributes |= FieldAttributes.Literal | FieldAttributes.HasDefault | FieldAttributes.Static;
            else if (varType == VarType.FINAL) fieldAttributes |= FieldAttributes.InitOnly;
            else if (varType == VarType.STATIC) fieldAttributes |= FieldAttributes.Static;
            classBuilder.DefineField(name, typeName, fieldAttributes);
        }

        public static FuncType CreateMethod()
        {
            string name = gen.reader.string_values.Peek(), typeName = gen.reader.string_values.Peek();
            FuncType funcType = gen.reader.function_types.Peek();
            SecurityDegree security = gen.reader.securities.Peek();
            classBuilder.CreateMethod(name, typeName, funcType, security);
            return funcType;
        }

        public static CustomAttributeBuilder FindAttribute(IEnumerable<string> namespaces)
        {
            string attributeName = gen.reader.string_values.Peek();
            Type[] ctorTypes = Type.EmptyTypes;
            object[] args = new object[] { };
            if (gen.reader.tokens[0] == TokenType.STATEMENT)
            {
                gen.reader.tokens.RemoveAt(0);
                if (gen.reader.bool_values.Peek())
                {
                    //pass
                }
                else
                {
                    TokensBuilder.Error(new NeedEndError(gen.line, "Extra closing bracket in attribute"));
                }
            }
            return new CustomAttributeBuilder(
                GetTypeByName(attributeName, namespaces).GetConstructor(ctorTypes), args); //it`s a pass
        }

        public static void Finish()
        {
            if (mainClass.IsEmpty)
            {
                MethodInfo method = null;
                foreach (Type type in moduleBuilder.GetTypes())
                {
                    foreach (MethodInfo methodInfo in type.GetMethods())
                    {
                        if (methodInfo.GetCustomAttribute<EntrypointAttribute>() != null)
                        {
                            method = methodInfo;
                            break;
                        }
                    }
                    if (method != null) break;
                }
                try
                {
                    assemblyBuilder.SetEntryPoint(method, Config.outputType);
                }
                catch
                {
                    //error
                }
            }
            else
            {
                mainClass.methodBuilder.generator.Emit(OpCodes.Ret);
                mainClass.End();
            }
            if (Config.header != (HeaderType.BUILDSCRIPT | HeaderType.TOKENSLIBRARY)) assemblyBuilder.Save(Config.FileName);
        }
    }
}
