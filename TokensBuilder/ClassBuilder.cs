using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using TokensAPI;
using TokensBuilder.Errors;

namespace TokensBuilder
{
    public sealed class ClassBuilder
    {
        public bool IsEmpty => typeBuilder == null;
        public bool? actual = null;
        public Dictionary<string, FieldBuilder> constants = new Dictionary<string, FieldBuilder>(),
            finalFields = new Dictionary<string, FieldBuilder>();
        public FunctionBuilder methodBuilder = null;
        public FieldBuilder fieldBuilder = null;
        private TypeBuilder typeBuilder;
        public ClassType type = ClassType.DEFAULT;
        public SecurityDegree security = SecurityDegree.PUBLIC;
        public ConstructorBuilder defaultCtor;
        public PropertyBuilder propertyBuilder;
        public EnumBuilder enumBuilder;
        private Generator gen => TokensBuilder.gen;

        public ClassBuilder(string name, string nameSpace = "",
            ClassType classType = ClassType.DEFAULT, SecurityDegree securityDegree = SecurityDegree.PUBLIC)
        {
            type = classType;
            security = securityDegree;
            TypeAttributes typeAttributes = TypeAttributes.Class;

            //select type of class
            if (classType == ClassType.DEFAULT) typeAttributes = TypeAttributes.Class;
            else if (classType == ClassType.FINAL) typeAttributes |= TypeAttributes.Sealed;
            else if (classType == ClassType.INTERFACE) typeAttributes = TypeAttributes.Interface;
            else if (classType == ClassType.TYPEALIAS) typeBuilder.SetCustomAttribute(Context.typeAliasAttr);

            //select security of class
            if (securityDegree == SecurityDegree.PRIVATE) typeAttributes |= TypeAttributes.NotPublic;
            else if (securityDegree == SecurityDegree.PUBLIC) typeAttributes |= TypeAttributes.Public;

            if (nameSpace.IsEmpty())
                typeBuilder = Context.moduleBuilder.DefineType(name, typeAttributes);
            else
                typeBuilder = Context.moduleBuilder.DefineType(nameSpace + "." + name, typeAttributes);
        }

        internal FunctionBuilder CreateMethod(string name, string typeName = "", FuncType type = FuncType.DEFAULT,
            SecurityDegree security = SecurityDegree.PUBLIC)
        {
            MethodAttributes attributes;

            //function security
            if (security == SecurityDegree.INTERNAL) attributes = MethodAttributes.Assembly;
            else if (security == SecurityDegree.PRIVATE) attributes = MethodAttributes.Private;
            else if (security == SecurityDegree.PROTECTED) attributes = MethodAttributes.Family;
            else attributes = MethodAttributes.Public;

            //function type
            if (type == FuncType.STATIC) attributes |= MethodAttributes.Static;
            else if (type == FuncType.ABSTRACT) attributes |= MethodAttributes.Abstract;
            else if (type == FuncType.FINAL) attributes |= MethodAttributes.Final;
            else if (type == FuncType.VIRTUAL) attributes |= MethodAttributes.Virtual;

            methodBuilder = new FunctionBuilder(typeBuilder.DefineMethod(name, attributes, CallingConventions.Standard));
            return methodBuilder;
        }

        public void Extends(string superTypeName) => typeBuilder.SetParent(Context.GetTypeByName(superTypeName));

        public void Implements(string interfaceName) => typeBuilder.AddInterfaceImplementation(
            Context.GetInterfaceByName(interfaceName, gen.usingNamespaces));

        public void SetAttribute(CustomAttributeBuilder attr) => typeBuilder.SetCustomAttribute(attr);

        /// <summary>
        /// Set fieldBuilder
        /// </summary>
        /// <param name="name">Name of field</param>
        /// <param name="typeName">Name of type of field</param>
        /// <param name="fieldAttributes">Attributes of field</param>
        public void DefineField(string name, string typeName, FieldAttributes fieldAttributes) =>
            fieldBuilder = typeBuilder.DefineField(name, Context.GetTypeByName(typeName), fieldAttributes);

        public Type End()
        {
            Type rt = typeBuilder.CreateType();
            typeBuilder = null;
            return rt;
        }
    }
}
