using System;
using System.Collections.Generic;
using System.Reflection.Emit;
using System.Reflection;
using TokensAPI;

namespace TokensBuilder
{
    public static class Context
    {
        public static AssemblyBuilder assemblyBuilder = null;
        public static AssemblyName assemblyName = new AssemblyName();
        public static ModuleBuilder moduleBuilder = null;
        public static ClassBuilder classBuilder = null, mainClass = null;
        public static FunctionBuilder functionBuilder = null;
        public static Dictionary<string, Label> labels = new Dictionary<string, Label>();
        private static Generator gen => TokensBuilder.gen;

        public static Type GetTypeByName(string name, IEnumerable<string> namespaces)
        {
            Type type = null;
            foreach (string nameSpace in namespaces)
            {
                type = Assembly.GetExecutingAssembly().GetType(nameSpace + name);
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
                assemblyName = new AssemblyName();
                assemblyName.Version = Config.version;
                assemblyName.Name = Config.appName;
            }
            assemblyBuilder = AppDomain.CurrentDomain.DefineDynamicAssembly(assemblyName, AssemblyBuilderAccess.RunAndSave);
            moduleBuilder = assemblyBuilder.DefineDynamicModule(Config.appName);
            if (Config.header == HeaderType.CLASS || Config.header == HeaderType.SCRIPT)
            {
                mainClass = new ClassBuilder(Config.MainClassName, "", ClassType.STATIC, SecurityDegree.PRIVATE);
            }
        }

        public static FieldBuilder CreateField()
        {
            FieldAttributes fieldAttributes;
            SecurityDegree security = gen.reader.securities.Peek();
            if (security == SecurityDegree.PUBLIC) fieldAttributes = FieldAttributes.Public;
            else if (security == SecurityDegree.PRIVATE) fieldAttributes = FieldAttributes.Private;
            else if (security == SecurityDegree.INTERNAL) fieldAttributes = FieldAttributes.Assembly;
            else fieldAttributes = FieldAttributes.Family;
            VarType varType = gen.reader.var_types.Peek();
            if (varType == VarType.CONST) fieldAttributes |= FieldAttributes.Literal;
            else if (varType == VarType.FINAL) fieldAttributes |= FieldAttributes.InitOnly;
            else if (varType == VarType.STATIC) fieldAttributes |= FieldAttributes.Static;
            string typeName = gen.reader.string_values.Peek(), name = gen.reader.string_values.Peek();
            return classBuilder.DefineField(name, typeName, fieldAttributes);
        }

        public static void Finish()
        {
            if (Config.header != (HeaderType.BUILDSCRIPT | HeaderType.TOKENSLIBRARY)) assemblyBuilder.Save(Config.appName);
        }
    }
}
