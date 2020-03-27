using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection.Emit;
using System.Reflection;

namespace TokensBuilder
{
    public static class Context
    {
        public static AssemblyBuilder assemblyBuilder = null;
        public static AssemblyName assemblyName = new AssemblyName();
        public static ModuleBuilder moduleBuilder = null;
        public static TypeBuilder typeBuilder = null;
        public static MethodBuilder methodBuilder = null;
        public static FieldBuilder fieldBuilder = null;
        public static ILGenerator generator => constructorBuilder == null ? methodBuilder.GetILGenerator() : constructorBuilder.GetILGenerator();
        public static LocalBuilder localBuilder = null;
        public static Dictionary<string, Label> labels = new Dictionary<string, Label>();
        public static ConstructorBuilder constructorBuilder = null;
        public static EnumBuilder enumBuilder = null;
        public static PropertyBuilder propertyBuilder = null;
        public static ParameterBuilder parameterBuilder = null;
    }
}
