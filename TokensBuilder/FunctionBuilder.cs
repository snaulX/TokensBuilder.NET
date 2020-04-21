using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using TokensAPI;

namespace TokensBuilder
{
    public sealed class FunctionBuilder
    {
        public bool IsEmpty => methodBuilder == null && constructorBuilder == null;
        public MethodBuilder methodBuilder = null;
        public ConstructorBuilder constructorBuilder = null;
        public FuncType type = FuncType.DEFAULT;
        public Dictionary<string, LocalBuilder> localVariables = new Dictionary<string, LocalBuilder>();
        public ParameterAttributes parameterAttributes = ParameterAttributes.None;
        public ILGenerator generator => constructorBuilder == null ? methodBuilder.GetILGenerator() : constructorBuilder.GetILGenerator();
        private Generator gen => TokensBuilder.gen;

        public void DeclareLocal(string name, string typeName) => 
            localVariables.Add(name, generator.DeclareLocal(Context.GetTypeByName(typeName, gen.usingNamespaces)));

        public FunctionBuilder(MethodBuilder methodBuilder)
        {
            this.methodBuilder = methodBuilder;
        }

        public FunctionBuilder(ConstructorBuilder constructorBuilder)
        {
            this.constructorBuilder = constructorBuilder;
        }

        public void SetAttribute(CustomAttributeBuilder attribute)
        {
            if (constructorBuilder == null)
                methodBuilder.SetCustomAttribute(attribute);
            else
                constructorBuilder.SetCustomAttribute(attribute);
        }
    }
}
