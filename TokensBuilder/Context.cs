using System;
using System.Collections.Generic;
using System.Reflection.Emit;
using System.Reflection;
using TokensAPI;
using TokensBuilder.Errors;
using TokensStandart;
using System.Linq;

namespace TokensBuilder
{
    public static class Context
    {
        #region Propeties and fields
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
        public static bool isFuncBody => functionBuilder != null && !functionBuilder.IsEmpty;
        private static Generator gen => TokensBuilder.gen;
        private static ILGenerator ilg => functionBuilder.generator;
        public static readonly CustomAttributeBuilder entrypointAttr = new CustomAttributeBuilder(
                        typeof(EntrypointAttribute).GetConstructor(Type.EmptyTypes), new object[] { }),
            scriptAttr = new CustomAttributeBuilder(
                        typeof(ScriptAttribute).GetConstructor(Type.EmptyTypes), new object[] { }),
            typeAliasAttr = new CustomAttributeBuilder(
                typeof(TypeAliasAttributte).GetConstructor(Type.EmptyTypes), new object[] { });
        public static Dictionary<string, object> constants = new Dictionary<string, object>();
        public static List<MethodInfo> scriptFunctions = new List<MethodInfo>();
        #endregion

        #region 'Find' methods
        public static MethodInfo FindScriptFunction(string name)
        {
            foreach (MethodInfo func in scriptFunctions)
            {
                if (func.Name == name)
                    return func;
            }
            return null;
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
        #endregion

        #region 'Get' methods
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

        public static FieldInfo GetVarByName(string caller, string name, IEnumerable<string> namespaces)
        {
            return GetTypeByName(caller, namespaces).GetField(name);
        }

        public static FieldInfo GetVarByName(string name, IEnumerable<string> namespaces)
        {
            List<string> literals = name.Split('.').ToList();
            name = literals.Last();
            literals.RemoveAt(literals.Count - 1);
            return GetVarByName(string.Join(".", literals), name, namespaces);
        }

        public static FieldInfo GetVarByName(string caller, string name) => GetVarByName(caller, name, gen.usingNamespaces);

        public static FieldInfo GetVarByName(string name) => GetVarByName(name, gen.usingNamespaces);
        #endregion

        #region 'Create' methods
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
            string typeName = gen.reader.string_values.Peek();
            gen.reader.tokens.RemoveAt(0); // remove name of type
            VarType varType = gen.reader.var_types.Peek();
            SecurityDegree security = gen.reader.securities.Peek();
            FieldAttributes fieldAttributes;
            if (security == SecurityDegree.PUBLIC) fieldAttributes = FieldAttributes.Public;
            else if (security == SecurityDegree.PRIVATE) fieldAttributes = FieldAttributes.Private;
            else if (security == SecurityDegree.INTERNAL) fieldAttributes = FieldAttributes.Assembly;
            else fieldAttributes = FieldAttributes.Family;
            if (varType == VarType.CONST) fieldAttributes |= FieldAttributes.Literal | FieldAttributes.HasDefault | FieldAttributes.Static;
            else if (varType == VarType.FINAL) fieldAttributes |= FieldAttributes.InitOnly;
            else if (varType == VarType.STATIC) fieldAttributes |= FieldAttributes.Static;

            check_var:
            string name = gen.reader.string_values.Peek();
            gen.reader.tokens.RemoveAt(0); // remove name
            classBuilder.DefineField(name, typeName, fieldAttributes);

            TokenType curtoken = gen.reader.tokens.Peek();
            if (curtoken == TokenType.OPERATOR)
            {
                OperatorType optype = gen.reader.operators.Peek();
                if (optype == OperatorType.ASSIGN)
                {
                    gen.parameterTypes.Push(new List<Type>()); // create new parameter types for value
                    curtoken = gen.reader.tokens.Peek();
                    do
                    {
                        gen.ParseToken(curtoken);
                    }
                    while (curtoken != TokenType.SEPARATOR || curtoken != TokenType.EXPRESSION_END);
                    if (gen.parameterTypes.Pop()[0] != GetTypeByName(typeName))
                        TokensBuilder.Error(new TokensError(gen.line, "Type of value not equals type of variable"));
                    functionBuilder.generator.Emit(OpCodes.Stloc, classBuilder.fieldBuilder); // save getted value
                    if (curtoken == TokenType.SEPARATOR)
                    {
                        if (gen.reader.bool_values.Peek())
                            goto check_var;
                        else
                            TokensBuilder.Error(new InvalidTokenError(gen.line, "Literal separator cannot insert in variable declaration"));
                    }
                    else // EXPRESSION_END
                    {
                        return;
                    }
                }
                else
                    TokensBuilder.Error(new InvalidOperatorError(gen.line, $"Operator {optype} cannot be after variable declaration"));
            }
            else if (curtoken == TokenType.EXPRESSION_END)
            {
                return;
            }
            else if (curtoken == TokenType.SEPARATOR)
            {
                if (!gen.reader.bool_values.Peek())
                    goto check_var;
                else
                    TokensBuilder.Error(new InvalidTokenError(gen.line, "Literal separator cannot insert in variable declaration"));
            }
            else
            {
                TokensBuilder.Error(new InvalidTokenError(gen.line, $"Invalid token {curtoken} after variable definition"));
            }
        }

        public static void CreateLocal()
        {
            string typeName = gen.reader.string_values.Peek();
            gen.reader.tokens.RemoveAt(0); // remove name of type
            gen.reader.securities.Peek();
            VarType varType = gen.reader.var_types.Peek();

            check_var:
            string name = gen.reader.string_values.Peek();
            gen.reader.tokens.RemoveAt(0); // remove name
            LocalBuilder local = functionBuilder.DeclareLocal(typeName);
            if (varType == VarType.CONST || varType == VarType.FINAL)
                functionBuilder.localFinals.Add(name, local);
            else if (varType == VarType.DEFAULT)
                functionBuilder.localVariables.Add(name, local);
            else
                TokensBuilder.Error(new InvalidVarTypeError(gen.line, $"Type of variable {varType} is not valid for local variables"));
            TokenType curtoken = gen.reader.tokens.Peek();
            if (curtoken == TokenType.OPERATOR)
            {
                OperatorType optype = gen.reader.operators.Peek();
                if (optype == OperatorType.ASSIGN)
                {
                    gen.parameterTypes.Push(new List<Type>()); // create new parameter types for value
                    curtoken = gen.reader.tokens.Peek();
                    do
                    {
                        gen.ParseToken(curtoken);
                    }
                    while (curtoken != TokenType.SEPARATOR || curtoken != TokenType.EXPRESSION_END);
                    if (gen.parameterTypes.Pop()[0] != GetTypeByName(typeName))
                        TokensBuilder.Error(new TokensError(gen.line, "Type of value not equals type of variable"));
                    functionBuilder.generator.Emit(OpCodes.Stloc, local); // save getted value
                    if (curtoken == TokenType.SEPARATOR)
                    {
                        if (gen.reader.bool_values.Peek())
                            goto check_var;
                        else
                            TokensBuilder.Error(new InvalidTokenError(gen.line, "Literal separator cannot insert in variable declaration"));
                    }
                    else // EXPRESSION_END
                    {
                        return;
                    }
                }
                else
                    TokensBuilder.Error(new InvalidOperatorError(gen.line, $"Operator {optype} cannot be after variable declaration"));
            }
            else if (curtoken == TokenType.EXPRESSION_END)
            {
                return;
            }
            else if (curtoken == TokenType.SEPARATOR)
            {
                if (!gen.reader.bool_values.Peek())
                    goto check_var;
                else
                    TokensBuilder.Error(new InvalidTokenError(gen.line, "Literal separator cannot insert in variable declaration"));
            }
            else
            {
                TokensBuilder.Error(new InvalidTokenError(gen.line, $"Invalid token {curtoken} after variable definition"));
            }
        }

        public static FuncType CreateMethod()
        {
            string name = gen.reader.string_values.Peek(), typeName = gen.reader.string_values.Peek();
            FuncType funcType = gen.reader.function_types.Peek();
            SecurityDegree security = gen.reader.securities.Peek();
            classBuilder.CreateMethod(name, typeName, funcType, security);
            return funcType;
        }
        #endregion

        #region Methods for manipulations with ILGenerator
        public static void LoadObject(object value)
        {
            if (value == null)
                ilg.Emit(OpCodes.Ldnull);
            else if (value is sbyte b)
                ilg.Emit(OpCodes.Ldc_I4_S, b);
            else if (value is short s)
                ilg.Emit(OpCodes.Ldc_I4, s);
            else if (value is int i)
                ilg.Emit(OpCodes.Ldc_I4, i);
            else if (value is float f)
                ilg.Emit(OpCodes.Ldc_R4, f);
            else if (value is long l)
                ilg.Emit(OpCodes.Ldc_I8, l);
            else if (value is double d)
                ilg.Emit(OpCodes.Ldc_R8, d);
            else if (value is bool bl)
            {
                if (bl) ilg.Emit(OpCodes.Ldc_I4_1);
                else ilg.Emit(OpCodes.Ldc_I4_0);
            }
            else if (value is char c)
                ilg.Emit(OpCodes.Ldc_I4, c);
            else if (value is string str)
                ilg.Emit(OpCodes.Ldstr, str);
            else if (value is FieldInfo fld)
                LoadField(fld);
            else if (value is LocalBuilder lcl)
                LoadLocal(lcl);
        }

        public static void CallMethod(MethodInfo method, bool dontPop = true)
        {
            ilg.Emit(OpCodes.Call, method);
            if (!dontPop)
            {
                if (method.ReturnType == typeof(void))
                    ilg.Emit(OpCodes.Nop);
                else
                    ilg.Emit(OpCodes.Pop);
            }
        }

        public static void LoadField(FieldInfo field)
        {
            if (field != null)
                ilg.Emit(OpCodes.Ldfld, field);
            else
                TokensBuilder.Error(new VarNotFoundError(gen.line, "Incorrect field given for load"));
        }

        public static void SetField(FieldInfo field)
        {
            if (field != null)
                ilg.Emit(OpCodes.Stfld, field);
            else
                TokensBuilder.Error(new VarNotFoundError(gen.line, "Incorrect field given for assign"));
        }

        public static void LoadLocal(LocalBuilder local)
        {
            if (local != null)
                ilg.Emit(OpCodes.Ldloc, local);
            else
                TokensBuilder.Error(new VarNotFoundError(gen.line, "Incorrect local given for load"));
        }

        public static void SetLocal(LocalBuilder local)
        {
            if (local != null)
                ilg.Emit(OpCodes.Stloc, local);
            else
                TokensBuilder.Error(new VarNotFoundError(gen.line, "Incorrect local given for assign"));
        }
        #endregion

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
