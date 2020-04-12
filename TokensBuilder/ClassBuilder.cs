using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using TokensAPI;

namespace TokensBuilder
{
    public sealed class ClassBuilder
    {
        public bool IsEmpty => typeBuilder == null;
        public FunctionBuilder methodBuilder = null;
        public FieldBuilder fieldBuilder = null;
        private TypeBuilder typeBuilder;
        public ClassType type = ClassType.DEFAULT;
        public SecurityDegree security = SecurityDegree.PUBLIC;
        public ConstructorBuilder defaultCtor;
        private Generator gen => TokensBuilder.gen;

        public ClassBuilder(string name, string nameSpace = "",
            ClassType classType = ClassType.DEFAULT, SecurityDegree securityDegree = SecurityDegree.PUBLIC)
        {
            type = classType;
            security = securityDegree;
            TypeAttributes typeAttributes = TypeAttributes.Class;
            if (classType == ClassType.DEFAULT) typeAttributes = TypeAttributes.Class;
            else if (classType == ClassType.FINAL) typeAttributes |= TypeAttributes.Sealed;
            else if (classType == ClassType.INTERFACE) typeAttributes = TypeAttributes.Interface;
            if (securityDegree == SecurityDegree.PRIVATE) typeAttributes |= TypeAttributes.NotPublic;
            else if (securityDegree == SecurityDegree.PUBLIC) typeAttributes |= TypeAttributes.Public;
            typeBuilder = Context.moduleBuilder.DefineType(nameSpace + name, typeAttributes);
        }

        public void CreateDefaultConstructorArgs(IEnumerable<TokenType> tokens)
        {
        }

        public void Extends(string superTypeName)
        {
            typeBuilder.SetParent(Context.GetTypeByName(superTypeName, gen.usingNamespaces));
        }

        public void Implements(string interfaceName)
        {
            typeBuilder.AddInterfaceImplementation(Context.GetInterfaceByName(interfaceName, gen.usingNamespaces));
        }

        public Type End()
        {
            Type rt = typeBuilder.CreateType();
            typeBuilder = null;
            return rt;
        }
    }
}
